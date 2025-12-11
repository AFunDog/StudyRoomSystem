<script setup lang="ts">
import MainHeader from './MainHeader.vue'
import MainSidebar from './MainSidebar.vue'
</script>

<template>
  <div class="flex flex-col w-full h-full">
    <!-- 顶部 -->
    <MainHeader />

    <!-- 下面 -->
    <div class="flex flex-row flex-1 min-h-0">
      <!-- 左侧导航条 -->
      <MainSidebar />

      <!-- 右侧主内容 -->
      <div class="flex-1 min-h-0">
        <RouterView v-slot="{ Component }">
          <component :is="Component" class="w-full h-full" />
        </RouterView>
      </div>
    </div>
  </div>
</template>


