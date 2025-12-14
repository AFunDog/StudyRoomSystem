<script setup lang="ts">
import type { Room } from '@/lib/types/Room'
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import { Button } from '@/components/ui/button'

const props = defineProps<{
  rooms: Room[]
  loading: boolean
}>()

const emit = defineEmits<{
  (e: 'open-bookings', room: Room): void
  (e: 'enter-room', room: Room): void
}>()

function seatCount(room: Room) {
  return (room.rows ?? 0) * (room.cols ?? 0)
}
</script>

<template>
  <div class="flex-1 min-h-0">
    <!-- 加载中 -->
    <div
      v-if="loading"
      class="w-full h-full flex items-center justify-center text-sm text-muted-foreground"
    >
      正在加载房间信息…
    </div>

    <!-- 没房间 -->
    <div
      v-else-if="rooms.length === 0"
      class="w-full h-full flex items-center justify-center text-sm text-muted-foreground"
    >
      暂无可预约房间
    </div>

    <!-- 房间列表 -->
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
              房间号: {{ room.name }}
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
                @click="emit('open-bookings', room)"
              >
                查看我的预约
              </Button>
              <Button
                size="sm"
                @click="emit('enter-room', room)"
              >
                前往预约
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  </div>
</template>
