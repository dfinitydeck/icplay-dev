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
const payConfig = JSON.parse(fs.readFileSync("payConfig.json", "utf8"));

const redisClient = createClient({
  socket: {
    host: '127.0.0.1',  // Force IPv4
    port: 6379
  }
});

await redisClient.connect();

// Initialize identity and agent
// Configure key path
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
const myPrincipal = "xxx";

const agent = await createAgent({
//  identity: merchantIdentity,
  host: "https://ic0.app"
});
const ledger = LedgerCanister.create({
  agent,
  canisterId: "xxx"
});

const app = express();
app.use(express.json());
app.use(cors());
// 1: Create recharge request
app.get("/pay/create_order", async (req, res) => {
  const { payId ,principal, sign} = req.query;
  const config = payConfig[payId];

  if (!config) {
    return res.json({code:-1,msg:"payId not exist"});
  }
  const ddcrKey = await redisClient.get("ddcr_signKey");
  const strsign = payId+principal+ddcrKey;
  const mySign = crypto.createHash('md5').update(strsign).digest('hex');
  if (mySign != sign){
      console.log(`strsign: ${strsign} mySign: ${mySign}`);
      return res.json({code:-1,msg:"sign error"});
  }
  
  // Get unique orderId
  const orderId = await redisClient.incr("ddrc_orderId");

  // Generate sub-account and address
  const subAccount = SubAccount.fromID(orderId);
  const address = await AccountIdentifier.fromPrincipal({
    principal: Principal.fromText(myPrincipal),
    subAccount
  });
  const subArr = Array.from(subAccount.toUint8Array());
  const strsubAccount = subArr.join(',');
  
  console.log(`create order principal:${principal} payId: ${payId} subAccount: ${strsubAccount} address: ${address.toHex()}`);
  // Write pay_info
  const payInfo = {
    principal,
    subAccount: strsubAccount,
    address: address.toHex(),
    payId,
    status: "init",
    retries: 0,
	ts: Date.now(),
	money: config.money
  };
  await redisClient.hSet("pay_info", payInfo.subAccount, JSON.stringify(payInfo));

  res.json({ code:0,msg:"", subAccount: strsubAccount, money: config.money ,address:address.toHex()});
});

// 2: Confirm recharge
app.get("/pay/confirm_payment", async (req, res) => {
  const { subAccount,sign } = req.query;

  // Verify sign
  const ddcrKey = await redisClient.get("ddcr_signKey");
  const strsign = subAccount+ddcrKey;
  const mySign = crypto.createHash('md5').update(strsign).digest('hex');
  if (mySign != sign){
      console.log(`strsign: ${strsign} mySign: ${mySign}`);
      return res.json({code:-1,msg:"sign error"});
  }
  
  // Find order data
  const payInfoStr = await redisClient.hGet("pay_info", subAccount);
  if (!payInfoStr) {
      console.log(`cant find order subAccount: ${subAccount}`);
      return res.json({ code:-1,msg:"Order not found"});
  }
  const payInfo = JSON.parse(payInfoStr);
  if (payInfo.status !== "init") {
    console.log(`order not init subAccount: ${subAccount} status: ${payInfo.status}`);
    return res.json({ code:-1,msg:"Order already processed"});
  }

  // Query balance
  const balance = await ledger.accountBalance({
    accountIdentifier: AccountIdentifier.fromHex(payInfo.address),
  });
  
  // Compare if payment received
  const config = payConfig[payInfo.payId];
  const configMoney = config.money*1e8;
  
  if (balance < BigInt(configMoney)) {
    console.log(`order pending subAccount: ${subAccount} balance: ${balance} configMoney: ${configMoney}`);
    payInfo.status = "pending";
    payInfo.retries = 1;
    await redisClient.hSet("pay_info", subAccount, JSON.stringify(payInfo));
    return res.json({ code:-1,msg:"balance not enough" });
  }
  console.log(`order over subAccount: ${subAccount} balance: ${balance} configMoney: ${configMoney}`);
  // Grant reward logic
  await grantReward(payInfo.principal, config.count);
  payInfo.status = "over";
  await redisClient.hSet("pay_info", subAccount, JSON.stringify(payInfo));
  res.json({ code:0,msg:"" });
});

// Grant reward function
async function grantReward(principal, count) {
  const playerInfoStr = await redisClient.hGet("ddcr_player_info", principal);
  let playerInfo = playerInfoStr ? JSON.parse(playerInfoStr) : { count: 0 };
  playerInfo.count = (playerInfo.count || 0) + count;
  await redisClient.hSet("ddcr_player_info", principal, JSON.stringify(playerInfo));
}

// Scheduled task checks pending status every 5 seconds
setInterval(async () => {
  const all = await redisClient.hGetAll("pay_info");
  for (const [subAccount, value] of Object.entries(all)) {
    const info = JSON.parse(value);
    if (info.status === "pending" && info.retries < 3) {
      const balance = await ledger.accountBalance({ accountIdentifier: AccountIdentifier.fromHex(info.address) });
      const config = payConfig[info.payId];

      const configMoney = config.money*1e8;
      if (balance >= BigInt(configMoney)) {
        await grantReward(info.principal, config.count);
        info.status = "over";
        console.log(`order set over subAccount: ${subAccount} balance: ${balance} configMoney: ${configMoney}`);
      } else {
        info.retries += 1;
        console.log(`order pending subAccount: ${subAccount} balance: ${balance} configMoney: ${configMoney} retries: ${info.retries}`);
      }

      await redisClient.hSet("pay_info", subAccount, JSON.stringify(info));
    }
  }
}, 5000);

app.listen(3000, () => console.log("Server running on port 3000"));