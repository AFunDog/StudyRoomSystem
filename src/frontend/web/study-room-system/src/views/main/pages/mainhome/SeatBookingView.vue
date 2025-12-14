<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { toast } from 'vue-sonner'
import { ArrowLeft } from 'lucide-vue-next'
import { seatRequest } from '@/lib/api/seatRequest'
import { bookingRequest } from '@/lib/api/bookingRequest'
import type { Seat } from '@/lib/types/Seat'
import type { Room } from '@/lib/types/Room'
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import dayjs from 'dayjs'

interface SeatWithRoom extends Seat {
  room: Room | null
}

const route = useRoute()
const router = useRouter()

const seatId = route.params.seatId as string
const start = route.query.start as string | undefined
const end = route.query.end as string | undefined

const seat = ref<SeatWithRoom | null>(null)
const loading = ref(false)

async function loadSeat() {
  try {
    loading.value = true
    const res = await seatRequest.getSeat(seatId)
    seat.value = res.data as SeatWithRoom
  } catch (err) {
    console.error(err)
    toast.error('加载座位信息失败')
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  if (!seatId || !start || !end) {
    toast.error('缺少预约参数，请重新选择座位和时间')
    router.back()
    return
  }
  loadSeat()
})

async function confirmBooking() {
  if (!seatId || !start || !end) return

  try {
    const res = await bookingRequest.createBooking({
      seatId,
      startTime: start,
      endTime: end,
    })

    if (res) {
      toast.success('预约成功')
      router.back()
    } else {
      toast.error('预约失败，请稍后重试')
    }
  } catch (err) {
    console.error(err)
    toast.error('预约失败，请稍后重试')
  }
}

function formatTime(t?: string) {
  if (!t) return ''
  return dayjs(t).format('YYYY-MM-DD HH:mm')
}
</script>

<template>
  <div class="flex flex-col h-full w-full px-4 py-4 gap-4">
    <!-- 顶部导航栏：返回 + 标题 -->
    <div class="flex items-center gap-2">
      <Button variant="ghost" size="icon" @click="router.back()">
        <ArrowLeft class="w-5 h-5" />
      </Button>
      <div class="text-lg font-semibold">
        座位预约确认
      </div>
    </div>

    <Card class="flex-1">
      <CardHeader>
        <CardTitle>
          {{ seat?.room?.name || '未知房间' }} - 座位 {{ seatId.slice(0, 8) }}…
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div v-if="loading" class="text-sm text-muted-foreground">
          加载中…
        </div>

        <div v-else-if="!seat">
          <div class="text-sm text-destructive">
            未找到座位信息，请返回重试。
          </div>
        </div>

        <div v-else class="space-y-4">
          <div>
            <div class="text-sm text-muted-foreground">
              房间
            </div>
            <div class="text-base">
              {{ seat.room?.name }}（{{ seat.room?.rows }} 行 × {{ seat.room?.cols }} 列）
            </div>
          </div>

          <div>
            <div class="text-sm text-muted-foreground">
              预约时间
            </div>
            <div class="text-base">
              {{ formatTime(start) }} ~ {{ formatTime(end) }}
            </div>
          </div>

          <!-- 这里可以扩展展示该座位当前预约情况（调用 seat v2） -->

          <div class="pt-6 flex justify-end gap-2">
            <Button variant="outline" @click="router.back()">
              返回
            </Button>
            <Button variant="default" @click="confirmBooking">
              确认预约
            </Button>
          </div>
        </div>
      </CardContent>
    </Card>
  </div>
</template>
