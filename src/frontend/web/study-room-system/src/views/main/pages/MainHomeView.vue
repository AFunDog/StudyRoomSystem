<script setup lang="ts">
import { provide, ref, watch } from 'vue'
import type { Room } from '@/lib/types/Room'
import { SELECT_ROOM } from '../home/define'
import MainSelectRoom from '../home/MainSelectRoom.vue'
import MainContent from '../home/MainContent.vue'

const selectRoom = ref<Room | null>(null)

watch(selectRoom, (room) => {
  console.log('selectRoom', room)
})

provide(SELECT_ROOM, selectRoom)
</script>

<template>
  <div class="flex flex-col md:flex-row w-full h-full gap-4 px-4 py-4 *:min-w-0">
    <div class="w-full md:w-56 md:min-w-48">
      <MainSelectRoom />
    </div>

    <div class="flex-1 min-w-0">
      <MainContent />
    </div>
  </div>
</template>

