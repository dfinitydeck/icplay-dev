import { Actor, HttpAgent, createIdentityDescriptor } from '@dfinity/agent'
import { AuthClient } from '@dfinity/auth-client'
import LocalStorage from '@/canisters/LocalStorage'
import { createAgent } from '@dfinity/utils'

export function getClient() {
  return AuthClient.create({
    storage: LocalStorage,
    keyType: 'Ed25519',
    idleOptions: {
      disableIdle: true // Disable IdleManager
    }
  })
}

export async function createActor(canisterId, idlFactory) {
  const authClient = await getClient()
  const identity = await authClient.getIdentity()
  return Actor.createActor(idlFactory, {
    agent: HttpAgent.createSync({
      host: 'https://icp0.io/',
      retryTimes: 3,
      identity
    }),
    canisterId
  })
}

export async function getDescriptor() {
  const authClient = await getClient()
  const identity = await authClient.getIdentity()
  const descriptor = createIdentityDescriptor(identity)
  return descriptor
}

export async function getAgent() {
  const authClient = await getClient()
  const identity = await authClient.getIdentity()
  return createAgent({
    identity,
    host: 'https://icp0.io/'
  })
}

export async function getIdentity() {
  const authClient = await getClient()
  const identity = await authClient.getIdentity()
  return identity
}
