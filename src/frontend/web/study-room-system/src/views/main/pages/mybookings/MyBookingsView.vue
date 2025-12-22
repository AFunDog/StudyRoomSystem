<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import dayjs from "dayjs";
import { toast } from "vue-sonner";
import { AxiosError } from "axios";

import { bookingRequest } from "@/lib/api/bookingRequest";
import type { Booking, BookingState } from "@/lib/types/Booking";
import BookingFilters from "./components/BookingFilters.vue";
import BookingList from "./components/BookingList.vue";
import BookingDetailDialog from "./components/BookingDetailDialog.vue";
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { RotateCw } from "lucide-vue-next";

type StatusFilter = "all" | BookingState;
type DateFilter = "all" | "today" | "future";

// 原始预约列表与加载态
const bookings = ref<Booking[]>([]);
const loading = ref(false);
const page = ref(1);
const pageSize = 20;
const hasMore = ref(true);
const loadingMore = ref(false);


// 筛选条件
const statusFilter = ref<StatusFilter>("all");
const dateFilter = ref<DateFilter>("all");
const roomFilter = ref<string>("all");

// 详情弹窗
const detailOpen = ref(false);
const selectedBooking = ref<Booking | null>(null);
const forceCancelOpen = ref(false);
const pendingForceCancel = ref<Booking | null>(null);

// 房间名称选项（从已加载的预约里去重生成）
const roomOptions = computed(() => {
  const names = new Set<string>();
  bookings.value.forEach((b) => {
    const name = b.seat?.room?.name;
    if (name) names.add(name);
  });
  return Array.from(names);
});

// 前端过滤：状态/房间/时间；按创建时间倒序
const filteredBookings = computed(() => {
  return bookings.value
    .filter((b) => {
      if (statusFilter.value !== "all" && b.state !== statusFilter.value) return false;

      if (roomFilter.value !== "all" && b.seat?.room?.name !== roomFilter.value) return false;

      if (dateFilter.value === "today") {
        const start = dayjs(b.startTime);
        return start.isSame(dayjs(), "day");
      }

      if (dateFilter.value === "future") {
        return dayjs(b.startTime).isAfter(dayjs(), "day");
      }

      return true;
    })
    .sort((a, b) => dayjs(b.createTime).valueOf() - dayjs(a.createTime).valueOf());
});

// 拉取我的预约列表（后端不支持筛选参数，前端过滤）
async function loadBookings(reset = false) {
  if (reset) {
    page.value = 1;
    hasMore.value = true;
    bookings.value = [];
  }

  if (!hasMore.value && !reset) return;

  if (page.value === 1) {
    loading.value = true;
  } else {
    loadingMore.value = true;
  }

  try {
    const res = await bookingRequest.getMyBookings({
      page: page.value,
      pageSize,
    });

    // 少于 pageSize，说明没有更多了
    if (res.length < pageSize) {
      hasMore.value = false;
    }

    if (page.value === 1) {
      bookings.value = res;
    } else {
      bookings.value = [...bookings.value, ...res];
    }
  } catch (err) {
    console.error("获取预约列表失败", err);
    toast.error("获取预约列表失败，请稍后重试");
  } finally {
    loading.value = false;
    loadingMore.value = false;
  }
}

async function loadMore() {
  if (!hasMore.value) return;
  page.value += 1;
  await loadBookings(false);
}


function openDetail(b: Booking) {
  selectedBooking.value = b;
  detailOpen.value = true;
}

// 签到/签退/取消操作后刷新列表
async function handleCheckIn(b: Booking) {
  try {
    await bookingRequest.checkIn({ id: b.id });
    toast.success("签到成功");
    await loadBookings(true);
    detailOpen.value = false;
  } catch (err) {
    console.error("签到时发生错误", err);

    if (err instanceof AxiosError) {
      const data = err.response?.data as any;
      if (data && typeof data.title === "string") {
        toast.error(data.title);
        return;
      }
      if (data && typeof data.message === "string") {
        toast.error(data.message);
        return;
      }
    }

    toast.error("签到失败，请稍后重试");
  }
}

async function handleCheckOut(b: Booking) {
  try {
    await bookingRequest.checkOut({ id: b.id });
    toast.success("签退成功");
    await loadBookings(true);
    detailOpen.value = false;
  } catch (err) {
    console.error("签退时发生错误", err);

    if (err instanceof AxiosError) {
      const data = err.response?.data as any;
      if (data && typeof data.title === "string") {
        toast.error(data.title);
        return;
      }
      if (data && typeof data.message === "string") {
        toast.error(data.message);
        return;
      }
    }

    toast.error("签退失败，请稍后重试");
  }
}

// 取消预约，必要时询问并执行强制取消（会记录违规）
async function handleCancel(b: Booking) {
  try {
    const res = await bookingRequest.cancelBooking(b.id, false);
    toast.success(res?.message || "预约已取消");
    await loadBookings(true);
    detailOpen.value = false;
  } catch (err) {
    if (err instanceof AxiosError) {
      const msg =
        err.response?.data?.title || err.response?.data?.message || "";

      // 后端提示需要强制取消的情况
      if (
        err.response?.status === 400 &&
        msg.includes("强制取消预约将会记录为违规")
      ) {
        pendingForceCancel.value = b;
        forceCancelOpen.value = true;
        return;
      }

      toast.error(msg || "取消预约失败，请稍后重试");
      return;
    }
    toast.error("取消预约失败，请稍后重试");
  }
}

async function confirmForceCancel() {
  const b = pendingForceCancel.value;
  if (!b) {
    forceCancelOpen.value = false;
    return;
  }

  try {
    const res = await bookingRequest.cancelBooking(b.id, true);
    toast.success(res?.message || "已强制取消并记录违规");
    forceCancelOpen.value = false;
    detailOpen.value = false;
    await loadBookings(true);
  } catch (err) {
    if (err instanceof AxiosError) {
      const msg =
        err.response?.data?.title || err.response?.data?.message || "";
      toast.error(msg || "强制取消失败，请稍后重试");
      return;
    }
    toast.error("强制取消失败，请稍后重试");
  }
}



onMounted(() => {
  loadBookings(true);
});
</script>

<template>
  <div class="flex flex-col h-full w-full px-4 py-4 gap-4 min-h-0">
    <div class="flex items-center justify-between">
      <div class="text-lg font-semibold">
        我的预约
      </div>

      <!-- 右上角刷新按钮：PC端图标按钮，移动端带文字 -->
      <div class="flex items-center gap-2">
        <Button
          variant="ghost"
          size="sm"
          class="flex items-center gap-1 md:hidden
                  bg-gray-100 hover:bg-gray-200
                  text-gray-600 border border-gray-200
                  disabled:opacity-60 disabled:cursor-not-allowed"
          :disabled="loading"
          @click="loadBookings(true)"
        >
          <RotateCw class="w-4 h-4" />
          <span class="text-xs">刷新</span>
        </Button>

        <!-- PC端：纯图标按钮 -->
        <Button
          variant="ghost"
          size="icon"
          class="hidden md:inline-flex
                  bg-gray-100 hover:bg-gray-200
                  text-gray-600 border border-gray-200
                  rounded-full
                  disabled:opacity-60 disabled:cursor-not-allowed"
          :disabled="loading"
          @click="loadBookings(true)"
        >
          <RotateCw class="w-4 h-4" />
        </Button>
      </div>
    </div>

    <BookingFilters
      :status="statusFilter"
      :date="dateFilter"
      :room="roomFilter"
      :room-options="roomOptions"
      @update:status="statusFilter = $event as StatusFilter"
      @update:date="dateFilter = $event as DateFilter"
      @update:room="roomFilter = $event"
    />

    <div class="flex-1 min-h-0 h-full overflow-hidden">
      <BookingList
        :bookings="filteredBookings"
        :loading="loading"
        @select="openDetail"
      />
    </div>

    <!-- 加载更多 -->
    <div
      v-if="hasMore"
      class="mt-2 flex justify-center"
    >
      <Button
        variant="outline"
        size="sm"
        :disabled="loadingMore"
        @click="loadMore"
      >
        {{ loadingMore ? "加载中..." : "加载更多" }}
      </Button>
    </div>

    <!-- 没有更多 -->
    <div
      v-else
      class="mt-1 flex justify-center text-sm text-muted-foreground"
    >
      没有更多了
    </div>

    <!-- 预约详情弹窗-->
    <BookingDetailDialog
      :open="detailOpen"
      :booking="selectedBooking"
      @update:open="detailOpen = $event"
      @check-in="handleCheckIn"
      @check-out="handleCheckOut"
      @cancel="handleCancel"
    />

    <!-- 强制取消确认弹窗-->
    <Dialog
      :open="forceCancelOpen"
      @update:open="forceCancelOpen = $event"
    >
      <DialogContent>
        <DialogHeader>
          <DialogTitle>确认强制取消预约</DialogTitle>
        </DialogHeader>

        <div class="py-2 text-sm space-y-1">
          <p>距离预约开始不足 3 小时，强制取消将会记录为违规。</p>
          <p>
            房间：
            <span class="font-medium">
              {{ pendingForceCancel?.seat?.room?.name || "未知房间" }}
            </span>
            ，座位：
            <span class="font-medium">
              {{
                (pendingForceCancel?.seat?.row ?? 0) *
                  (pendingForceCancel?.seat?.room?.cols ?? 0) +
                (pendingForceCancel?.seat?.col ?? 0) +
                1
              }}
            </span>
          </p>
          <p class="text-xs text-muted-foreground">
            继续操作会在违规记录中增加一条“强制取消预约”的记录。
          </p>
        </div>

        <DialogFooter>
          <div class="flex justify-end gap-2 w-full">
            <Button variant="outline" @click="forceCancelOpen = false">
              再想想
            </Button>
            <Button variant="destructive" @click="confirmForceCancel">
              继续强制取消
            </Button>
          </div>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  </div>
</template>
