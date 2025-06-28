import express from "express";
import cors from "cors";
import fs from "fs";
import path from 'path';
import crypto from "crypto";
import { createClient } from "redis";
import { LedgerCanister } from "@dfinity/ledger-icp";
import { createAgent } from "@dfinity/utils";
import { Ed25519KeyIdentity } from "@dfinity/identity";
import { AccountIdentifier } from "@dfinity/ledger-icp";
import { SubAccount  } from "@dfinity/ledger-icp";
import { Principal } from '@dfinity/principal';

// Load configuration
const disConfig = JSON.parse(fs.readFileSync("disConfig.json", "utf8"));
// Transfer remaining amount to secondary account
const dstPrincipal = disConfig["dstPrincipal"];
// Transfer ratio
const transferPerArr = [1,1,1];
// Transfer fee
const transferFee = BigInt(10000);

// Connect to redis
const redisClient = createClient({
  socket: {
    host: '127.0.0.1',  // Force IPv4
    port: 6379
  }
});
await redisClient.connect();
// Current week id
const curWeekIdValue = await redisClient.get("curWeekId");
if (curWeekIdValue === null) {
	console.log(`curWeekIdValue not exist`);
	process.exit();
}
let curWeekId = parseInt(curWeekIdValue);

// Get and generate keys
const KEY_DIR = './.secrets';
const PROTO = path.join(KEY_DIR, 'proto');
const ensureKeyExists = () => {
  try {
    // Create key storage directory (if it doesn't exist)
    if (!fs.existsSync(KEY_DIR)) {
      fs.mkdirSync(KEY_DIR, { recursive: true, mode: 0o700 });
    }

    // Try to load existing key
    if (fs.existsSync(PROTO)) {
      const pemContent = fs.readFileSync(PROTO, 'utf8');
      return Ed25519KeyIdentity.fromParsedJson(JSON.parse(pemContent));
    }

    // Generate new key and store
    const newIdentity = Ed25519KeyIdentity.generate();
    fs.writeFileSync(
      PROTO,
      JSON.stringify(newIdentity.toJSON()), // Convert to storable JSON format
      { mode: 0o600 } // Set file permissions to owner read/write only
    );
    
    console.log(`New merchant key generated at: ${PROTO}`);
    return newIdentity;
  } catch (error) {
    console.error('Key management error:', error);
    throw new Error('Failed to initialize merchant identity');
  }
};
const merchantIdentity = ensureKeyExists();

// Initialize identity and agent
const agent = await createAgent({
  identity: merchantIdentity,
  host: "https://ic0.app"
});
const ledger = LedgerCanister.create({
  agent,
  canisterId: "ryjl3-tyaaa-aaaaa-aaaba-cai"
});

// Get top 3 members of the leaderboard
const getTopMembers = async (redisClient, num = 3) => {
  try {
    // Get sorted set members (descending by score)
    const members = await redisClient.zRange("ddcr_rank_week_sort", 0, num - 1, { REV: true });
    const list = [];
    
    for (const id of members) {
      if (!id.includes('_')) continue;
      
      const parts = id.split('_');
      const pId = parts[0];
      list.push(pId);
      
      if (list.length >= num) break;
    }
    
    return list;
  } catch (err) {
    console.error('Redis operation failed:', err);
    return []; // Return empty array on error
  }
};
const topMembers = await getTopMembers(redisClient);
if (topMembers.length == 0){
	console.log(`topMembers empty`);
	process.exit();
}

// Collect sub-account balances to main account
// Main account address
const mainAddr = await AccountIdentifier.fromPrincipal({
	principal: merchantIdentity.getPrincipal()
  });
const allPay = await redisClient.hGetAll("pay_info");
for (const [subAccount, value] of Object.entries(allPay)) {
	const info = JSON.parse(value);
	if (info.status !== "over") {
		continue;
	}
	if (info.curWeekId !== curWeekId){
		continue;
	}
	let money = BigInt(info.money*1e8);
	if (money <= transferFee){
		continue;
	}
	money -= transferFee;
	const fromSubAccount = subAccount.split(',').map(Number);
	await ledger.transfer({
      to: mainAddr,
      amount: money,
      fromSubAccount: fromSubAccount
    });
	console.log(`sub to main ${subAccount} ${money}`);
}
// Transfer main account balance to top 3 according to ratio
// Query main account balance
let balance = await ledger.accountBalance({
	accountIdentifier: mainAddr,
});
console.log(`main balance ${balance}`);
// Remove fee
const allFree = transferFee * BigInt(topMembers.length+1);
if (balance < allFree){
	console.log(`main balance not enough ${balance}`);
	process.exit();
}
balance -= BigInt(allFree);
let toMoney = BigInt(0);
// Transfer amount to top 3
for(let i = 0; i < topMembers.length && i < transferPerArr.length; i++) {
	const to = await AccountIdentifier.fromPrincipal({
		principal: Principal.fromText(topMembers[i])
	  });
	const money = balance*BigInt(transferPerArr[i])/100n;
	await ledger.transfer({
      to: to,
      amount: money
    });
	toMoney += money;
	console.log(`transfer to ${topMembers[i]} ${money}`);
	const transferInfo = {
        principal: topMembers[i],
    	ts: Date.now(),
    	money: money
      };
    await redisClient.hSet("transferInfo", topMembers[i], JSON.stringify(transferInfo));
}
balance -= toMoney;
// Transfer remaining amount to target account
const to = await AccountIdentifier.fromPrincipal({
	principal: Principal.fromText(dstPrincipal)
  });
await ledger.transfer({
    to: to,
    amount: balance
  });
console.log(`final transfer to ${dstPrincipal} ${balance}`);
// Update week id
curWeekId += 1;
await redisClient.set("curWeekId", curWeekId.toString());
await redisClient.quit();
process.exit();