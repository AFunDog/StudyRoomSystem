<script setup lang="ts">
import MainShowDate from '../../../nouseview/MainShowDate.vue'
import MainSelectSeat from './selectseat/MainSelectSeat.vue'

const props = defineProps<{
  timeRange: { start: string; end: string } | null
}>()
</script>

<template>
  <div class="flex flex-col gap-y-4 justify-center h-full w-full">
    <!-- <MainShowDate /> -->
    <MainSelectSeat class="flex-6" :time-range="timeRange" />
  </div>
</template>
