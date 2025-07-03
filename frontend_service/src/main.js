/* global BUILD_TIME GIT_VERSION */
import './assets/main.scss'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'
import * as gamebox from '@/game-context/index.js'
window.gamebox = Object.assign(window.gamebox || {}, gamebox)

import App from './App.vue'
import router from './router'

const app = createApp(App)
const pinia = createPinia()
pinia.use(piniaPluginPersistedstate)
app.use(pinia)
app.use(router)

app.mount('#app')
console.log(`BUILD_TIME ${BUILD_TIME}`)
console.log(`GIT_VERSION ${GIT_VERSION}`)
