<script setup lang="ts">
import { onMounted, provide, ref, computed } from 'vue'
import { ArrowLeft } from 'lucide-vue-next'
import { toast } from 'vue-sonner'

import type { Room } from '@/lib/types/Room'
import type { Booking } from '@/lib/types/Booking'
import { roomRequest } from '@/lib/api/roomRequest'
import { bookingRequest } from '@/lib/api/bookingRequest'
import { SELECT_ROOM } from './define'

import MainSelectTime from './content/MainSelectTime.vue'
import MainSelectSeat from './content/selectseat/MainSelectSeat.vue'

import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'

type Mode = 'list' | 'booking'

// 所有房间
const rooms = ref<Room[]>([])
const loadingRooms = ref(false)

// 当前页面模式：房间列表 / 某个房间的预约界面
const mode = ref<Mode>('list')

// 当前正在预约的房间
const activeRoom = ref<Room | null>(null)

// 当前选择的时间段（在某个房间内）
const timeRange = ref<{ start: string; end: string } | null>(null)

// provide 给子组件使用的“当前房间”
provide(SELECT_ROOM, activeRoom)

// 「查看我的预约」弹窗相关
const myBookings = ref<Booking[]>([])
const bookingsLoaded = ref(false)
const bookingDialogOpen = ref(false)
const bookingRoom = ref<Room | null>(null)

const roomBookings = computed(() => {
  if (!bookingRoom.value) return []
  return myBookings.value.filter(
    (b) => b.seat?.room?.id === bookingRoom.value?.id
  )
})

// 初始化拉取房间列表
onMounted(async () => {
  loadingRooms.value = true
  try {
    const res = await roomRequest.getRooms()
    rooms.value = res.data as Room[]
  } catch (err) {
    console.error(err)
    toast.error('获取房间列表失败，请稍后重试')
  } finally {
    loadingRooms.value = false
  }
})

async function openRoomBookings(room: Room) {
  bookingRoom.value = room
  bookingDialogOpen.value = true

  if (!bookingsLoaded.value) {
    try {
      const list = await bookingRequest.getMyBookings()
      myBookings.value = list
      bookingsLoaded.value = true
    } catch (err) {
      console.error(err)
      toast.error('获取预约记录失败')
    }
  }
}

function enterBooking(room: Room) {
  activeRoom.value = room
  timeRange.value = null
  mode.value = 'booking'
}

function backToList() {
  mode.value = 'list'
  activeRoom.value = null
  timeRange.value = null
}

function seatCount(room: Room) {
  return (room.rows ?? 0) * (room.cols ?? 0)
}
</script>

<template>
  <div class="flex flex-col h-full w-full px-4 py-4 gap-4">

    <!-- 模式一：房间列表 -->
    <template v-if="mode === 'list'">
      <div class="flex items-center justify-between">
        <div class="text-lg font-semibold">
          选择房间进行预约
        </div>
      </div>

      <div class="flex-1 min-h-0">
        <!-- 加载中 -->
        <div
          v-if="loadingRooms"
          class="w-full h-full flex items-center justify-center text-sm text-muted-foreground"
        >
          正在加载房间信息…
        </div>

        <!-- 房间为空 -->
        <div
          v-else-if="rooms.length === 0"
          class="w-full h-full flex items-center justify-center text-sm text-muted-foreground"
        >
          暂无可预约房间
        </div>

        <!-- 房间网格 -->
        <div
          v-else
          class="h-full overflow-y-auto"
        >
          <div
            class="grid gap-4
                   grid-cols-1
                   sm:grid-cols-2
                   lg:grid-cols-3
                   xl:grid-cols-4"
          >
            <Card
              v-for="room in rooms"
              :key="room.id"
              class="flex flex-col h-full"
            >
              <CardHeader>
                <CardTitle class="text-base">
                  {{ room.name }}
                </CardTitle>
              </CardHeader>
              <CardContent class="flex flex-col gap-2 text-sm text-muted-foreground">
                <div>
                  开放时间：{{ room.openTime }} - {{ room.closeTime }}
                </div>
                <div>
                  座位数量：{{ seatCount(room) }}
                </div>

                <div class="mt-3 flex gap-2 justify-end">
                  <Button
                    variant="outline"
                    size="sm"
                    @click="openRoomBookings(room)"
                  >
                    查看我的预约
                  </Button>
                  <Button
                    size="sm"
                    @click="enterBooking(room)"
                  >
                    前往预约
                  </Button>
                </div>
              </CardContent>
            </Card>
          </div>
        </div>
      </div>

      <!-- 查看当前房间我的预约的弹窗 -->
      <Dialog v-model:open="bookingDialogOpen">
        <DialogContent class="max-w-lg">
          <DialogHeader>
            <DialogTitle>
              {{ bookingRoom?.name || '本房间' }} 的我的预约
            </DialogTitle>
          </DialogHeader>

          <div
            v-if="!bookingsLoaded"
            class="py-4 text-sm text-muted-foreground"
          >
            正在加载…
          </div>
          <div
            v-else-if="roomBookings.length === 0"
            class="py-4 text-sm text-muted-foreground"
          >
            当前房间暂无我的预约记录
          </div>
          <div
            v-else
            class="flex flex-col gap-2 max-h-80 overflow-y-auto text-sm"
          >
            <div
              v-for="b in roomBookings"
              :key="b.id"
              class="rounded-md border px-3 py-2 flex flex-col gap-1"
            >
              <div class="flex justify-between items-center">
                <span class="font-medium">
                  座位号：
                  {{
                    (b.seat?.row ?? 0) * (b.seat?.room?.cols ?? 0) +
                    (b.seat?.col ?? 0) + 1
                  }}
                </span>
                <span class="text-xs text-muted-foreground">
                  状态：{{ b.state }}
                </span>
              </div>
              <div class="text-xs text-muted-foreground">
                {{ b.startTime }} ~ {{ b.endTime }}
              </div>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </template>

    <!-- 模式二：进入某个房间的预约界面 -->
    <template v-else>
      <!-- 顶部返回 + 房间名称 -->
      <div class="flex items-center gap-2">
        <Button
          variant="ghost"
          size="icon"
          @click="backToList"
          class="shrink-0"
        >
          <ArrowLeft class="w-5 h-5" />
        </Button>
        <div class="text-lg font-semibold truncate">
          {{ activeRoom?.name }} 座位预约
        </div>
      </div>

      <!-- 下方：时间选择 + 座位选择 -->
      <div class="flex flex-col gap-4 flex-1 min-h-0">
        <!-- 时间选择条：在 PC/手机都是横向排布 -->
        <MainSelectTime v-model="timeRange" />

        <!-- 座位网格区域：占满剩余高度 -->
        <MainSelectSeat
          class="flex-1 min-h-0"
          :time-range="timeRange"
        />
      </div>
    </template>
  </div>
</template>
