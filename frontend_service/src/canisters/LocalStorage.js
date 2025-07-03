export default {
  get(key) {
    return Promise.resolve(window.localStorage.getItem(key))
  },
  set(key, value) {
    window.localStorage.setItem(key, value)
    return Promise.resolve()
  },
  remove(key) {
    window.localStorage.removeItem(key)
    return Promise.resolve()
  }
}
