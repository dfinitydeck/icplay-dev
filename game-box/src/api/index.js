import request from './request'
import md5 from 'md5'
import { loadConfig } from '@/config'
let key = null
loadConfig('key').then((res) => {
  key = res
})

// Create order
export function createOrder({ payId, principal }) {
  return request.get('/pay/create_order', {
    params: {
      payId,
      principal,
      sign: md5(`${payId}${principal}${key}`)
    }
  })
}

// Notify payment success
export function confirmPayment(subAccount) {
  return request.get('/pay/confirm_payment', {
    params: {
      subAccount,
      sign: md5(`${subAccount}${key}`)
    }
  })
}
