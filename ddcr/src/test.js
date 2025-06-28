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

//Get and generate keys
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

//Initialize identity and agent
const agent = await createAgent({
  identity: merchantIdentity,
  host: "https://ic0.app"
});
const ledger = LedgerCanister.create({
  agent,
  canisterId: "ryjl3-tyaaa-aaaaa-aaaba-cai"
});

const addr = AccountIdentifier.fromHex("xxxxxxxxxxxxxxxxxxxxxxxxxxxx");
//Query main account balance
const balance1 = await ledger.accountBalance({
	accountIdentifier: addr,
});
console.log(`before balance ${balance1}`);
await ledger.transfer({
      to: addr,
      amount: 10000,
//      fromSubAccount: []
    });

//Query main account balance
const balance2 = await ledger.accountBalance({
	accountIdentifier: addr,
});
console.log(`after balance ${balance2}`);
process.exit();