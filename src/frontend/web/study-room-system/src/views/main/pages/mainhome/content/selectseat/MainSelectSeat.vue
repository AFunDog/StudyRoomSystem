<script setup lang="ts">
import { inject, ref, watch } from 'vue'
import type { Ref } from 'vue'
import dayjs from 'dayjs'
import { toast } from 'vue-sonner'
import { Armchair } from 'lucide-vue-next'

import { SELECT_ROOM } from '../../define'
import { cn } from '@/lib/utils'
import ViewBox from '@/components/ui/view-box/ViewBox.vue'
import type { Seat } from '@/lib/types/Seat'
import type { Room } from '@/lib/types/Room'
import { bookingRequest } from '@/lib/api/bookingRequest'

import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'

const props = defineProps<{
  timeRange: { start: string; end: string } | null
}>()

// 来自 MainHomeView 的当前房间
const selectRoom = inject<Ref<Room | null>>(SELECT_ROOM)!
const seats = ref<(Seat | null)[]>([])

// 预留：座位状态（未来可以根据时间段从后端加载）
const seatStatusMap = ref<Record<string, 'free' | 'busy' | 'disabled'>>({})

// 弹窗相关
const dialogOpen = ref(false)
const selectedSeat = ref<Seat | null>(null)

// 房间变化时，重建 seats 网格
watch(
  selectRoom,
  (room) => {
    if (!room) {
      seats.value = []
      seatStatusMap.value = {}
      return
    }

    const total = (room.rows ?? 0) * (room.cols ?? 0)
    const grid: (Seat | null)[] = Array(total).fill(null)

    room.seats?.forEach((seat) => {
      const idx = seat.row * (room.cols ?? 0) + seat.col
      if (idx >= 0 && idx < total) {
        grid[idx] = seat
      }
    })

    seats.value = grid

    // 这里先把所有存在的座位标记为 free，后面可以根据时间段刷新
    const status: Record<string, 'free' | 'busy' | 'disabled'> = {}
    room.seats?.forEach((s) => {
      status[s.id] = 'free'
    })
    seatStatusMap.value = status
  },
  { immediate: true }
)

// 时间范围变化时预留刷新座位状态的入口
watch(
  () => props.timeRange,
  async (range) => {
    const room = selectRoom.value
    if (!room || !range) return

    // TODO: 在这里调用 seat/booking 相关接口，基于 range 刷新 seatStatusMap
    // 目前先全部保持 'free'
  }
)

function seatClass(seat: Seat | null) {
  if (!seat) return 'invisible pointer-events-none'

  const status = seatStatusMap.value[seat.id] ?? 'free'

  if (status === 'disabled') {
    return 'text-gray-300 cursor-not-allowed'
  }
  if (status === 'busy') {
    return 'text-red-400 cursor-not-allowed'
  }
  // free
  return 'text-black hover:text-primary cursor-pointer'
}

function onSeatClick(seat: Seat | null) {
  if (!seat) return

  if (!selectRoom.value) {
    toast.error('请先选择房间')
    return
  }
  if (!props.timeRange) {
    toast.error('请先选择预约时间')
    return
  }

  const status = seatStatusMap.value[seat.id] ?? 'free'
  if (status !== 'free') {
    toast.error('该座位在所选时间段内不可用')
    return
  }

  selectedSeat.value = seat
  dialogOpen.value = true
}

function displaySeatNumber(seat: Seat | null) {
  const room = selectRoom.value
  if (!seat || !room) return ''
  const idx = seat.row * (room.cols ?? 0) + seat.col + 1
  return idx
}

function formatRange() {
  if (!props.timeRange) return ''
  const start = dayjs(props.timeRange.start)
  const end = dayjs(props.timeRange.end)
  return `${start.format('YYYY-MM-DD HH:mm')} ~ ${end.format('HH:mm')}`
}

async function confirmBooking() {
  const seat = selectedSeat.value
  const range = props.timeRange
  if (!seat || !range) return

  try {
    const res = await bookingRequest.createBooking({
      seatId: seat.id,
      startTime: range.start,
      endTime: range.end,
    })

    if (res) {
      toast.success('预约成功')
      dialogOpen.value = false
    } else {
      toast.error('预约失败，请稍后重试')
    }
  } catch (err) {
    console.error(err)
    toast.error('预约失败，请稍后重试')
  }
}
</script>

<template>
  <div class="bg-accent p-4 rounded-xl w-full h-full grid grid-rows-[auto_1fr]">
    <!-- 顶部房间名 -->
    <div class="flex items-center justify-center gap-x-2 mb-2">
      <div>
        <div class="rounded-full w-3 h-3 bg-green-400"></div>
      </div>
      <div>
        {{ selectRoom?.name || '请先选择房间' }}
      </div>
    </div>

    <!-- 座位网格 -->
    <div class="min-h-0">
      <ViewBox>
        <div
          v-if="selectRoom"
          :class="cn('grid border-gray-500 border-4 rounded-md bg-muted')"
          :style="{
            'grid-template-columns': `repeat(${selectRoom?.cols ?? 0},1fr)`,
          }"
        >
          <div
            v-for="(s, i) in seats"
            :key="i"
            :class="cn(seatClass(s), 'flex items-center justify-center p-1')"
          >
            <Armchair
              class="size-10 md:size-12 transition-colors ease-in-out"
              @click="onSeatClick(s)"
            />
          </div>
        </div>
      </ViewBox>
    </div>

    <!-- 预约确认弹窗 -->
    <Dialog v-model:open="dialogOpen">
      <DialogContent>
        <DialogHeader>
          <DialogTitle>确认座位预约</DialogTitle>
        </DialogHeader>

        <div class="py-2 text-sm space-y-2">
          <div>
            房间：<span class="font-medium">{{ selectRoom?.name }}</span>
          </div>
          <div>
            座位号：
            <span class="font-medium">
              {{ displaySeatNumber(selectedSeat) }}
            </span>
          </div>
          <div>
            预约时间：
            <span class="font-medium">
              {{ formatRange() }}
            </span>
          </div>
        </div>

        <DialogFooter>
          <div class="flex flex-row justify-end gap-x-3 w-full">
            <Button variant="outline" @click="dialogOpen = false">
              取消
            </Button>
            <Button variant="default" @click="confirmBooking">
              确认预约
            </Button>
          </div>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  </div>
</template>
