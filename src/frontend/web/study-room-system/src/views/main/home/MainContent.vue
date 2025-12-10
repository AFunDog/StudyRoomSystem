<script setup lang="ts">
import { cn } from '@/lib/utils';
import { Armchair, House } from 'lucide-vue-next';
import { inject, ref } from 'vue';
import MainSelectRoom from './MainSelectRoom.vue';
import { SELECT_ROOM } from './define';
import { isValid } from 'zod';
import MainSelectSeat from './MainSelectSeat.vue';
import MainMyBooking from './MainMyBooking.vue';
import MainSelectDate from './MainSelectDate.vue';

// const count = ref(10);
const selectRoom = inject(SELECT_ROOM);


</script>
<template>
  <div class="flex flex-col gap-y-4 justify-center h-full w-full">
    <MainSelectDate />
    <MainSelectSeat class="flex-6" />
  </div>
</template>
