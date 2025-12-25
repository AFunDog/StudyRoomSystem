<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from "vue";
import dayjs from "dayjs";
import { useRouter } from "vue-router";
import { toast } from "vue-sonner";

import type { Booking } from "@/lib/types/Booking";
import type { User } from "@/lib/types/User";

import { bookingRequest } from "@/lib/api/bookingRequest";
import { userRequest } from "@/lib/api/userRequest";

import { CalendarCheck, Armchair, ShieldAlert, Megaphone, RotateCw } from "lucide-vue-next";
import { Button } from "@/components/ui/button";

import HomeHeaderCard from "./components/HomeHeaderCard.vue";
import HomeScheduleCard from "./components/HomeScheduleCard.vue";
import HomeQuickActions from "./components/HomeQuickActions.vue";

const router = useRouter();

const bookings = ref<Booking[]>([]);
const user = ref<User | null>(null);
const creditScore = ref<number | null>(null);

const refreshing = ref(false);
const acting = ref(false);

// 倒计时：每 30 秒更新一次 now
const now = ref(dayjs());
let timer: number | null = null;

// ---------- 日期展示 ----------
const weekZh = ["日", "一", "二", "三", "四", "五", "六"];
const todayText = computed(() => {
  const d = now.value;
  return `${d.format("YYYY年M月D日")} 星期${weekZh[d.day()]}`;
});

// ---------- 统一错误提取（复用“我的预约”的风格） ----------
function extractErrMessage(err: unknown, fallback: string) {
  if (err && typeof err === "object") {
    const data = (err as any)?.response?.data;
    if (data?.detail && typeof data.detail === "string") return data.detail;
    if (data?.message && typeof data.message === "string") return data.message;
    if (data?.title && typeof data.title === "string") return data.title;
  }
  if (err instanceof Error && err.message) {
    if (err.message === "请求错误") return fallback;
    return err.message;
  }
  return fallback;
}

// ---------- 数据加载 ----------
async function loadBookings() {
  try {
    const data: any = await bookingRequest.getMyBookings({ page: 1, pageSize: 50 });
    bookings.value = Array.isArray(data) ? data : data?.items ?? data?.Items ?? [];
  } catch (err) {
    console.error("获取预约失败", err);
    toast.error(extractErrMessage(err, "获取预约失败，请稍后重试"));
  }
}

async function loadUser() {
  try {
    const res = await userRequest.getUser();
    const data = res.data as User;

    user.value = data;
    creditScore.value =
      typeof (data as any)?.credits === "number" ? (data as any).credits : Number((data as any)?.credits);

    localStorage.setItem("user", JSON.stringify(data));
  } catch (err) {
    console.error("获取用户信息失败", err);
  }
}

async function refreshAll() {
  if (refreshing.value) return;
  refreshing.value = true;
  try {
    await Promise.all([loadBookings(), loadUser()]);
  } finally {
    refreshing.value = false;
  }
}

function go(path: string) {
  router.push(path);
}

// ---------- 预约排序/统计 ----------
const sortedBookings = computed(() =>
  [...bookings.value].sort((a, b) => dayjs(a.startTime).valueOf() - dayjs(b.startTime).valueOf())
);

const todayBookings = computed(() => {
  const t = now.value;
  return sortedBookings.value.filter((b) => dayjs(b.startTime).isSame(t, "day"));
});

const weekBookings = computed(() => {
  const t = now.value;
  return sortedBookings.value.filter((b) => dayjs(b.startTime).isSame(t, "week"));
});

const todayCount = computed(() => todayBookings.value.length);
const weekCount = computed(() => weekBookings.value.length);

function isCanceledOrTimeout(b: Booking) {
  return b.state === "已取消" || b.state === "已超时";
}
const canceledCount = computed(() => bookings.value.filter(isCanceledOrTimeout).length);

// ---------- 当前日程：最近一条“有用预约” ----------
function isUsefulBooking(b: Booking) {
  if (b.state !== "已预约" && b.state !== "已签到") return false;
  return dayjs(b.endTime).isAfter(now.value);
}

const activeBooking = computed<Booking | null>(() => {
  const list = sortedBookings.value.filter(isUsefulBooking);
  if (list.length === 0) return null;

  // 1) 进行中优先
  const ongoing = list.find((b) => {
    const s = dayjs(b.startTime);
    const e = dayjs(b.endTime);
    return (s.isBefore(now.value) || s.isSame(now.value)) && e.isAfter(now.value);
  });
  if (ongoing) return ongoing;

  // 2) 已签到优先
  const checked = list.find((b) => b.state === "已签到");
  if (checked) return checked;

  // 3) 最近即将开始
  const upcoming = list
    .filter((b) => dayjs(b.startTime).isAfter(now.value))
    .sort((a, b) => dayjs(a.startTime).valueOf() - dayjs(b.startTime).valueOf())[0];

  return upcoming ?? null;
});

// 行列从 0 开始 → 展示时 +1
function seatRowColText(b: Booking) {
  const r = b.seat?.row;
  const c = b.seat?.col;
  if (r == null || c == null) return "未知座位";
  return `第${r + 1}行第${c + 1}列`;
}

// 座位号（线性编号）
function seatNoText(b: Booking) {
  const r = b.seat?.row;
  const c = b.seat?.col;
  const cols = b.seat?.room?.cols;
  if (r == null || c == null || cols == null) return null;
  return r * cols + c + 1;
}

function statusText(b: Booking) {
  if (b.state === "已签到") return "已签到";
  const s = dayjs(b.startTime);
  const e = dayjs(b.endTime);
  if ((s.isBefore(now.value) || s.isSame(now.value)) && e.isAfter(now.value)) return "进行中";
  if (s.isAfter(now.value)) return "即将开始";
  return "已预约";
}

const activeCountdown = computed(() => {
  const b = activeBooking.value;
  if (!b) return "";

  const s = dayjs(b.startTime);
  const e = dayjs(b.endTime);

  if (now.value.isBefore(s)) {
    const diffMin = s.diff(now.value, "minute");
    const h = Math.floor(diffMin / 60);
    const m = diffMin % 60;
    return `距离开始${h ? `${h}小时` : ""}${m}分钟`;
  }

  if (now.value.isBefore(e)) {
    const diffMin = e.diff(now.value, "minute");
    const h = Math.floor(diffMin / 60);
    const m = diffMin % 60;
    return `剩余 ${h ? `${h}小时` : ""}${m}分钟`;
  }

  return "";
});

// ---------- 主页直接签到/签退 ----------
async function doCheckIn() {
  const b = activeBooking.value;
  if (!b) return;

  acting.value = true;
  try {
    await bookingRequest.checkIn({ id: b.id });
    toast.success("签到成功");
    await refreshAll();
  } catch (err) {
    console.error("签到失败", err);
    toast.error(extractErrMessage(err, "签到失败，请稍后重试"));
  } finally {
    acting.value = false;
  }
}

async function doCheckOut() {
  const b = activeBooking.value;
  if (!b) return;

  acting.value = true;
  try {
    await bookingRequest.checkOut({ id: b.id });
    toast.success("签退成功");
    await refreshAll();
  } catch (err) {
    console.error("签退失败", err);
    toast.error(extractErrMessage(err, "签退失败，请稍后重试"));
  } finally {
    acting.value = false;
  }
}

// ---------- 信用分染色 ----------
type CreditLevel = "NORMAL" | "LIMITED" | "STRICT" | "FORBIDDEN";

const creditLevel = computed<CreditLevel>(() => {
  const s = creditScore.value;
  if (s == null) return "NORMAL";
  if (s >= 60) return "NORMAL";
  if (s >= 40) return "LIMITED";
  return "FORBIDDEN";
});

const creditText = computed(() => {
  switch (creditLevel.value) {
    case "NORMAL":
      return "正常";
    case "LIMITED":
      return "受限";
    case "FORBIDDEN":
      return "不可使用";
  }
  return "不可使用";
});

// ---------- 状态染色 ----------
function statusBadgeClass(b: Booking) {
  const t = statusText(b);
  if (t === "已签到") return "bg-emerald-50 text-emerald-700 border border-emerald-200";
  if (t === "进行中") return "bg-orange-50 text-orange-700 border border-orange-200";
  if (t === "即将开始") return "bg-amber-50 text-amber-700 border border-amber-200";
  return "bg-slate-100 text-slate-600 border border-slate-200";
}


const creditHint = computed(() => {
  switch (creditLevel.value) {
    case "NORMAL":
      return "60分及以上：正常使用";
    case "LIMITED":
      return "40-59分：预约受限";
    case "FORBIDDEN":
      return "40分以下：不可进行预约";
  }
  return "40分以下：不可进行预约";
});

const creditBoxClass = computed(() => {
  switch (creditLevel.value) {
    case "NORMAL":
      return "bg-emerald-50 border-emerald-200 text-emerald-800 ring-1 ring-emerald-100 shadow-sm";
    case "LIMITED":
      return "bg-amber-50 border-amber-200 text-amber-800 ring-1 ring-amber-100 shadow-sm";
    case "FORBIDDEN":
      return "bg-red-50 border-red-200 text-red-800 ring-1 ring-red-100 shadow-sm";
  }
  return "bg-red-50 border-red-200 text-red-800 ring-1 ring-red-100 shadow-sm";
});


// ---------- 快捷入口 ----------
const quickLinks = computed(() => [
  { label: "座位预约", path: "/seatbooking", icon: Armchair, value: "", hint: "去选择座位" },
  { label: "我的预约", path: "/mybookings", icon: CalendarCheck, value: `${todayCount.value} 条今日`, hint: "查看 / 签到 / 取消" },
  { label: "投诉进度", path: "/mycomplaints", icon: Megaphone, value: "跟进", hint: "处理中 / 已处理" },
  { label: "违规记录", path: "/myviolations", icon: ShieldAlert, value: "查看", hint: "超时 / 强制取消" },
]);

// ---------- 生命周期 ----------
onMounted(() => {
  refreshAll();

  timer = window.setInterval(() => {
    now.value = dayjs();
  }, 30 * 1000);
});

onUnmounted(() => {
  if (timer) window.clearInterval(timer);
  timer = null;
});
</script>

<template>
  <div class="w-full h-full flex flex-col">
    <!-- 固定头 -->
    <div class="sticky top-0 z-20 bg-background/80 backdrop-blur">
      <div class="flex items-center justify-between px-4 py-3">
        <div class="text-lg font-semibold">
          首页
        </div>

        <div class="flex items-center gap-2">
          <Button
            variant="ghost"
            size="sm"
            class="flex items-center gap-1 md:hidden
                    bg-gray-100 hover:bg-gray-200
                    text-gray-600 border border-gray-200
                    disabled:opacity-60 disabled:cursor-not-allowed"
            :disabled="refreshing"
            @click="refreshAll"
          >
            <RotateCw class="w-4 h-4" :class="refreshing ? 'animate-spin' : ''" />
            <span class="text-xs">刷新</span>
          </Button>

          <Button
            variant="ghost"
            size="icon"
            class="hidden md:inline-flex
                    bg-gray-100 hover:bg-gray-200
                    text-gray-600 border border-gray-200
                    rounded-full
                    disabled:opacity-60 disabled:cursor-not-allowed"
            :disabled="refreshing"
            @click="refreshAll"
          >
            <RotateCw class="w-4 h-4" :class="refreshing ? 'animate-spin' : ''" />
          </Button>
        </div>
      </div>
    </div>

    <!-- 内容区 -->
    <div class="relative flex-1 overflow-y-auto">
      <div class="px-4 py-5 pb-10 w-full flex flex-col gap-5">
        <HomeHeaderCard
          :today-text="todayText"
          :display-name="user?.displayName || '同学'"
          :credit-score="creditScore"
          :credit-text="creditText"
          :credit-hint="creditHint"
          :credit-box-class="creditBoxClass"
          :today-count="todayCount"
          :week-count="weekCount"
          :canceled-count="canceledCount"
          :refreshing="refreshing"
          @refresh="refreshAll"
        />

        <HomeScheduleCard
          :active-booking="activeBooking"
          :active-countdown="activeCountdown"
          :acting="acting"
          :seat-row-col-text="seatRowColText"
          :seat-no-text="seatNoText"
          :status-text="statusText"
          :status-badge-class="statusBadgeClass"
          @goBookings="go('/mybookings')"
          @goSeatBooking="go('/seatbooking')"
          @checkIn="doCheckIn"
          @checkOut="doCheckOut"
        />

        <HomeQuickActions :quick-links="quickLinks" @select="go" />
      </div>

      <!-- 蒙版 -->
      <div
        v-if="refreshing"
        class="absolute inset-0 z-30 flex items-center justify-center bg-white/70 backdrop-blur-sm"
      >
        <div class="flex flex-col items-center gap-2 text-sm text-gray-600">
          <span>正在刷新...</span>
        </div>
      </div>
    </div>
  </div>
</template>
