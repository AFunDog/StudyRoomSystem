<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { toast } from "vue-sonner";
import dayjs from "dayjs";
import type { Violation } from "@/lib/types/Violation";
import { violationRequest } from "@/lib/api/violationRequest";
import { bookingRequest } from "@/lib/api/bookingRequest";
import { seatRequest } from "@/lib/api/seatRequest";
import { roomRequest } from "@/lib/api/roomRequest";
import ViolationFilters from "./components/ViolationFilters.vue";
import ViolationList from "./components/ViolationList.vue";
import { Button } from "@/components/ui/button";
import { RotateCw } from "lucide-vue-next";

type TypeFilter = "all" | Violation["type"];

const violations = ref<Violation[]>([]);
const loading = ref(false);
const loadingMore = ref(false);
const page = ref(1);
const pageSize = 20;
const hasMore = ref(true);

const typeFilter = ref<TypeFilter>("all");
const detailOpen = ref(false);
const detailBooking = ref<any>(null);
const detailViolation = ref<Violation | null>(null);

function formatSeatNumber(seat?: any) {
  const cols = seat?.room?.cols;
  const row = seat?.row;
  const col = seat?.col;
  if (cols == null || row == null || col == null) return "未知座位";
  return row * cols + col + 1;
}

function formatRoomName(seat?: any) {
  return seat?.room?.name || "未知房间";
}

const filteredViolations = computed(() => {
  return violations.value
    .filter((v) => typeFilter.value === "all" || v.type === typeFilter.value)
    .sort(
      (a, b) =>
        dayjs(b.createTime).valueOf() -
        dayjs(a.createTime).valueOf()
    );
});

async function loadViolations(reset = false) {
  if (reset) {
    page.value = 1;
    hasMore.value = true;
    violations.value = [];
  }
  if (!hasMore.value && !reset) return;

  if (page.value === 1) {
    loading.value = true;
  } else {
    loadingMore.value = true;
  }

  try {
    const res = await violationRequest.getMyViolations({
      page: page.value,
      pageSize,
    });

    if (res.items.length < pageSize || page.value * pageSize >= res.total ) {
      hasMore.value = false;
    }

    if (page.value === 1) {
      violations.value = res.items;
    } else {
      violations.value = [...violations.value, ...res.items];
    }
  } catch (err) {
    console.error("获取违规记录失败", err);
    toast.error("获取违规记录失败，请稍后重试");
  } finally {
    loading.value = false;
    loadingMore.value = false;
  }
}

async function loadMore() {
  if (!hasMore.value) return;
  page.value += 1;
  await loadViolations(false);
}

async function openDetail(v: Violation) {
  detailViolation.value = v;
  detailBooking.value = null;
  detailOpen.value = true;

  if (v.bookingId) {
    try {
      const booking = await bookingRequest.getBookingById(v.bookingId);
      let seat = booking.seat;

      // 补充座位/房间信息
      if (!seat && booking.seatId) {
        try {
          const seatRes = await seatRequest.getSeat(booking.seatId);
          seat = (seatRes as any).data ?? seatRes;
        } catch (e) {
          console.warn("获取座位信息失败", e);
        }
      }
      if (seat && !seat.room && (seat as any).roomId) {
        try {
          const roomRes = await roomRequest.getRoom((seat as any).roomId);
          seat.room = (roomRes as any).data ?? roomRes;
        } catch (e) {
          console.warn("获取房间信息失败", e);
        }
      }

      detailBooking.value = { ...booking, seat };
    } catch (err) {
      console.error("获取预约详情失败", err);
      detailBooking.value = null;
    }
  }
}

function closeDetail() {
  detailOpen.value = false;
  detailBooking.value = null;
  detailViolation.value = null;
}

onMounted(() => {
  loadViolations(true);
});
</script>

<template>
  <div class="flex flex-col h-full w-full px-4 py-4 gap-4 min-h-0 relative">
    <div class="flex items-center justify-between gap-2">
      <div class="text-lg font-semibold">
        我的违规
      </div>

      <div class="flex items-center gap-2">
        <Button
          variant="ghost"
          size="sm"
          class="flex items-center gap-1 md:hidden
                  bg-gray-100 hover:bg-gray-200
                  text-gray-600 border border-gray-200
                  disabled:opacity-60 disabled:cursor-not-allowed"
          :disabled="loading"
          @click="loadViolations(true)"
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
          @click="loadViolations(true)"
        ><RotateCw class="w-4 h-4" />
        </Button>
      </div>
    </div>

    <ViolationFilters
      :type-filter="typeFilter"
      :loading="loading"
      @update:type-filter="typeFilter = $event as TypeFilter"
    />

    <div class="flex-1 min-h-0 h-full overflow-hidden">
      <ViolationList
        :violations="filteredViolations"
        :loading="loading"
        @view-detail="openDetail"
      />
    </div>

    <div v-if="hasMore" class="mt-2 flex justify-center">
      <Button
        variant="outline"
        size="sm"
        :disabled="loadingMore"
        @click="loadMore"
      >
        {{ loadingMore ? "加载中..." : "加载更多" }}
      </Button>
    </div>
    <div
      v-else
      class="mt-1 flex justify-center text-sm text-muted-foreground"
    >
      没有更多了
    </div>
  
    <div
      v-if="detailOpen"
      class="fixed inset-0 bg-black/40 flex items-center justify-center z-50"
    >
      <div class="bg-background rounded-lg shadow-lg w-full max-w-md mx-4 p-4 space-y-3">
        <div class="flex items-center justify-between">
          <div class="text-lg font-semibold">违规详情</div>
          <Button variant="ghost" size="sm" @click="closeDetail">关闭</Button>
        </div>

        <div class="text-sm text-muted-foreground space-y-1">
          <div>类型：{{ detailViolation?.type }}</div>
          <div>记录时间：{{ detailViolation ? dayjs(detailViolation.createTime).format("YYYY/MM/DD HH:mm") : "--" }}</div>
          <div>主要内容：{{ detailViolation?.content || "无" }}</div>
        </div>

        <div class="text-sm text-muted-foreground">
          <div v-if="detailViolation?.bookingId">关联预约信息如下</div>
          <div v-else>无关联预约</div>
        </div>

        <div v-if="detailBooking" class="rounded-md border p-3 space-y-1 text-sm bg-accent/40">
          <div class="font-medium">预约信息</div>
          <div>房间：{{ formatRoomName(detailBooking.seat) }}</div>
          <div>座位：{{ formatSeatNumber(detailBooking.seat) }}</div>
          <div>时间：{{ dayjs(detailBooking.startTime).format("YYYY/MM/DD HH:mm") }} - {{ dayjs(detailBooking.endTime).format("HH:mm") }}</div>
        </div>
      </div>
    </div>
  </div>
</template>
