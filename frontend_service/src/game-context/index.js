import { getClient } from '@/canisters/connect'
import { createOrder, confirmPayment } from '@/api'
import { transferToPrincipal, queryAccountBalance } from '@/canisters/ledger/api'

export function login() {
  return connect()
}

function connect() {
  return new Promise((resolve, reject) => {
    getClient()
      .then((authClient) => {
        authClient.login({
          // Valid for 7 days
          maxTimeToLive: window.BigInt(7 * 24 * 60 * 60 * 1000 * 1000 * 1000),
          // derivationOrigin: import.meta.env.VITE_DEPLOY_CANISTER_URL,
          onSuccess: async () => {
            const identity = await authClient.getIdentity()
            const principal = identity.getPrincipal().toString()
            localStorage.setItem('principal', principal)
            resolve(principal.toString())
          },
          onError: (error) => {
            reject(error)
          }
        })
      })
      .catch(reject)
  })
}

export async function buy(packId) {
  const order = await createOrder({
    payId: packId,
    principal: getPrincipal()
  })

  try {
    await transferToPrincipal(order.address, order.money * 10 ** 8)
    const res = await confirmPayment(order.subAccount)
    return res
  } catch (error) {
    console.log('error', error)
    return false
  }
}

