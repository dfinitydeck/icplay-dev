import { IcrcLedgerCanister } from "@dfinity/ledger-icrc";
import { HttpAgent } from "@dfinity/agent";
import { Principal } from "@dfinity/principal";

// Initialize Agent (connect to mainnet)
const agent = new HttpAgent({ 
  host: "https://ic0.app" 
});

// Initialize ICRC-1 Ledger Canister (example uses ICP mainnet contract)
const ledger = IcrcLedgerCanister.create({
  agent,
  canisterId: "ryjl3-tyaaa-aaaaa-aaaba-cai"
});

async function getBalance(principalText) {
  try {
    const principal = Principal.fromText(principalText);
    const balance = await ledger.icrc1_balance_of({
      owner: principal,
      subaccount: [] // Leave empty array if no subaccount
    });
    
    // Convert to actual amount (assuming token precision is 8 decimal places)
    const displayBalance = Number(balance) / 1e8;
    console.log(`Balance: ${displayBalance}`);
    return balance;
  } catch (error) {
    console.error("Query failed:", error);
    throw error;
  }
}

// Usage example (replace with actual Principal)
// getBalance("");

