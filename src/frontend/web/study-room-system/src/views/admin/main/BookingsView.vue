<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch } from "vue";
import { debounce } from "lodash";
import { toast } from "vue-sonner";
import { CalendarIcon, Loader2, Copy } from "lucide-vue-next";
import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem
} from "@/components/ui/select";

// 表单相关
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog';

import { Button } from "@/components/ui/button";

import * as echarts from "echarts";

import { userRequest } from "@/lib/api/userRequest";
import { bookingRequest } from "@/lib/api/bookingRequest";
import { roomRequest } from "@/lib/api/roomRequest";

import type { Booking } from "@/lib/types/Booking";
import type { Room } from "@/lib/types/Room";
import type { Seat } from "@/lib/types/Seat";

// 复制文本函数
function copyText(text: string) { 
  if (!text) return; 
  navigator.clipboard.writeText(text); 
  toast.success("复制成功"); 
}

/* -------------------------------------------------------
类型声明
------------------------------------------------------- */
interface TimeSlot {
  start: string;
  end: string;
  label: string;
}

type CellState =
  | "空闲"
  | "已预约"
  | "已签到"
  | "已签退"
  | "已取消"
  | "已超时";

interface HeatmapCell {
  seat: Seat;
  slot: TimeSlot;
  slotIndex: number;
  booking: Booking | null;
  state: CellState;
}

interface HeatmapRow {
  seat: Seat;
  cells: HeatmapCell[];
}

/* -------------------------------------------------------
本地日期工具函数
------------------------------------------------------- */
const formatLocalDate = (date: Date): string => {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, "0");
  const d = String(date.getDate()).padStart(2, "0");
  return `${y}-${m}-${d}`;
};

const makeSlotDate = (dateStr: string, timeStr: string): Date => {
  const time = timeStr.length === 5 ? `${timeStr}:00` : timeStr;
  return new Date(`${dateStr}T${time}`);
};

// 日期转换函数
function formatDate(dateStr: string) {
  if (!dateStr) return "";
  const date = new Date(dateStr);
  return date.toLocaleString("zh-CN", {
    year: "numeric",
    month: "long",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit"
  });
}

function formatTimeOnly(dateStr: string) {
  const d = new Date(dateStr);
  return d.toLocaleTimeString("zh-CN", {
    hour: "2-digit",
    minute: "2-digit",
    hour12: false
  });
}

/* -------------------------------------------------------
页面状态
------------------------------------------------------- */
let chartInstance: echarts.ECharts | null = null;

const rooms = ref<Room[]>([]);
const selectedRoomId = ref<string>("");
const selectedDate = ref<string>(formatLocalDate(new Date()));

const seats = ref<Seat[]>([]);
const bookings = ref<Booking[]>([]);
const timeSlots = ref<TimeSlot[]>([]);

const selectedCell = ref<HeatmapCell | null>(null);
const selectedBooking = ref<Booking | null>(null);

const hoverCell = ref<HeatmapCell | null>(null);
let hoverTimer: number | null = null;

const chartRef = ref<HTMLDivElement | null>(null);

/* -------------------------------------------------------
生成半小时时间段
------------------------------------------------------- */
function generateTimeSlots(
  openTime: string,
  closeTime: string,
  labelInterval = 2
): TimeSlot[] {
  const defaultOpen = "08:00:00";
  const defaultClose = "22:00:00";

  const [hStr, mStr] = (openTime || defaultOpen).split(":");
  const [endHStr, endMStr] = (closeTime || defaultClose).split(":");

  let h = Number(hStr);
  let m = Number(mStr);
  const endH = Number(endHStr);
  const endM = Number(endMStr);

  const result: TimeSlot[] = [];
  let slotCount = 0;

  while (true) {
    const start = `${String(h).padStart(2, "0")}:${String(m).padStart(2, "0")}`;

    // 计算 end
    let nextH = h;
    let nextM = m + 30;
    if (nextM >= 60) {
      nextM = 0;
      nextH++;
    }

    // 如果 end >= closeTime，则停止，不 push
    if (nextH > endH || (nextH === endH && nextM > endM)) break;

    const end = `${String(nextH).padStart(2, "0")}:${String(nextM).padStart(2, "0")}`;

    const showLabel = slotCount % (labelInterval * 2) === 0;
    const label = showLabel ? `${start}-${end}` : "";

    result.push({ start, end, label });
    slotCount++;

    // 更新 h,m
    h = nextH;
    m = nextM;
  }

  return result;
}


/* -------------------------------------------------------
加载数据
------------------------------------------------------- */
async function loadAll(): Promise<void> {
  if (!selectedRoomId.value) return;

  try {
    // 获取房间基础信息
    const room: Room | undefined = rooms.value.find(
      r => r.id === selectedRoomId.value
    );
    if (!room) return;

    timeSlots.value = generateTimeSlots(room.openTime, room.closeTime, 1);
    console.log("打印时间段",timeSlots.value);
    console.log("数量:", timeSlots.value.length);


    // 获取房间详细信息（含座位）
    const roomDetail = await roomRequest.getRoom(selectedRoomId.value);
    seats.value = roomDetail.data.seats ?? [];

    // 座位排序
    seats.value.sort((a, b) => {
      if (a.row !== b.row) return a.row - b.row;
      return a.col - b.col;
    });

    // console.log("房间 seats:", seats.value);

    // 构造筛选参数
    const targetDate = selectedDate.value; 
    console.log("当前选中日期:", targetDate);   // 调试选中日期
    const startTime = new Date(`${targetDate}T00:00:00`).toISOString(); 
    const endTime = new Date(`${targetDate}T23:59:59`).toISOString();

    // 获取预约数据（已筛选 roomId + 日期）
    const result = await bookingRequest.getAllBookings({ 
      page: 1, 
      pageSize: 100, 
      roomId: selectedRoomId.value, 
      startTime, 
      endTime, 
    });
    const allBookings: Booking[] = result.items;

    // 调试：打印所有预约
    // console.log("所有预约（原始）:", allBookings);

    // 并行加载缺失的用户信息 
    await Promise.all( 
      allBookings 
        .filter(b => !b.user) 
        .map(async b => { 
          try { 
            const res = await userRequest.getUserById(b.userId); 
            b.user = res.data; 
          } catch (err) { 
            console.error(`加载用户 ${b.userId} 失败`, err); 
          } 
        }) 
    );

    // 调试：打印每条预约的日期
    // allBookings.forEach(b => {
    //   if (!b.startTime) return;
    //   console.log(
    //     `预约ID ${b.id} → startTime=${b.startTime} → 本地日期=${formatLocalDate(new Date(b.startTime))}`
    //   );
    // });

    // 调试：过滤前数量
    // console.log("过滤前预约数量:", allBookings.length);

    // 过滤出当前房间、当前日期的预约
    bookings.value = allBookings.filter(b => {
      if (!b.startTime || !b.seatId) return false;

      const bookingDateStr = formatLocalDate(new Date(b.startTime));
      const isSeatInRoom = seats.value.some(s => s.id === b.seatId);

      const match = bookingDateStr === targetDate && isSeatInRoom;

      // 调试：打印过滤结果
      // console.log(
      //   `过滤预约ID ${b.id}: 日期=${bookingDateStr}, 座位匹配=${isSeatInRoom}, 是否保留=${match}`
      // );

      return match;
    });

    // -------------------------------------------------------
    // 构建 seatId → 预约列表映射，并预计算时间戳
    // -------------------------------------------------------
    bookingMap.clear();

    for (const b of bookings.value) {
      const start = new Date(b.startTime).getTime();
      const end = new Date(b.endTime).getTime();
    
      if (!bookingMap.has(b.seatId)) bookingMap.set(b.seatId, []);
      bookingMap.get(b.seatId)!.push({ start, end, raw: b });
    }


    // 调试：过滤后数量
    // console.log("过滤后预约数量:", bookings.value.length);
    // console.log("最终保留的预约:", bookings.value);

    buildHeatmap();
    renderUsageChartDebounced();
  } catch (error) {
    toast.error("加载数据失败，请重试");
    console.error(error);
  }
}


/* -------------------------------------------------------
高性能热力图矩阵
------------------------------------------------------- */
// 预处理：seatId → 预约列表映射
const bookingMap = new Map<
  string,
  { start: number; end: number; raw: Booking }[]
>();

const heatmap = ref<HeatmapRow[]>([]);

function buildHeatmap() {
  const rows: HeatmapRow[] = [];

  for (const seat of seats.value) {
    const seatBookings = bookingMap.get(seat.id) ?? [];
    const cells: HeatmapCell[] = [];

    for (let i = 0; i < timeSlots.value.length; i++) {
      const slot = timeSlots.value[i];
      if (!slot) continue;
      const slotStart = makeSlotDate(selectedDate.value, slot.start).getTime();
      const slotEnd = slotStart + 30 * 60 * 1000;

      let matched: Booking | null = null;

      // 只遍历当前座位的预约（极大提升性能）
      for (const b of seatBookings) {
        if (slotStart >= b.start && slotStart < b.end) {
          matched = b.raw;
          break;
        }
      }

      let state: CellState = "空闲";
      if (matched) {
        switch (matched.state) {
          case "已预约":
            state = matched.state;
            break;
          case "已签到":
            state = matched.state;
            break;
          case "已签退":
            state = matched.state;
            break;
          case "已取消":
            state = matched.state;
            break;
          case "已超时":
            state = matched.state;
            break;
        }
      }

      cells.push({
        seat,
        slot,
        slotIndex: i,
        booking: matched,
        state
      });
    }

    rows.push({ seat, cells });
  }

  heatmap.value = rows;
}


/* -------------------------------------------------------
热力图辅助函数
------------------------------------------------------- */
// 查找是否有预约覆盖当前半小时格子
function getCellState(seatId: string, slotIndex: number): CellState {
  const slot = timeSlots.value[slotIndex];
  if (!slot) return "空闲";

  const slotStart = makeSlotDate(selectedDate.value, slot.start).getTime();
  const list = bookingMap.get(seatId);
  if (!list) return "空闲";

  const booking = list.find(b => slotStart >= b.start && slotStart < b.end)?.raw;
  return booking?.state ?? "空闲";
}

function onCellLeave(): void {
  if (hoverTimer !== null) {
    clearTimeout(hoverTimer);
    hoverTimer = null;
  }
  hoverCell.value = null;
}


// 包含预约
function safeCellEnter(seatId: string, slotIndex: number): void {
  // 清除旧的延迟
  if (hoverTimer !== null) {
    clearTimeout(hoverTimer);
    hoverTimer = null;
  }

  // 设置新的延迟（300ms 后显示悬浮提示）
  hoverTimer = window.setTimeout(() => {
    const slot = timeSlots.value[slotIndex];
    if (!slot) return;

    const seat = seats.value.find(s => s.id === seatId);
    if (!seat) return;

    const slotStart = makeSlotDate(selectedDate.value, slot.start).getTime();
    const list = bookingMap.get(seatId);

    let booking: Booking | null = null;
    if (list) {
      booking = list.find(b => slotStart >= b.start && slotStart < b.end)?.raw || null;
    }

    hoverCell.value = {
      seat,
      slot,
      slotIndex,
      booking,
      state: getCellState(seatId, slotIndex)
    };
  }, 300); // ← 悬停延迟时间（300ms）
}


//点击格子详情面板
function safeCellClick(seatId: string, slotIndex: number): void {
  // 测试该函数是否被触发
  console.log("点击触发:", seatId, slotIndex);


  const slot = timeSlots.value[slotIndex];
  const seat = seats.value.find(s => s.id === seatId);
  if (!slot || !seat) return;

  const slotStart = makeSlotDate(selectedDate.value, slot.start).getTime();
  const list = bookingMap.get(seatId);

  let booking: Booking | null = null;
  if (list) {
    booking = list.find(b => slotStart >= b.start && slotStart < b.end)?.raw || null;
  }

  // 测试booking是否为空
  console.log("找到的 booking:", booking);


  selectedCell.value = {
    seat,
    slot,
    slotIndex,
    booking,
    state: getCellState(seatId, slotIndex)
  };

  // 单独存储预约对象，方便按钮操作（取消/签到/签退）
  selectedBooking.value = booking;
}


/* -------------------------------------------------------
利用率图表
------------------------------------------------------- */
function renderUsageChart(): void {
  if (!chartRef.value || !seats.value.length || !timeSlots.value.length) {
    if (chartInstance) chartInstance.clear();
    return;
  }

  if (!chartInstance) {
    chartInstance = echarts.init(chartRef.value, undefined, {
      renderer: "svg",
      useDirtyRect: true
    });
  }

  const usageStates = ["已预约", "已签到", "已签退"];
  const wasteStates = ["已超时"];

  const usageMap = new Map<string, { start: Date; end: Date }[]>();
  const wasteMap = new Map<string, { start: Date; end: Date }[]>();

  for (const b of bookings.value) {
    if (!b.seatId || !b.startTime || !b.endTime) continue;

    const entry = {
      start: new Date(b.startTime),
      end: new Date(b.endTime)
    };

    if (usageStates.includes(b.state)) {
      if (!usageMap.has(b.seatId)) usageMap.set(b.seatId, []);
      usageMap.get(b.seatId)!.push(entry);
    } else if (wasteStates.includes(b.state)) {
      if (!wasteMap.has(b.seatId)) wasteMap.set(b.seatId, []);
      wasteMap.get(b.seatId)!.push(entry);
    }
  }

  const totalCells = seats.value.length * timeSlots.value.length;
  let usedCells = 0;
  let wastedCells = 0;

  const usageSeries: number[] = [];
  const wasteSeries: number[] = [];

  for (const slot of timeSlots.value) {
    const slotStart = makeSlotDate(selectedDate.value, slot.start);
    const slotEnd = new Date(slotStart.getTime() + 30 * 60 * 1000);

    let used = 0;
    let wasted = 0;

    for (const seat of seats.value) {
      const usageList = usageMap.get(seat.id) ?? [];
      const wasteList = wasteMap.get(seat.id) ?? [];

      const isUsed = usageList.some(b => b.start < slotEnd && b.end > slotStart);
      const isWasted = wasteList.some(b => b.start < slotEnd && b.end > slotStart);

      if (isUsed) {
        used++;
        usedCells++;
      } else if (isWasted) {
        wasted++;
        wastedCells++;
      }
    }

    usageSeries.push(seats.value.length ? (used / seats.value.length) * 100 : 0);
    wasteSeries.push(seats.value.length ? (wasted / seats.value.length) * 100 : 0);
  }

  const usageRate = totalCells
    ? (usedCells / totalCells * 100).toFixed(1)
    : "0.0";
  const wasteRate = totalCells
    ? (wastedCells / totalCells * 100).toFixed(1)
    : "0.0";

  const xLabels = timeSlots.value.map(slot => {
    const date = makeSlotDate(selectedDate.value, slot.start);
    return date.toLocaleTimeString("zh-CN", {
      hour: "2-digit",
      minute: "2-digit"
    });
  });

  chartInstance.setOption(
    {
      title: {
        text: `房间利用率：${usageRate}%   资源浪费率：${wasteRate}%`
      },
      tooltip: { trigger: "axis" },
      legend: {
        data: ["利用率", "浪费率"]
      },
      xAxis: {
        type: "category",
        data: xLabels,
        axisLabel: { rotate: 30, interval: 0 },
        boundaryGap: false
      },
      yAxis: {
        type: "value",
        max: 100,
        axisLabel: { formatter: "{value}%" }
      },
      series: [
        {
          name: "浪费率",
          type: "line",
          data: wasteSeries,
          smooth: true,
          areaStyle: { opacity: 0.2 },
          emphasis: { disabled: true },
          lineStyle: { color: "#ef4444" }, // 红色
          itemStyle: { color: "#ef4444" }
        },
        {
          name: "利用率",
          type: "line",
          data: usageSeries,
          smooth: true,
          areaStyle: { opacity: 0.2 },
          emphasis: { disabled: true },
          lineStyle: { color: "#3b82f6" }, // 蓝色
          itemStyle: { color: "#3b82f6" }
        }
      ]
    },
    true
  );
}

const renderUsageChartDebounced = debounce(renderUsageChart, 50, {
  leading: true
});

/* -------------------------------------------------------
事件与生命周期
------------------------------------------------------- */
function onDateChange(e: Event): void {
  const target = e.target as HTMLInputElement;
  if (target.value) {
    selectedDate.value = target.value;
    void loadAll();
  }
}

watch(
  () => selectedRoomId.value,
  () => {
    void loadAll();
  }
);

onMounted(async () => {
  try {
    const res = await roomRequest.getRooms({ page: 1, pageSize: 20 });
    rooms.value = res.data.items as Room[];

    if (rooms.value.length > 0) {
      selectedRoomId.value = rooms.value[0]?.id ?? "";
      await loadAll();
    }
  } catch (error) {
    toast.error("初始化失败");
    console.error(error);
  }
});

onUnmounted(() => {
  renderUsageChartDebounced.cancel();
  if (chartInstance) {
    chartInstance.dispose();
    chartInstance = null;
  }
});

// 删除弹窗状态
const isDeleteDialogOpen = ref(false);
// 当前准备删除的预约 ID
const pendingDeleteBookingId = ref<string | null>(null);
// 正在删除的预约 ID（用于 loading）
const deletingBookingId = ref<string | null>(null);

//打开删除弹窗方法
function openDeleteDialog(bookingId: string) {
  pendingDeleteBookingId.value = bookingId;
  isDeleteDialogOpen.value = true;
}

//确认删除
async function confirmDeleteBooking() {
  if (!pendingDeleteBookingId.value) return;

  deletingBookingId.value = pendingDeleteBookingId.value;

  try {
    await bookingRequest.deleteBooking(pendingDeleteBookingId.value);
    toast.success("删除成功");

    // 关闭弹窗
    isDeleteDialogOpen.value = false;

    // 清空状态
    pendingDeleteBookingId.value = null;
    deletingBookingId.value = null;

    // 刷新热力图
    loadAll();

    // 清空右侧详情面板
    selectedCell.value = null;
    selectedBooking.value = null;

  } catch (err: any) {
    toast.error(err.message || "删除失败");
  } finally {
    deletingBookingId.value = null;
  }
}


async function checkIn(id: string): Promise<void> {
  try {
    await bookingRequest.checkIn({ id });
    toast.success("签到成功");
    await loadAll();
  } catch (error) {
    toast.error("签到失败");
    console.error(error);
  }
}

async function checkOut(id: string): Promise<void> {
  try {
    await bookingRequest.checkOut({ id });
    toast.success("签退成功");
    await loadAll();
  } catch (error) {
    toast.error("签退失败");
    console.error(error);
  }
}

</script>


<template>
  <div class="space-y-4">

    <!-- 页面标题 -->
    <h2 class="text-xl font-bold mb-4">预约管理</h2>

    <!-- 顶部控制区 -->
    <div class="flex items-center gap-4">
      <div class="flex flex-col gap-1">
        <div class="text-sm font-medium">房间</div>
        <Select v-model="selectedRoomId" @update:modelValue="loadAll">
          <SelectTrigger class="w-full md:w-55">
            <SelectValue placeholder="选择房间" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem v-for="r in rooms" :key="r.id" :value="r.id">
              {{ r.name }}
            </SelectItem>
          </SelectContent>
        </Select>
      </div>

      <div class="flex flex-col gap-1">
        <div class="text-sm font-medium">预约日期</div>
        <div class="relative">
          <input
            type="date"
            v-model="selectedDate"
            @change="onDateChange"
            class="w-full md:w-55 px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
          <CalendarIcon class="absolute right-2 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-500 pointer-events-none" />
        </div>
      </div>
    </div>

    <!-- 颜色图例 -->
    <div class="flex flex-wrap gap-4 text-sm">
      <div class="flex items-center gap-1"><div class="w-4 h-4 bg-gray-100 border rounded" /> 空闲</div>
      <div class="flex items-center gap-1"><div class="w-4 h-4 bg-blue-200 rounded" /> 已预约</div>
      <div class="flex items-center gap-1"><div class="w-4 h-4 bg-green-300 rounded" /> 已签到</div>
      <div class="flex items-center gap-1"><div class="w-4 h-4 bg-gray-300 rounded" /> 已签退</div>
      <div class="flex items-center gap-1"><div class="w-4 h-4 bg-yellow-200 rounded" /> 已取消</div>
      <div class="flex items-center gap-1"><div class="w-4 h-4 bg-red-300 rounded" /> 已超时</div>
    </div>

    <!-- 利用率图表 -->
    <div ref="chartRef" class="w-full h-64 border rounded-xl" />

    <div class="flex gap-4">
      <!-- 热力图容器 -->
      <div class="w-full border rounded-xl bg-white p-4">
        <div v-if="!heatmap.length || !timeSlots.length" class="p-8 text-center text-gray-500">
          暂无数据，请选择房间并等待加载
        </div>
      
        <div v-else class="overflow-auto max-h-[45vh] p-2">
        
          <!-- 顶部座位编号 -->
          <div class="flex sticky top-0 bg-white z-10">
            <div class="w-16"></div>
            <div class="grid gap-0.5" :style="{ gridTemplateColumns: `repeat(${heatmap.length}, 20px)` }">
              <div
                v-for="row in heatmap"
                :key="row.seat.id"
                class="text-[10px] text-gray-600 origin-bottom-left rotate-45"
              >
                {{ row.seat.row }}-{{ row.seat.col }}
              </div>
            </div>
          </div>
        
          <!-- 热力图主体 -->
          <div class="space-y-1 mt-1">
            <div
              v-for="(slot, slotIndex) in timeSlots"
              :key="slot.start"
              class="flex items-center gap-x-1"
            >
              <!-- 左侧时间标签 -->
              <div class="w-16 text-xs text-right pr-1 text-gray-700">
                {{ slot.label }}
              </div>
            
              <!-- 每行的格子 -->
              <div class="grid gap-0.5" :style="{ gridTemplateColumns: `repeat(${heatmap.length}, 20px)` }">
                <div
                  v-for="row in heatmap"
                  :key="row.seat.id"
                  class="w-4 h-4 rounded-[3px] cursor-pointer relative"
                  :class="{
                    'bg-gray-100': row.cells[slotIndex]?.state === '空闲',
                    'bg-blue-200': row.cells[slotIndex]?.state === '已预约',
                    'bg-green-300': row.cells[slotIndex]?.state === '已签到',
                    'bg-gray-300': row.cells[slotIndex]?.state === '已签退',
                    'bg-yellow-200': row.cells[slotIndex]?.state === '已取消',
                    'bg-red-300': row.cells[slotIndex]?.state === '已超时'
                  }"
                  @mouseenter="safeCellEnter(row.seat.id, slotIndex)"
                  @mouseleave="onCellLeave"
                  @click="safeCellClick(row.seat.id, slotIndex)"
                >
                  <!-- 悬浮提示 -->
                  <div
                    v-if="hoverCell && hoverCell.seat.id === row.seat.id && hoverCell.slotIndex === slotIndex"
                    class="absolute bg-black text-white text-xs p-1 rounded shadow-lg z-50 -top-10 left-1/2 -translate-x-1/2 whitespace-nowrap"
                  >
                    <div>座位：{{ row.seat.row }}-{{ row.seat.col }}</div>
                
                    <div>
                      时间：
                      <template v-if="hoverCell.booking">
                        {{ formatTimeOnly(hoverCell.booking.startTime) }} - {{ formatTimeOnly(hoverCell.booking.endTime) }}
                      </template>
                      <template v-else>
                        {{ slot.start }} - {{ slot.end }}
                      </template>
                    </div>
                  
                    <template v-if="hoverCell.booking">
                      <div>用户：{{ hoverCell.booking.user?.userName }}</div>
                      <div>状态：{{ hoverCell.booking.state }}</div>
                    </template>
                  
                    <template v-else>
                      <div>状态：空闲</div>
                    </template>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 详情面板 -->
      <div v-if="selectedCell" class="w-200 p-4 border rounded-xl space-y-2 bg-gray-50">
        <h3 class="font-bold">预约详情</h3>

        <template v-if="selectedBooking">
          <div class="space-y-2 text-sm">
            <!-- 预约 ID -->
            <div class="flex items-center gap-2">
              <strong>预约 ID：</strong>
              <span>{{ selectedBooking.id }}</span>
              <Copy class="size-4 cursor-pointer" @click="copyText(selectedBooking.id)" />
            </div>
            <!-- 用户 -->
            <div class="flex items-center gap-2">
              <strong>用户：</strong>
              <span>{{ selectedBooking.user?.userName }}</span>
              <Copy class="size-4 cursor-pointer" @click="copyText(selectedBooking.user?.userName ?? '')" />
            </div>
            <!-- 用户 ID -->
            <div class="flex items-center gap-2">
              <strong>用户 ID：</strong>
              <span>{{ selectedBooking.userId }}</span>
              <Copy class="size-4 cursor-pointer" @click="copyText(selectedBooking.userId)" />
            </div>
            <!-- 座位 ID -->
            <div class="flex items-center gap-2">
              <strong>座位 ID：</strong>
              <span>{{ selectedBooking.seatId }}</span>
              <Copy class="size-4 cursor-pointer" @click="copyText(selectedBooking.seatId)" />
            </div>
            <!-- 座位 -->
            <div>
              <strong>座位：</strong>
              {{ selectedBooking.seat?.row }}-{{ selectedBooking.seat?.col }}
            </div>
            <!-- 开始时间 -->
            <div>
              <strong>开始时间：</strong>
              {{ formatDate(selectedBooking.startTime) }}
            </div>
            <!-- 结束时间 -->
            <div>
              <strong>结束时间：</strong>
              {{ formatDate(selectedBooking.endTime) }}
            </div>
            <!-- 状态 -->
            <div>
              <strong>状态：</strong>
              {{ selectedBooking.state }}
            </div>
          </div>


          <div class="flex gap-2 mt-3">
            <Button class="bg-green-500 hover:bg-green-500 hover:brightness-110 text-white" @click="checkIn(selectedBooking.id)">签到</Button>
            <Button class="bg-gray-500 hover:bg-gray-500 hover:brightness-110 text-white" @click="checkOut(selectedBooking.id)">签退</Button>
            <Button class="bg-red-600 hover:bg-red-600 hover:brightness-110 text-white" @click="openDeleteDialog(selectedBooking.id)">删除预约</Button>
          </div>
        </template>

        <template v-else>
          <div class="text-muted-foreground">该时间段当前无预约（空闲）</div>
        </template>
      </div>
    </div>

    <!-- 删除确认弹窗 -->
    <Dialog v-model:open="isDeleteDialogOpen">
      <DialogContent>
        <DialogHeader>
          <DialogTitle>确认删除</DialogTitle>
        </DialogHeader>
        <p class="text-sm text-muted-foreground">确定要删除这个预约吗？此操作不可恢复。</p>
        <DialogFooter>
          <Button class="hover:brightness-90" variant="secondary" @click="isDeleteDialogOpen = false">取消</Button>
          <Button
            class="bg-red-600 hover:bg-red-600 hover:brightness-90 text-white flex items-center gap-x-2"
            :disabled="deletingBookingId === pendingDeleteBookingId"
            @click="confirmDeleteBooking"
          >
            <Loader2 v-if="deletingBookingId === pendingDeleteBookingId" class="size-4 animate-spin" />
            {{ deletingBookingId === pendingDeleteBookingId ? '删除中...' : '确认删除' }}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  </div>
</template>


<style scoped>
::-webkit-scrollbar { 
  width: 6px; 
  height: 6px; 
}

::-webkit-scrollbar-thumb { 
  background-color: #e0e0e0; 
  border-radius: 3px; 
}

input[type="date"]::-webkit-calendar-picker-indicator { 
  opacity: 0; 
}

.absolute .text-gray-500 { 
  pointer-events: none; 
}
</style>
