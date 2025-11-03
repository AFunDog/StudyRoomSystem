<script setup lang="ts">
import { cn } from '@/lib/utils';
import { Armchair, House } from 'lucide-vue-next';
import { inject, ref } from 'vue';
import MainSelectRoom from './MainSelectRoom.vue';
import { SELECT_ROOM } from './define';
import { isValid } from 'zod';
import MainSelectSeat from './MainSelectSeat.vue';
import MainMyBooking from './MainMyBooking.vue';

// const count = ref(10);
const selectRoom = inject(SELECT_ROOM);


</script>
<template>
  <div class="px-4 flex flex-col gap-y-4 items-center justify-center h-full  max-w-full">
    <MainSelectRoom />
    <MainSelectSeat class="flex-5"/>
    <MainMyBooking class="flex-1"/>
  </div>
</template>