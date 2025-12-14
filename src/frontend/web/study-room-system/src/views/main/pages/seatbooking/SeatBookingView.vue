<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { ArrowLeft } from 'lucide-vue-next'
import { toast } from 'vue-sonner'

import type { Room } from '@/lib/types/Room'
import type { Booking } from '@/lib/types/Booking'
import { roomRequest } from '@/lib/api/roomRequest'
import { bookingRequest } from '@/lib/api/bookingRequest'

import RoomListPanel from './components/RoomListPanel.vue'
import RoomBookingPanel from './components/RoomBookingPanel.vue'
import RoomBookingsDialog from './components/RoomBookingsDialog.vue'
import { Button } from '@/components/ui/button'

type Mode = 'rooms' | 'roomBooking'

const mode = ref<Mode>('rooms')

// 房间列表相关
const rooms = ref<Room[]>([])
const loadingRooms = ref(false)

// 房间预约相关
const activeRoom = ref<Room | null>(null)
const timeRange = ref<{ start: string; end: string } | null>(null)

// “查看该房间我的预约”弹窗相关
const bookingsDialogOpen = ref(false)
const bookingsLoading = ref(false)
const bookings = ref<Booking[]>([])
const bookingsRoom = ref<Room | null>(null)

/** 初始化加载房间列表 */
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

/** 打开“该房间我的预约”弹窗 */
async function handleOpenRoomBookings(room: Room) {
  bookingsRoom.value = room
  bookingsDialogOpen.value = true

  if (bookings.value.length > 0) return

  bookingsLoading.value = true
  try {
    const list = await bookingRequest.getMyBookings()
    bookings.value = list
  } catch (err) {
    console.error(err)
    toast.error('获取预约记录失败')
  } finally {
    bookingsLoading.value = false
  }
}

/** 进入某个房间的预约界面 */
function handleEnterRoom(room: Room) {
  activeRoom.value = room
  timeRange.value = null
  mode.value = 'roomBooking'
}

/** 从房间预约界面返回房间列表 */
function handleBack() {
  mode.value = 'rooms'
  activeRoom.value = null
  timeRange.value = null
}
</script>

<template>
  <div class="flex flex-col h-full w-full px-4 py-4 gap-4">

    <!-- 模式一：房间列表 -->
    <template v-if="mode === 'rooms'">
      <div class="flex items-center justify-between">
        <div class="text-lg font-semibold">
          自习室列表
        </div>
      </div>

      <RoomListPanel
        class="flex-1 min-h-0"
        :rooms="rooms"
        :loading="loadingRooms"
        @open-bookings="handleOpenRoomBookings"
        @enter-room="handleEnterRoom"
      />

      <RoomBookingsDialog
        v-model:open="bookingsDialogOpen"
        :room="bookingsRoom"
        :bookings="bookings"
        :loading="bookingsLoading"
      />
    </template>

    <!-- 模式二：某个房间的预约界面 -->
    <template v-else>
      <div class="flex items-center gap-2">
        <Button
          variant="ghost"
          size="icon"
          class="shrink-0"
          @click="handleBack"
        >
          <ArrowLeft class="w-5 h-5" />
        </Button>
        <div class="text-lg font-semibold truncate">
          {{ activeRoom?.name }} 座位预约
        </div>
      </div>

      <RoomBookingPanel
        v-if="activeRoom"
        class="flex-1 min-h-0"
        :room="activeRoom"
        v-model:timeRange="timeRange"
      />
    </template>
  </div>
</template>
