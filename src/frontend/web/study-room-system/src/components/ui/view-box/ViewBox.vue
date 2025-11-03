<template>
  <div ref="container" class="h-full relative overflow-hidden">
    <div ref="content" class="absolute w-fit h-fit left-1/2 top-1/2 overflow-hidden" :style="{
      transform: `translate(-50%, -50%) scale(${scale})`,
    }">
      <slot ref="contentSolt"/>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount } from 'vue'

// const designWidth = 1920
// const designHeight = 1080
let containerObserver: ResizeObserver | null = null
let contentObserver: ResizeObserver | null = null
const container = ref<HTMLDivElement | null>(null)
const content = ref<HTMLDivElement | null>(null)

const scale = ref(1)

const updateScale = () => {
  const { width, height } = container.value?.getBoundingClientRect()!;
  const rect = content.value?.getBoundingClientRect();
  if (!rect) return;
  var res = Math.min(width / (rect.width / scale.value),  height / (rect.height / scale.value));
  if(!isFinite(res) || isNaN(res) || scale.value == 0) return;
  scale.value = res;
  console.log('updateScale', width, height, rect.width, rect.height,res,scale.value)
}

onMounted(() => {
  if (container.value) {
    containerObserver = new ResizeObserver(() => updateScale())
    containerObserver.observe(container.value)
  }
  if(content.value){
    contentObserver = new ResizeObserver(() => updateScale())
    contentObserver.observe(content.value)
  }
  // window.addEventListener('resize', updateScale)
})
onBeforeUnmount(() => {
  containerObserver?.disconnect()
  contentObserver?.disconnect()
  // window.removeEventListener('resize', updateScale)
})
</script>