import { fileURLToPath, URL } from 'node:url'
import { execSync } from 'child_process'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

const getGitVersion = () => {
  try {
    return execSync('git rev-parse --short HEAD').toString().trim()
  } catch (e) {
    return 'Unknown Version'
  }
}

const now = new Date()
// https://vitejs.dev/config/
export default defineConfig({
  base: './',
  define: {
    BUILD_TIME: JSON.stringify(
      `${now.getFullYear()}-${now.getMonth() + 1}-${now.getDate()} ${now.getHours()}:${now.getMinutes()}:${now.getSeconds()}`
    ),
    GIT_VERSION: JSON.stringify(getGitVersion())
  },
  plugins: [vue()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  server: {
    host: '0.0.0.0',
    port: 5173,
    proxy: {
      '/api': {
        target: 'https://icp0.io/',
        changeOrigin: true
      },
      '/pay': {
        target: 'https://pycr.ddecks.xyz',
        changeOrigin: true
        // rewrite: path => path.replace(/^\/api/, '')
      }
    }
  }
})
