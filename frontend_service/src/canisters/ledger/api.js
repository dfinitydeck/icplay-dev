import { Principal } from '@dfinity/principal'
import { getAgent } from '../connect'
import { LedgerCanister, AccountIdentifier } from '@dfinity/ledger-icp'

/**
 * @param {Uint8Array} to
 * @param {number} amount
 * @param {number} subaccount
 * @returns
 */
export async function transferToPrincipal(address, amount) {
  const to = AccountIdentifier.fromHex(address)

  const agent = await getAgent()
  const { transfer } = LedgerCanister.create({
    agent,
    canisterId: import.meta.env.VITE_CANISTER_LEDGER
  })

  return transfer({
    to,
    amount
  })
}

export async function queryAccountBalance(principal) {
  const account = AccountIdentifier.fromPrincipal({
    principal: Principal.fromText(principal),
    subaccount: []
  })

  const agent = await getAgent()
  const { accountBalance } = LedgerCanister.create({
    agent,
    canisterId: import.meta.env.VITE_CANISTER_LEDGER
  })

  return accountBalance({
    accountIdentifier: account
  })
}
