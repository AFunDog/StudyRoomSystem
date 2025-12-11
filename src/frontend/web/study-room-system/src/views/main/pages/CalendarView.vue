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
import type { Booking } from '@/lib/types/Booking'

const showOnlyActive = ref(false)

const bookings = ref<Booking[]>([])
const loading = ref(false)

const isDetailDialogOpen = ref(false)
const isCancelDialogOpen = ref(false)
const selectBooking = ref<Booking | null>(null)

const canCancel = computed(() => selectBooking.value?.state === 'Booking')

// 修改列表显示顺序：按创建时间倒序，最新预约在前
const filteredBookings = computed<Booking[]>(() => {
  const list = showOnlyActive.value
    ? bookings.value.filter(
        (b) => b.state === 'Booking' || b.state === 'CheckIn'
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

  // 状态不是 Booking 禁止取消预约按钮
  if (b.state !== 'Booking') {
    toast.error('当前预约状态不可取消')
    isCancelDialogOpen.value = false
    return
  }

  try {
    const res = await bookingRequest.cancelBooking(b.id, false)
    console.log(res)

    // 根据返回 message 判断是否真正取消成功
    if (res.message === '预约已取消') {
      // 成功
      toast.success(res.message)
      await getBookings()
      // 关掉详情弹窗
      isDetailDialogOpen.value = false
    } else {
      // 后端返回了失败原因
      toast.error(res.message || '取消预约失败')
    }
  } catch (err) {
    console.error('取消预约时发生错误', err)
    toast.error('取消预约失败，请稍后重试')
  } finally {
    // 只关掉“确认取消”的弹窗
    isCancelDialogOpen.value = false
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

onMounted(() => {
  getBookings()
})
</script>

<template>
  <div class="flex flex-col items-stretch justify-between h-full w-full min-h-0">
    <Dialog v-model:open="isDetailDialogOpen">
      <DialogContent>
        <DialogHeader>
          <DialogTitle>我的预约</DialogTitle>
          <div class="flex flex-col gap-y-2">
            <div class="flex flex-row gap-x-2 items-center">
              <div class="text-lg">
                {{ selectBooking?.seat?.room?.name }}
              </div>
              <div class="text-lg">—</div>
              <div>
                {{
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
          <div class="flex flex-row items-center justify-center gap-x-2">
            <Button variant="outline" @click="isDetailDialogOpen = false">
              返回
            </Button>
            <Button
              variant="destructive"
              :disabled="!canCancel"
              :class="[!canCancel && 'opacity-50 cursor-not-allowed']"
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
                <Card class="py-2" @click="openDetailDialog(b)">
                  <CardHeader>
                    <div class="flex flex-row gap-x-2 items-center">
                      <div class="text-lg">
                        {{ b.seat?.room?.name }}
                      </div>
                      <div class="text-lg">—</div>
                      <div>
                        {{
                          (b.seat?.row ?? 0) * (b.seat?.room?.cols ?? 0) +
                          (b.seat?.col ?? 0)
                        }}
                      </div>

                      <!-- 右侧只展示状态 -->
                      <div
                        class="flex flex-row items-center justify-center ml-auto text-sm text-muted-foreground"
                      >
                        <div>{{ b.state }}</div>
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
