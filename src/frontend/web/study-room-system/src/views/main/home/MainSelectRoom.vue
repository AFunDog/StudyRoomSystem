<script setup lang="ts">
import Button from '@/components/ui/button/Button.vue';
import { inject, ref } from 'vue';
import dayjs from 'dayjs';
import type { Room } from '@/lib/types/room';
import { http } from '@/lib/utils';
import { SELECT_ROOM } from '../define';


const rooms = ref<Room[]>([]);
const selectRoom = inject(SELECT_ROOM);

async function getRooms() {
    rooms.value = await http.get('/room').then(res => res.data);
}

getRooms();

</script>
<template>
  <div class="h-fit w-full min-w-0">
    <div class="flex bg-accent h-full w-full rounded-xl p-2 px-4 min-w-0">
      <div class="h-full max-w-full w-full flex flex-row flex-nowrap items-center gap-x-2 overflow-x-auto min-w-0">
        <div v-for="room in rooms">
          <Button :variant="selectRoom?.id == room.id ? 'default' : 'outline'" class="h-full hover:cursor-pointer" @click="selectRoom = room">
            {{ room.name }}
          </Button>

        </div>
      </div>
    </div>
  </div>
</template>