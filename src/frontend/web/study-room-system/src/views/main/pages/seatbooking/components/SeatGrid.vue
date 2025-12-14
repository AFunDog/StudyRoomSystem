<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import type { Room } from '@/lib/types/Room'
import type { Seat } from '@/lib/types/Seat'
import { Armchair } from 'lucide-vue-next'
import { toast } from 'vue-sonner'
import dayjs from 'dayjs'

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

const seats = ref<(Seat | null)[]>([])

// 预留的状态：free / busy / disabled
const seatStatusMap = ref<Record<string, 'free' | 'busy' | 'disabled'>>({})

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

// ========= 根据房间生成网格 =========
watch(
  () => props.room,
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

    // 初始：所有存在的座位先视为 free
    const status: Record<string, 'free' | 'busy' | 'disabled'> = {}
    room.seats?.forEach((s) => {
      status[s.id] = 'free'
    })
    seatStatusMap.value = status
  },
  { immediate: true }
)

// ========= 时间变化时，调用 v2 接口刷新状态 =========
watch(
  () => props.timeRange,
  async (range) => {
    const room = props.room
    if (!room || !range) {
      return
    }

    try {
      const res = await roomRequest.getRoomWithTime({
        id: room.id,
        start: range.start,   // ISO UTC 字符串
        end: range.end,
      })

      // axios.get<RoomAvailabilityResponse> 返回的是 AxiosResponse
      const data = res.data as {
        room: Room
        seats?: string[] | null
      }

      const openSeatIds = new Set<string>(data.seats ?? [])

      const status: Record<string, 'free' | 'busy' | 'disabled'> = {}
      room.seats?.forEach((s) => {
        // 在 seats 里的就是空闲座位
        status[s.id] = openSeatIds.has(s.id) ? 'free' : 'busy'
      })
      seatStatusMap.value = status
    } catch (err) {
      console.error('获取房间可用座位失败', err)
      // 兜底：全部当 free，让后端创建预约时再做最终校验
      const status: Record<string, 'free' | 'busy' | 'disabled'> = {}
      room.seats?.forEach((s) => {
        status[s.id] = 'free'
      })
      seatStatusMap.value = status
      toast.error('获取座位状态失败，将暂时按全部可用处理')
    }
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

  // 3) 有 seat，根据状态来
  const status = seatStatusMap.value[seat.id] ?? 'free'
  if (status === 'disabled') {
    return 'text-gray-300 opacity-60 cursor-not-allowed'
  }
  if (status === 'busy') {
    return 'text-red-400 cursor-not-allowed'
  }
  // free
  return 'text-black hover:text-primary cursor-pointer'
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
      <div class="rounded-full w-3 h-3 bg-green-400"></div>
      <div>
        {{ room.name }}
      </div>
    </div>

    <!-- 座位网格 -->
    <div class="min-h-0">
      
      <ViewBox>
        <div class="relative w-full h-full flex items-center justify-center">
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
