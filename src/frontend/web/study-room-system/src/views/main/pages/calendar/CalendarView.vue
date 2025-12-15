<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import dayjs from 'dayjs'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle
} from '@/components/ui/card'
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { toast } from 'vue-sonner'

import { bookingRequest } from '@/lib/api/bookingRequest'
import { localizeState, type Booking } from '@/lib/types/Booking'
import { CornerDownRight } from 'lucide-vue-next'
import { AxiosError } from 'axios'

const showOnlyActive = ref(false)

const bookings = ref<Booking[]>([])
const loading = ref(false)

const isDetailDialogOpen = ref(false)
const isCancelDialogOpen = ref(false)
const isEditDialogOpen = ref(false)

const selectBooking = ref<Booking | null>(null)

// 按状态控制按钮可用
const canCheckIn = computed(() => selectBooking.value?.state === 'Booked')
const canCheckOut = computed(() => selectBooking.value?.state === 'CheckIn')
const canCancel = computed(() => selectBooking.value?.state === 'Booked')
const canEdit   = computed(() => selectBooking.value?.state === 'Booked')

// 只看有效预约：Booked + CheckIn；按创建时间倒序
const filteredBookings = computed<Booking[]>(() => {
  const list = showOnlyActive.value
    ? bookings.value.filter(
        (b) => b.state === 'Booked' || b.state === 'CheckIn'
      )
    : bookings.value

  return list
    .slice()
    .sort(
      (a, b) => dayjs(b.createTime).valueOf() - dayjs(a.createTime).valueOf()
    )
})

/**
 * 拉取当前登录用户的所有预约
 */
async function getBookings() {
  loading.value = true
  try {
    const res = await bookingRequest.getMyBookings()
    bookings.value = res
  } catch (err) {
    console.error('获取预约列表失败', err)
    toast.error('获取预约列表失败，请稍后重试')
  } finally {
    loading.value = false
  }
}

/**
 * 打开详情弹窗
 */
function openDetailDialog(b: Booking) {
  selectBooking.value = b
  isDetailDialogOpen.value = true
}

function openCancelDialog() {
  if (!selectBooking.value) return
  isCancelDialogOpen.value = true
}

// 取消预约
async function confirmCancelBooking() {
  const b = selectBooking.value
  if (!b) {
    isCancelDialogOpen.value = false
    return
  }

  if (b.state !== 'Booked') {
    toast.error('当前预约状态不可取消')
    isCancelDialogOpen.value = false
    return
  }

  try {
    const res = await bookingRequest.cancelBooking(b.id, false)
    console.log(res)

    if (res.message === '预约已取消') {
      toast.success(res.message)
      await getBookings()
      isDetailDialogOpen.value = false
    } else {
      toast.error(res.message || '取消预约失败')
    }
  } catch (err) {
    console.error('取消预约时发生错误', err)
    toast.error('取消预约失败，请稍后重试')
  } finally {
    isCancelDialogOpen.value = false
  }
}

// 签到
async function checkInBooking() {
  const b = selectBooking.value
  if (!b) return

  if (!canCheckIn.value) {
    toast.error('当前预约状态不可签到')
    return
  }

  try {
    await bookingRequest.checkIn({ id: b.id })
    toast.success('签到成功')
    await getBookings()
  } catch (err) {
    console.error('签到时发生错误', err)

    // 尝试解析后端 ProblemDetails.Title
    if (err instanceof AxiosError) {
      const data = err.response?.data as any
      if (data && typeof data.title === 'string') {
        toast.error(data.title)
        return
      }
      if (data && typeof data.message === 'string') {
        toast.error(data.message)
        return
      }
    }

    toast.error('签到失败，请稍后重试')
  }
}

// 签退
async function checkOutBooking() {
  const b = selectBooking.value
  if (!b) return

  if (!canCheckOut.value) {
    toast.error('当前预约状态不可签退')
    return
  }

  try {
    await bookingRequest.checkOut({ id: b.id })
    toast.success('签退成功')
    await getBookings()
  } catch (err) {
    console.error('签退时发生错误', err)

    if (err instanceof AxiosError) {
      const data = err.response?.data as any
      if (data && typeof data.title === 'string') {
        toast.error(data.title)
        return
      }
      if (data && typeof data.message === 'string') {
        toast.error(data.message)
        return
      }
    }

    toast.error('签退失败，请稍后重试')
  }
}

function formatDate(t: string | undefined | null) {
  if (!t) return ''
  return dayjs(t).format('YYYY/MM/DD')
}

function formatTime(t: string | undefined | null) {
  if (!t) return ''
  return dayjs(t).format('HH:mm:ss')
}

function bookingCardClass(state: Booking['state']) {
  switch (state) {
    case 'Booked':
      // 已预约：蓝色，当前/即将到来的预约
      return 'border-l-4 border-l-sky-500/80 bg-background hover:bg-accent/40'
    case 'CheckIn':
      // 已签到：绿色，高亮一点
      return 'border-l-4 border-l-emerald-500/80 bg-background hover:bg-accent/40'
    case 'Checkout':
      // 已签退：灰一点，弱化
      return 'border-l-4 border-l-slate-400/80 bg-background opacity-80 hover:bg-accent/30'
    case 'Canceled':
      // 已取消：红色+透明度+一点删除感
      return 'border-l-4 border-l-red-400/80 bg-background/60 opacity-70 hover:bg-accent/20'
    default:
      return 'bg-background hover:bg-accent/40'
  }
}

function stateBadgeClass(state: Booking['state']) {
  const base =
    'inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium border'

  switch (state) {
    case 'Booked':
      return base + ' bg-sky-50 text-sky-700 border-sky-200'
    case 'CheckIn':
      return base + ' bg-emerald-50 text-emerald-700 border-emerald-200'
    case 'Checkout':
      return base + ' bg-slate-50 text-slate-600 border-slate-200'
    case 'Canceled':
      return base + ' bg-red-50 text-red-600 border-red-200 line-through'
    default:
      return base + ' bg-muted text-muted-foreground border-border'
  }
}

onMounted(() => {
  getBookings()
})
</script>

<template>
  <div class="flex flex-col items-stretch justify-between h-full w-full min-h-0">
    <!-- 详情弹窗 -->
    <Dialog v-model:open="isDetailDialogOpen">
      <DialogContent>
        <DialogHeader>
          <DialogTitle>我的预约</DialogTitle>
          <div class="flex flex-col gap-y-2">
            <div class="flex flex-row gap-x-2 items-center">
              <div class="text-lg">
                房间号: {{ selectBooking?.seat?.room?.name }}
              </div>
              <div class="text-lg">—</div>
              <div class="text-lg">
                座位号: {{
                  (selectBooking?.seat?.row ?? 0) *
                    (selectBooking?.seat?.room?.cols ?? 0) +
                  (selectBooking?.seat?.col ?? 0)
                }}
              </div>
            </div>
            <div class="text-left text-muted-foreground">
              预约时间：
              {{ formatDate(selectBooking?.startTime ?? '') }}
            </div>
            <div class="text-left text-muted-foreground">
              {{ formatTime(selectBooking?.startTime ?? '') }} -
              {{ formatTime(selectBooking?.endTime ?? '') }}
            </div>
          </div>
        </DialogHeader>
        <DialogFooter>
          <div class="flex flex-wrap items-center justify-center gap-2">
            <Button variant="outline" @click="isDetailDialogOpen = false">
              <CornerDownRight class="mr-1" />
              <span>返回</span>
            </Button>

            <Button
              variant="default"
              :disabled="!canCheckIn"
              @click="checkInBooking"
            >
              签到
            </Button>
            <Button
              variant="secondary"
              :disabled="!canCheckOut"
              @click="checkOutBooking"
            >
              签退
            </Button>
            <Button
              variant="destructive"
              :disabled="!canCancel"
              @click="() => { if (canCancel) openCancelDialog() }"
            >
              取消预约
            </Button>
          </div>
        </DialogFooter>
      </DialogContent>
    </Dialog>

    <!-- 确认取消弹窗 -->
    <Dialog v-model:open="isCancelDialogOpen">
      <DialogContent>
        <DialogHeader>
          <DialogTitle>确认取消预约</DialogTitle>
        </DialogHeader>
        <div class="py-2">
          <p>确定要取消这条预约吗？该操作不可撤销。</p>
        </div>
        <DialogFooter>
          <div class="flex flex-row justify-end gap-x-2">
            <Button variant="outline" @click="isCancelDialogOpen = false">
              再想想
            </Button>
            <Button variant="destructive" @click="confirmCancelBooking">
              确认取消
            </Button>
          </div>
        </DialogFooter>
      </DialogContent>
    </Dialog>

    <!-- 上半部分：列表 -->
    <div class="flex-1 px-4 pt-4 min-h-0">
      <Card class="h-full w-full bg-muted flex flex-col">
        <CardHeader class="flex flex-row items-center justify-between">
          <CardTitle>我的预约</CardTitle>

          <div class="flex items-center gap-x-2 text-sm text-muted-foreground">
            <span>只看有效预约</span>
            <input
              type="checkbox"
              :checked="showOnlyActive"
              @change="
                showOnlyActive = ($event.target as HTMLInputElement).checked
              "
              class="h-4 w-4 accent-orange-500"
            />
          </div>
        </CardHeader>

        <CardContent class="px-2 flex-1 min-h-0">
          <!-- 加载中 -->
          <div
            v-if="loading"
            class="text-center py-4 text-sm text-gray-500"
          >
            加载中...
          </div>

          <!-- 没有任何预约 -->
          <div
            v-else-if="filteredBookings.length === 0"
            class="text-center py-4 text-sm text-gray-500"
          >
            暂无预约记录
          </div>

          <!-- 有预约列表 -->
          <div
            v-else
            class="mt-2 flex flex-col min-w-0 gap-y-2 max-w-full flex-nowrap gap-x-2
                  overflow-x-hidden overflow-y-auto h-full rounded-md"
          >
            <TransitionGroup name="bookings">
              <div v-for="b in filteredBookings" :key="b.id">
                <Card
                  class="py-2 transition-colors cursor-pointer"
                  :class="bookingCardClass(b.state)"
                  @click="openDetailDialog(b)"
                >
                  <CardHeader>
                    <div class="flex flex-row gap-x-2 items-center">
                      <div class="text-lg">
                        {{ b.seat?.room?.name }}
                      </div>
                      <div class="text-lg">—</div>
                      <div class="text-lg">
                        {{
                          (b.seat?.row ?? 0) * (b.seat?.room?.cols ?? 0) +
                          (b.seat?.col ?? 0)
                        }}
                      </div>

                      <!-- 右侧：彩色状态胶囊 -->
                      <div
                        class="flex flex-row items-center justify-center ml-auto text-sm"
                      >
                        <span :class="stateBadgeClass(b.state)">
                          {{ localizeState(b.state) }}
                        </span>
                      </div>
                    </div>

                    <CardDescription>
                      <div>
                        {{ formatDate(b.startTime) }}
                        {{ formatTime(b.startTime) }}
                        -
                        {{ formatTime(b.endTime) }}
                      </div>
                    </CardDescription>
                  </CardHeader>
                </Card>
              </div>
            </TransitionGroup>
          </div>
        </CardContent>
      </Card>
    </div>
  </div>
</template>
