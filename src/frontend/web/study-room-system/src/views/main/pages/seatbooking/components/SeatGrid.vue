<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import type { Room } from '@/lib/types/Room'
import type { Seat } from '@/lib/types/Seat'
import { Armchair } from 'lucide-vue-next'
import { toast } from 'vue-sonner'
import dayjs from 'dayjs'
import utc from 'dayjs/plugin/utc'

import ViewBox from '@/components/ui/view-box/ViewBox.vue'
import { cn, http } from '@/lib/utils'
import { bookingRequest } from '@/lib/api/bookingRequest'
import { roomRequest } from '@/lib/api/roomRequest'

import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'

const props = defineProps<{
  room: Room
  timeRange: { start: string; end: string } | null
}>()

dayjs.extend(utc)

const seats = ref<(Seat | null)[]>([])

// 预留的状态：free / busy / disabled
const seatStatusMap = ref<Record<string, 'free' | 'busy' | 'disabled'>>({})

// 加载遮罩
const loadingSeats = ref(false)

// 弹窗
const dialogOpen = ref(false)
const selectedSeat = ref<Seat | null>(null)

// ========= 图标大小自适应 =========
const seatIconClass = computed(() => {
  const rows = props.room.rows ?? 0
  const cols = props.room.cols ?? 0
  const maxDim = Math.max(rows, cols)

  if (maxDim <= 4) {
    return 'w-14 h-14 md:w-16 md:h-16'
  } else if (maxDim <= 6) {
    return 'w-12 h-12 md:w-14 md:h-14'
  } else if (maxDim <= 8) {
    return 'w-10 h-10 md:w-12 md:h-12'
  } else {
    return 'w-8 h-8 md:w-10 md:h-10'
  }
})

function buildGrid(room: Room | null, seatList: Seat[] | null | undefined) {
  if (!room) return []
  const total = (room.rows ?? 0) * (room.cols ?? 0)
  const grid: (Seat | null)[] = Array(total).fill(null)

  seatList?.forEach((seat) => {
    const idx = (seat.row ?? 0) * (room.cols ?? 0) + (seat.col ?? 0)
    if (idx >= 0 && idx < total) {
      grid[idx] = seat
    }
  })
  return grid
}

// ========= 根据房间生成网格 =========
watch(
  () => props.room,
  (room) => {
    if (!room) {
      seats.value = []
      seatStatusMap.value = {}
      return
    }

    seats.value = buildGrid(room, room.seats)

    // 初始：所有存在的座位先视为 free
    const status: Record<string, 'free' | 'busy' | 'disabled'> = {}
    room.seats?.forEach((s) => {
      status[s.id] = 'free'
    })
    seatStatusMap.value = status
  },
  { immediate: true }
)

// 根据当前房间 + 时间段刷新座位状态
async function refreshSeatStatus(
  rangeOverride?: { start: string; end: string } | null
) {
  const room = props.room
  const range = rangeOverride ?? props.timeRange

  if (!room || !range) {
    return
  }

  loadingSeats.value = true

  try {
    const res = await roomRequest.getRoomWithTime({
      id: room.id,
      start: dayjs(range.start).utc().toISOString(),
      end: dayjs(range.end).utc().toISOString(),
    });


    const data = res.data as {
      room?: Room
      Room?: Room
      seats?: string[] | null
      Seats?: string[] | null
    }

    const roomData = data.room ?? data.Room ?? room
    const seatList = roomData?.seats ?? room.seats ?? []

    // 用后端返回的座位明细刷新网格（v2 接口会返回 seats 详情）
    seats.value = buildGrid(roomData, seatList)

    const openSeatIds = new Set<string>(data.seats ?? data.Seats ?? [])

    const status: Record<string, 'free' | 'busy' | 'disabled'> = {}
    seatList.forEach((s) => {
      status[s.id] = openSeatIds.has(s.id) ? 'free' : 'busy'
    })
    seatStatusMap.value = status
  } catch (err) {
    console.error('获取房间可用座位失败', err)
    const status: Record<string, 'free' | 'busy' | 'disabled'> = {}
    room.seats?.forEach((s) => {
      status[s.id] = 'free'
    })
    seatStatusMap.value = status
    toast.error('获取座位状态失败，将暂时按全部可用处理')
  } finally {
    loadingSeats.value = false
  }
}

// 时间变化时刷新座位状态
watch(
  () => props.timeRange,
  async () => {
    await refreshSeatStatus(null)
  }
)

// ========= 样式 & 交互 =========

function seatClass(seat: Seat | null) {
  // 1) 未选择时间：全部灰色禁用，但网格保留
  if (!props.timeRange) {
    return 'text-gray-400 opacity-40 cursor-not-allowed'
  }

  // 2) 网格位置上没有 seat（这个位置没座位）
  if (!seat) {
    return 'text-gray-300 opacity-30 cursor-not-allowed'
  }

  // 3) 有 seat
  const status = seatStatusMap.value[seat.id] ?? 'free'
  if (status === 'disabled') {
    return 'text-gray-300 opacity-60 cursor-not-allowed'
  }
  if (status === 'busy') {
    return 'text-red-400 cursor-not-allowed'
  }
  // free
  return 'hover:text-primary cursor-pointer'
}

function displaySeatNumber(seat: Seat | null) {
  if (!seat) return ''
  const idx = seat.row * (props.room.cols ?? 0) + seat.col + 1
  return idx
}

function onSeatClick(seat: Seat | null) {
  if (!seat) return

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
      // 后端要求 UTC 时间：使用 dayjs.utc() 再转 ISO
      startTime: dayjs(range.start).utc().toISOString(),
      endTime: dayjs(range.end).utc().toISOString(),
    });


    if (res) {
      toast.success('预约成功')
      dialogOpen.value = false
      await refreshSeatStatus(range)
    } else {
      toast.error('预约失败，请稍后重试')
    }
  } catch (err) {
    console.error(err)
    const msg =
      (err && typeof err === 'object' && (err as any).message) ||
      '预约失败，请稍后重试'
    toast.error(msg)
  }
}
</script>

<template>
  <div class="bg-accent p-4 rounded-xl w-full h-full grid grid-rows-[auto_1fr]">
    <div
      class="flex flex-row items-center justify-center gap-3 text-xs md:text-sm [&>div>span]:text-muted-foreground"
    >
      <div class="flex items-center gap-1">
        <Armchair class="w-6 h-6" />
        <span>可预约</span>
      </div>
      <div class="flex items-center gap-1">
        <Armchair class="w-6 h-6 text-red-400" />
        <span>已占用</span>
      </div>
      <div class="flex items-center gap-1">
        <Armchair class="w-6 h-6 text-gray-300" />
        <span>不可用</span>
      </div>
    </div>

    <!-- 座位网格 -->
    <div class="min-h-0">
      <ViewBox>
        <div class="relative w-full h-full flex items-center justify-center">
          <div
            v-if="loadingSeats"
            class="absolute inset-0 z-10 flex items-center justify-center bg-white/60 backdrop-blur-sm rounded-lg"
          >
            <span class="text-sm text-gray-700">正在加载座位状态...</span>
          </div>

          <div
            :class="cn('grid border-gray-500 border-4 rounded-md bg-muted')"
            :style="{
              'grid-template-columns': `repeat(${room.cols},1fr)`,
            }"
          >
            <div
              v-for="(s, i) in seats"
              :key="i"
              :class="cn(seatClass(s), 'flex items-center justify-center p-1')"
            >
              <Armchair
                :class="cn(seatIconClass, 'transition-colors ease-in-out')"
                @click="onSeatClick(s)"
              />
            </div>
          </div>

          <!-- 未选时间时的提示 -->
          <div
            v-if="!timeRange"
            class="absolute inset-0 flex items-center justify-center pointer-events-none"
          >
            <div
              class="rounded-full bg-background/90 px-4 py-2 text-sm text-muted-foreground shadow"
            >
              请先在上方选择预约日期和时间
            </div>
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
            房间：<span class="font-medium">{{ room.name }}</span>
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
