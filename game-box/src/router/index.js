import { createRouter, createWebHashHistory } from 'vue-router'

import IndexView from '@/views/index/IndexView.vue'
const router = createRouter({
  history: createWebHashHistory(),
  routes: [
    {
      path: '/',
      name: 'root',
      redirect: '/index'
    },
    {
      path: '/index',
      name: 'index',
      component: IndexView
    }
  ]
})

export default router
