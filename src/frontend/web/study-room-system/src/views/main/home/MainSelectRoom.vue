<script setup lang="ts">
import Button from '@/components/ui/button/Button.vue'
import { inject, ref } from 'vue'
import type { Room } from '@/lib/types/Room'
import { http } from '@/lib/utils'
import { SELECT_ROOM } from './define'

const rooms = ref<Room[]>([])

const selectRoom = inject(SELECT_ROOM) as any

async function getRooms() {
  rooms.value = await http.get('/room').then(res => res.data)
}
getRooms()
</script>

<template>
  <div class="w-full min-w-0">
    <!-- 手机端 -->
    <div class="block md:hidden h-fit w-full">
      <div class="bg-accent w-full rounded-xl py-2">
        <div
          class="flex flex-row flex-nowrap items-center gap-x-2 overflow-x-auto px-4 min-w-0"
        >
          <div v-for="room in rooms" :key="room.id">
            <Button
              :variant="selectRoom?.id == room.id ? 'default' : 'outline'"
              class="flex-shrink-0 px-4 hover:cursor-pointer"
              @click="selectRoom = room"
            >
              {{ room.name }}
            </Button>
          </div>
        </div>
      </div>
    </div>

    <!-- 电脑端 -->
    <div class="hidden md:block h-full w-full">
      <div class="bg-accent h-full w-full rounded-xl p-3 min-w-0">
        <div
          class="grid grid-cols-3 gap-2 max-h-64 overflow-y-auto"
        >
          <div v-for="room in rooms" :key="room.id">
            <Button
              :variant="selectRoom?.id == room.id ? 'default' : 'outline'"
              class="w-full hover:cursor-pointer"
              @click="selectRoom = room"
            >
              {{ room.name }}
            </Button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
