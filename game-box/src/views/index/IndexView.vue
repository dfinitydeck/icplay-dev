<template>
  <div class="page-box">
    <canvas
      id="unity-canvas"
      width="750"
      height="1624"
      tabindex="-1"
      style="width: 46vh; height: 100vh; background: #231f20"
    ></canvas>
    <div class="loader-box" v-if="isLoading">
      <span class="loader"></span>
    </div>
  </div>
</template>
<script setup>
import { ref, onMounted } from 'vue'
const isLoading = ref(true)

window.gamebox.onLoaded = () => {
  isLoading.value = false
}

onMounted(() => {
  if (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent)) {
    // Mobile device style: fill the whole browser client area with the game canvas:
    const meta = document.createElement('meta')
    meta.name = 'viewport'
    meta.content =
      'width=device-width, height=device-height, initial-scale=1.0, user-scalable=no, shrink-to-fit=yes'
    document.getElementsByTagName('head')[0].appendChild(meta)
    const canvas = document.querySelector('#unity-canvas')
    canvas.style.height = '100%'
    canvas.style.aspectRatio = '750/1624'
    canvas.style.position = 'fixed'
    document.body.style.textAlign = 'left'
  }

  window
    .createUnityInstance(document.querySelector('#unity-canvas'), {
      dataUrl: 'Build/Build.data',
      frameworkUrl: 'Build/Build.framework.js',
      codeUrl: 'Build/Build.wasm',
      streamingAssetsUrl: 'StreamingAssets',
      companyName: 'DefaultCompany',
      productName: 'CompareTheSize',
      productVersion: '0.1'
      // matchWebGLToCanvasSize: false // Uncomment this to separately control WebGL canvas render size and DOM element size.
      // devicePixelRatio: 1, // Uncomment this to override low DPI rendering on high DPI displays.
    })
    .then(function (instance) {
      window.unityInstance = instance // Save instance
      console.log('Unity loaded successfully')
      // Initialize JS-Unity communication here
    })
    .catch(function (message) {
      console.error('Unity loading failed', message)
    })
})
</script>

<style lang="scss" scoped>
.page-box {
  width: 100%;
  height: 100%;
  overflow: hidden;
  display: flex;
  justify-content: center;
  align-items: center;
  background: url('./img/repeated.png') repeat;

  .loader-box {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    display: flex;
    justify-content: center;
    align-items: center;
    background-color: rgba(0, 0, 0, 0.7);
  }

  .loader {
    width: 48px;
    height: 48px;
    border-radius: 50%;
    display: inline-block;
    position: relative;
    border: 3px solid;
    border-color: #fff #fff transparent;
    box-sizing: border-box;
    animation: rotation 1s linear infinite;
  }
  .loader::after {
    content: '';
    box-sizing: border-box;
    position: absolute;
    left: 0;
    right: 0;
    top: 0;
    bottom: 0;
    margin: auto;
    border: 3px solid;
    border-color: transparent #ff3d00 #ff3d00;
    width: 24px;
    height: 24px;
    border-radius: 50%;
    animation: rotationBack 0.5s linear infinite;
    transform-origin: center center;
  }
}

@keyframes rotation {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}

@keyframes rotationBack {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(-360deg);
  }
}
</style>
