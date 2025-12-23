<script setup lang="ts">
import { computed } from 'vue'
import dayjs from 'dayjs'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import type { Room } from '@/lib/types/Room'
import { localizeState, type Booking } from '@/lib/types/Booking'

const props = defineProps<{
  open: boolean
  room: Room | null
  bookings: Booking[]
  loading: boolean
}>()

const emit = defineEmits<{
  (e: 'update:open', value: boolean): void
}>()

const innerOpen = computed({
  get: () => props.open,
  set: (v) => emit('update:open', v),
})

// 只保留当前房间、状态为“已预约”的记录
const roomBookings = computed(() => {
  if (!props.room) return []
  return props.bookings.filter(
    (b) =>
      b.seat?.room?.id === props.room?.id &&
      b.state === '已预约'
  )
})
</script>

<template>
  <Dialog v-model:open="innerOpen">
    <DialogContent class="max-w-lg">
      <DialogHeader>
        <DialogTitle>
          {{ room?.name || '本房间' }} 的我的预约
        </DialogTitle>
      </DialogHeader>

      <div
        v-if="loading"
        class="py-4 text-sm text-muted-foreground"
      >
        正在加载…
      </div>

      <div
        v-else-if="roomBookings.length === 0"
        class="py-4 text-sm text-muted-foreground"
      >
        当前房间暂无正在预约中的记录
      </div>

      <div
        v-else
        class="flex flex-col gap-2 max-h-80 overflow-y-auto text-sm"
      >
        <div
          v-for="b in roomBookings"
          :key="b.id"
          class="rounded-md border px-3 py-2 flex flex-col gap-1 bg-muted/60"
        >
          <div class="flex flex-row gap-x-2 items-center">
            <div class="text-base">
              {{ b.seat?.room?.name }}
            </div>
            <div class="text-base">—</div>
            <div class="text-base">
              {{
                (b.seat?.row ?? 0) * (b.seat?.room?.cols ?? 0) +
                (b.seat?.col ?? 0) + 1
              }}
            </div>

            <div class="ml-auto text-xs text-muted-foreground">
              {{ localizeState(b.state) }}
            </div>
          </div>

          <div class="text-xs text-muted-foreground">
            {{ dayjs(b.startTime).format('YYYY/MM/DD HH:mm:ss') }}
            -
            {{ dayjs(b.endTime).format('HH:mm:ss') }}
          </div>
        </div>
      </div>
    </DialogContent>
  </Dialog>
</template>
