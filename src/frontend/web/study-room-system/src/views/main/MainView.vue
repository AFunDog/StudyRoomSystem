<script setup lang="ts">
import MainHeader from './MainHeader.vue'
import MainContent from './MainContent.vue'
import MainBottom from './MainBottom.vue'
import { provide, ref, watch } from 'vue';
import type { Room } from '@/lib/types/room';
import { SELECT_ROOM } from './define';
import BottomTag from '@/components/ui/bottomTag/BottomTag.vue';

const selectRoom = ref<Room | null>(null);

watch(selectRoom, (room) => {
  console.log('selectRoom', room);
});


provide(SELECT_ROOM, selectRoom);
</script>

<template>

  <!-- 子元素设置 min-w-0 能解决宽度溢出的问题 -->
  <div class="grid grid-rows-[auto_1fr_auto] w-full h-full gap-y-4 *:min-w-0">
    <MainHeader />
    <MainContent />
    <!-- <MainBottom /> -->
    <BottomTag />
  </div>
</template>