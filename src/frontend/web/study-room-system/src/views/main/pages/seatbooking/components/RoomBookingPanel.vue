<script setup lang="ts">
import { computed } from 'vue'
import type { Room } from '@/lib/types/Room'
import TimeSelectorBar from './TimeSelectorBar.vue'
import SeatGrid from './SeatGrid.vue'

const props = defineProps<{
  room: Room
  timeRange?: { start: string; end: string } | null
}>()

const emit = defineEmits<{
  (e: 'update:timeRange', value: { start: string; end: string } | null): void
}>()

// 用 computed 包一层，方便直接 v-model
const innerTimeRange = computed({
  get: () => props.timeRange ?? null,
  set: (v) => emit('update:timeRange', v),
})
</script>

<template>
  <div class="flex flex-col gap-4 h-full min-h-0">
    <!-- 上面：时间选择条 -->
    <TimeSelectorBar v-model="innerTimeRange" :room="room"/>

    <!-- 下面：座位网格 -->
    <SeatGrid
      class="flex-1 min-h-0"
      :room="room"
      :time-range="innerTimeRange"
    />
  </div>
</template>
