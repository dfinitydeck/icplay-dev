import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import { Principal } from '@dfinity/principal'
import { AccountIdentifier } from '@dfinity/ledger-icp'
import { login as loginApi, profile } from '@/canisters/user/api.js'
import { getClient } from '@/canisters/connect.js'

export const useUserStore = defineStore(
  'user',
  () => {
    const store = ref({})
    const spaceId = computed(() => {
      return Principal.fromText(store.value.spaces[0].space_id)
    })
    const spaceIdString = computed(() => {
      return store.value.spaces[0].space_id
    })
    const ossId = computed(() => {
      return store.value.spaces[0].oss_id.map((oss) => {
        return Principal.fromText(oss)
      })
    })
    const lastOssId = computed(() => {
      return Principal.fromText(store.value.spaces[0].oss_id.slice(-1)[0])
    })
    const pid = computed(() => {
      return Principal.fromText(store.value.pid)
    })

    const aid = computed(() => {
      const accountId = AccountIdentifier.fromPrincipal({
        principal: pid.value
      })
      return accountId.toHex()
    })

    const shortPid = computed(() => {
      let r = ''
      if (store.value.pid) {
        r = store.value.pid.slice(0, 5) + '...' + store.value.pid.slice(-3)
      }
      return r
    })

    async function login() {
      const res = await loginApi()
      const userinfo = {
        ...res,
        music_content_type: res.music_content_type[0]
          ? Object.keys(res.music_content_type[0])[0]
          : null,
        updated_at: Number(res.updated_at),
        pid: res.pid.toString(),
        created: Number(res.created),
        spaces: res.spaces.map((item) => {
          item.oss_id = item.oss_id.map((oss) => {
            return oss.toString()
          })
          item.space_id = item.space_id.toString()
          return item
        })
      }
      return (store.value = userinfo)
    }

    function logout() {
      window.localStorage.clear()
      getClient().then((authClient) => {
        authClient.logout()
      })
      return (store.value = {})
    }

    async function refresh() {
      const res = await profile()
      const userinfo = {
        ...res,
        music_content_type: res.music_content_type[0]
          ? Object.keys(res.music_content_type[0])[0]
          : null,
        updated_at: Number(res.updated_at),
        pid: res.pid.toString(),
        created: Number(res.created),
        spaces: res.spaces.map((item) => {
          item.oss_id = item.oss_id.map((oss) => {
            return oss.toString()
          })
          item.space_id = item.space_id.toString()
          return item
        })
      }
      return (store.value = userinfo)
    }

    return {
      store,
      login,
      logout,
      refresh,
      pid,
      aid,
      shortPid,
      spaceId,
      spaceIdString,
      ossId,
      lastOssId
    }
  },
  {
    persist: true
  }
)
