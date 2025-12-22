<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch } from "vue";
import { debounce } from 'lodash';
import { toast } from "vue-sonner";
import { CalendarIcon } from 'lucide-vue-next';
import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem
} from "@/components/ui/select";
import { userRequest } from "@/lib/api/userRequest";
import { bookingRequest } from "@/lib/api/bookingRequest";
import { roomRequest } from "@/lib/api/roomRequest";
import * as echarts from "echarts";
import { Button } from "@/components/ui/button";

import type { Booking } from "@/lib/types/Booking";
import type { Room } from "@/lib/types/Room";
import type { Seat } from "@/lib/types/Seat";


/* -------------------------------------------------------
自定义类型
------------------------------------------------------- */
interface TimeSlot {
  start: string;
  end: string;
  label: string;
}

type CellState =
  | "free"
  | "reserved"
  | "checked_in"
  | "checked_out"
  | "canceled";

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
新增：北京时间转换工具函数（核心修复时区问题）
作用：将任意 Date 转为 北京时间的 YYYY-MM-DD 格式，解决 UTC 偏移问题
------------------------------------------------------- */
const getBeijingDate = (date?: Date): string => {
  const targetDate = date || new Date();
  // 计算北京时间偏移：8小时 - 本地时区偏移（分钟转毫秒）
  const beijingTime = new Date(targetDate.getTime() + (8 * 60 - targetDate.getTimezoneOffset()) * 60 * 1000);
  // 非空断言消除 TS 报错（此场景下不会为 undefined）
  return beijingTime.toISOString().split("T")[0]!;
};



/* -------------------------------------------------------
1. 页面状态管理
------------------------------------------------------- */

let chartInstance: echarts.ECharts | null = null;

// 房间列表
const rooms = ref<Room[]>([]);

// 当前选中的房间 ID
const selectedRoomId = ref<string>("");

// 修改：默认值改为北京时间今天，替换原 UTC 日期逻辑
const selectedDate = ref<string>(getBeijingDate());

// 日期变更事件处理：原生 input date 触发
function onDateChange(e: Event) {
  const target = e.target as HTMLInputElement;
  if (target.value) {
    selectedDate.value = target.value;
    loadAll();
  }
}

// 当前房间的座位列表
const seats = ref<Seat[]>([]);

// 当前房间在指定日期的所有预约
const bookings = ref<Booking[]>([]);

// 半小时时间段列表（根据房间开放时间生成）
const timeSlots = ref<TimeSlot[]>([]);

// 热力图中被点击的单元格
const selectedCell = ref<HeatmapCell | null>(null);

// 被点击单元格对应的预约（可能为空）
const selectedBooking = ref<Booking | null>(null);

/* -------------------------------------------------------
2. 生成半小时时间段
    输入：房间 openTime / closeTime（格式 HH:mm:ss）
    输出：[{ start: "08:00", end: "08:30", label: "08:00-08:30" }, ...]
------------------------------------------------------- */
function generateTimeSlots(openTime: string, closeTime: string, labelInterval = 2): TimeSlot[] {
  // 参数兜底，解决 room.openTime/closeTime 可能为 undefined 的 TS 报错
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

  while (h < endH || (h === endH && m < endM)) {
    const start = `${String(h).padStart(2, "0")}:${String(m).padStart(2, "0")}`;

    m += 30;
    if (m >= 60) {
      m = 0;
      h++;
    }

    const end = `${String(h).padStart(2, "0")}:${String(m).padStart(2, "0")}`;

    // 每隔 labelInterval 小时显示一次 label（即每 2 小时显示一次）
    const showLabel = slotCount % (labelInterval * 2) === 0;
    const label = showLabel ? `${start}-${end}` : "";

    result.push({ start, end, label });
    slotCount++;
  }

  return result;
}



/* -------------------------------------------------------
3. 加载房间、座位、预约数据
------------------------------------------------------- */
async function loadAll(): Promise<void> {
  if (!selectedRoomId.value) return;

  try {
    // 找到当前房间
    const room = rooms.value.find(r => r.id === selectedRoomId.value);
    if (!room) return;

    // 根据房间开放时间生成半小时时间段
    timeSlots.value = generateTimeSlots(room.openTime, room.closeTime, 1);

    // 获取房间详情（包含 seats）
    const roomDetail = await roomRequest.getRoom(selectedRoomId.value);
    seats.value = roomDetail.data.seats ?? [];

    // 获取所有预约（管理员接口）
    const result = await bookingRequest.getAllBookings(1, 100);
    const allBookings: Booking[] = result.items;

    // 补全每个预约的用户信息
    for (const b of allBookings) {
      if (!b.user) {
        try {
          const userRes = await userRequest.getUserById(b.userId);
          // console.log("加载预约用户信息：", userRes.data);
          b.user = userRes.data;
        } catch (err){
          console.error(`加载用户 ${b.userId} 信息失败:`, err);
        }
      }
    }

    // 过滤预约时，用北京时间匹配，解决时区不一致导致的过滤失效
    bookings.value = allBookings.filter((b: Booking) => {
      if (!b.seat?.room?.id || b.seat.room.id !== selectedRoomId.value) return false;
      // 预约时间转为北京时间，再和 selectedDate 比较
      const bookingDate = getBeijingDate(new Date(b.startTime));
      return bookingDate === selectedDate.value;
    });

    // 渲染房间利用率图表
    renderUsageChart();

  // 后台日志打印
  // console.log("当前预约列表：", bookings.value);
  } catch (error) {
    toast.error("加载数据失败，请重试");
    console.error("loadAll 执行错误：", error);
  }
}


/* -------------------------------------------------------
4. 构建热力图矩阵
    - 行：座位
    - 列：半小时时间段
    - 单元格：该座位在该时间段的状态
------------------------------------------------------- */
const heatmap = computed<HeatmapRow[]>(() => {
  return seats.value.map(seat => {
    const cells: HeatmapCell[] = timeSlots.value.map((slot, index) => {
      // 时间段起始时间转为北京时间，解决预约匹配错位
      const slotStart = new Date(`${selectedDate.value}T${slot.start}:00`);
      const slotStartBeijing = new Date(slotStart.getTime() + (8 * 60 - slotStart.getTimezoneOffset()) * 60 * 1000);
      // 计算时间段结束时间（半小时后）
      const slotEndBeijing = new Date(slotStartBeijing.getTime() + 30 * 60 * 1000);

      // 使用标准时间段重叠判断逻辑，提高匹配准确性
      const booking = bookings.value.find(b => {
        const bookStart = new Date(b.startTime);
        const bookEnd = new Date(b.endTime);
        // 重叠条件：预约开始 < 时间段结束 且 预约结束 > 时间段开始
        return bookStart < slotEndBeijing && bookEnd > slotStartBeijing;
      });

      // 默认状态：空闲
      let state: CellState = "free";

      // 根据预约状态映射颜色（后端：Booked | CheckIn | Checkout | Canceled）
      if (booking) {
        switch (booking.state) {
          case "Booked":
            state = "reserved";        // 已预约
            break;
          case "CheckIn":
            state = "checked_in";      // 已签到
            break;
          case "Checkout":
            state = "checked_out";     // 已签退
            break;
          case "Canceled":
            state = "canceled";        // 已取消
            break;
        }
      }

      return {
        seat,
        slot,
        slotIndex: index,
        booking: booking ?? null,
        state
      };
    });

    return {
      seat,
      cells
    };
  });
});


/* -------------------------------------------------------
4.1 转置访问函数
------------------------------------------------------- */
function getCell(seatId: string, slotIndex: number): HeatmapCell | null {
  const row = heatmap.value.find((r) => r.seat.id === seatId);
  if (!row) return null;

  const cell = row.cells[slotIndex];
  return cell ?? null;
}

function getCellState(seatId: string, slotIndex: number): CellState {
  return getCell(seatId, slotIndex)?.state ?? "free";
}

/* -------------------------------------------------------
4.2 悬停延迟 Tooltip
------------------------------------------------------- */
const hoverCell = ref<HeatmapCell | null>(null)
const hoverTimer = ref<number | null>(null)

function onCellEnter(cell: HeatmapCell): void {
  hoverTimer.value = window.setTimeout(() => {
    hoverCell.value = cell
  }, 300)
}

function onCellLeave(): void {
  if (hoverTimer.value) {
    clearTimeout(hoverTimer.value)
  }
  hoverCell.value = null
}

function safeCellEnter(seatId: string, slotIndex: number): void {
  const cell = getCell(seatId, slotIndex);
  if (cell) onCellEnter(cell);
}

function safeCellClick(seatId: string, slotIndex: number): void {
  const cell = getCell(seatId, slotIndex);
  if (cell) handleCellClick(cell);
}

/* -------------------------------------------------------
5. 点击热力图单元格
------------------------------------------------------- */
function handleCellClick(cell: HeatmapCell): void {
  selectedCell.value = cell;
  selectedBooking.value = cell.booking;
}


/* -------------------------------------------------------
6. 管理员操作（取消预约 / 签到 / 签退）
------------------------------------------------------- */
async function cancelBooking(id: string):Promise<void> {
  try{
    const res = await bookingRequest.cancelBooking(id, true);
    toast.success(res.message);
    await loadAll();
  } catch (error) {
    toast.error("取消预约失败，请重试");
    console.error("取消预约失败：", error);
  }
}

async function checkIn(id: string):Promise<void> {
  try{
    await bookingRequest.checkIn({ id });
    toast.success("已签到");
    await loadAll();
  } catch (error) {
    toast.error("签到失败，请重试");
    console.error("签到失败：", error);
  }
}

async function checkOut(id: string):Promise<void> {
  try{
    await bookingRequest.checkOut({ id });
    toast.success("已签退");
    await loadAll();
  } catch (error) {
    toast.error("签退失败，请重试");
    console.error("签退失败：", error);
  }
}


/* -------------------------------------------------------
7. 房间利用率统计图（ECharts）
------------------------------------------------------- */
const chartRef = ref<HTMLDivElement | null>(null);

function renderUsageChart(): void {
  // 空值判断，防止 DOM 未挂载或无数据时渲染报错
  if (!chartRef.value || !seats.value.length || !timeSlots.value.length) {
    if (chartInstance) chartInstance.dispose();
    chartInstance = null;
    return;
  }

  if (chartInstance) chartInstance.dispose();
  chartInstance = echarts.init(chartRef.value);

  // 1. 过滤掉取消的预约
  const validBookings = bookings.value.filter(
    b => b.state !== "Canceled"
  );

  // 2. 总格子数 = 座位数 * 时间段数
  const totalCells = seats.value.length * timeSlots.value.length || 1;

  // 3. 实际被占用的格子数（精确到半小时）
  let usedCells = 0;

  for (const slot of timeSlots.value) {
    // 时间段转为北京时间，解决利用率计算错位
    const slotStart = new Date(`${selectedDate.value}T${slot.start}:00`);
    const slotStartBeijing = new Date(slotStart.getTime() + (8 * 60 - slotStart.getTimezoneOffset()) * 60 * 1000);
    const slotEndBeijing = new Date(slotStartBeijing.getTime() + 30 * 60 * 1000);

    for (const seat of seats.value) {
      const hasBooking = validBookings.some(b => {
        const bookStart = new Date(b.startTime);
        const bookEnd = new Date(b.endTime);
        return bookStart < slotEndBeijing && bookEnd > slotStartBeijing;
      });
      if (hasBooking) usedCells++;
    }
  }

  const usageRate = (usedCells / totalCells * 100).toFixed(1);

  // 4. 每个时间段的占用率（和之前类似，只是用 validBookings）
  const slotUsage = timeSlots.value.map(slot => {
    // 时间段转为北京时间，解决图表数据错位
    const slotStart = new Date(`${selectedDate.value}T${slot.start}:00`);
    const slotStartBeijing = new Date(slotStart.getTime() + (8 * 60 - slotStart.getTimezoneOffset()) * 60 * 1000);
    const slotEndBeijing = new Date(slotStartBeijing.getTime() + 30 * 60 * 1000);

    const count = validBookings.filter(b => {
      const bookStart = new Date(b.startTime);
      const bookEnd = new Date(b.endTime);
      return bookStart < slotEndBeijing && bookEnd > slotStartBeijing;
    }).length;

    return seats.value.length
      ? (count / seats.value.length) * 100
      : 0;
  });

  // 本地化时间用于展示
  const xLabels = timeSlots.value.map(slot => {
    const d = new Date(`${selectedDate.value}T${slot.start}:00`);
    const beijingD = new Date(d.getTime() + (8 * 60 - d.getTimezoneOffset()) * 60 * 1000);
    return beijingD.toLocaleTimeString("zh-CN", {
      hour: "2-digit",
      minute: "2-digit"
    });
  });

  chartInstance.setOption({
    title: { text: `房间利用率：${usageRate}%` },
    tooltip: { trigger: "axis" },
    // X轴标签旋转，防止重叠
    xAxis: { type: "category", data: xLabels, axisLabel: { rotate: 30 } },
    yAxis: { type: "value", max: 100, axisLabel: { formatter: "{value}%" } },
    series: [
      {
        name: "占用率",
        type: "line",
        data: slotUsage,
        smooth: true,
        areaStyle: { opacity: 0.3 }
      }
    ]
  });
}


/* -------------------------------------------------------
8. 监听房间/日期变化自动刷新
------------------------------------------------------- */
// 防抖处理，50ms 内只执行一次
const resizeHandler = debounce(() => {
  if (chartInstance) renderUsageChart();
}, 50);

watch(() => window.innerWidth, resizeHandler);

// 组件卸载时清除防抖
onUnmounted(() => {
  resizeHandler.cancel();
  if (chartInstance) chartInstance.dispose();
});


/* -------------------------------------------------------
9. 页面初始化
------------------------------------------------------- */
onMounted(async () => {
  try{
    const res = await roomRequest.getRooms();
    rooms.value = res.data;

    if (rooms.value.length > 0) {
      selectedRoomId.value = rooms.value[0]?.id ?? "";
    }
    await loadAll();
  } catch (error) {
    toast.error("初始化失败，请重试");
    console.error("初始化失败：", error);
  }
});
// 监听窗口大小变化，自动重绘图表，适配响应式布局
watch(() => window.innerWidth, () => {
  if (chartInstance) renderUsageChart();
});
</script>

<template>
  <div class="space-y-4">

    <!-- 顶部控制区：房间选择 + 日期选择 -->
    <div class="flex items-center gap-4">
      <!-- 房间选择 -->
      <div class="flex flex-col gap-1">
        <div class="text-sm font-medium">房间</div>
      
        <Select v-model="selectedRoomId" @update:modelValue="loadAll">
          <SelectTrigger class="w-full md:w-55">
            <SelectValue placeholder="选择房间" />
          </SelectTrigger>
        
          <SelectContent>
            <SelectItem
              v-for="r in rooms"
              :key="r.id"
              :value="r.id"
            >
              {{ r.name }}
            </SelectItem>
          </SelectContent>
        </Select>
      </div>
    
      <!-- 原生日期选择器 -->
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
    
      <div class="flex items-center gap-1">
        <div class="w-4 h-4 bg-gray-100 border rounded" /> 空闲
      </div>
    
      <div class="flex items-center gap-1">
        <div class="w-4 h-4 bg-blue-200 rounded" /> 已预约
      </div>
    
      <div class="flex items-center gap-1">
        <div class="w-4 h-4 bg-green-300 rounded" /> 已签到
      </div>
    
      <div class="flex items-center gap-1">
        <div class="w-4 h-4 bg-gray-300 rounded" /> 已签退
      </div>
    
      <div class="flex items-center gap-1">
        <div class="w-4 h-4 bg-yellow-200 rounded" /> 已取消
      </div>
    
    </div>

    <!-- 房间利用率统计图 -->
    <div ref="chartRef" class="w-full h-64 border rounded-xl" />

    <!-- 热力图容器 -->
    <div class="w-full border rounded-xl bg-white p-4">
      <div class="overflow-auto max-h-[45vh] p-2">
      
        <!-- 顶部：座位轴（横向） -->
        <div class="flex sticky top-0 bg-white z-10">
          <div class="w-16"></div> <!-- 占位：时间轴 -->
          <div
            class="grid gap-0.5"
            :style="{ gridTemplateColumns: `repeat(${seats.length}, 20px)` }"
          >
            <div
              v-for="seat in seats"
              :key="seat.id"
              class="text-[10px] text-gray-600 origin-bottom-left rotate-45"
            >
              {{ seat.row }}-{{ seat.col }}
            </div>
          </div>
        </div>
      
        <!-- 主体：时间轴在左，座位在上 -->
        <div class="space-y-1 mt-1">
          <div
            v-for="(slot, slotIndex) in timeSlots"
            :key="slot.start"
            class="flex items-center gap-x-1"
          >
            <!-- 时间标签（纵向） -->
            <div class="w-16 text-xs text-right pr-1 text-gray-700">
              {{ slot.label }}
            </div>
          
            <!-- 小方块：每一行是一个时间段，每一列是一个座位 -->
            <div
              class="grid gap-0.5"
              :style="{ gridTemplateColumns: `repeat(${seats.length}, 20px)` }"
            >
              <div
                v-for="seat in seats"
                :key="seat.id"
                class="w-4 h-4 rounded-[3px] cursor-pointer relative group"
                :class="{
                  'bg-gray-100': getCellState(seat.id, slotIndex) === 'free',
                  'bg-blue-200': getCellState(seat.id, slotIndex) === 'reserved',
                  'bg-green-300': getCellState(seat.id, slotIndex) === 'checked_in',
                  'bg-gray-300': getCellState(seat.id, slotIndex) === 'checked_out',
                  'bg-yellow-200': getCellState(seat.id, slotIndex) === 'canceled'
                }"
                @mouseenter="safeCellEnter(seat.id, slotIndex)"
                @mouseleave="onCellLeave"
                @click="safeCellClick(seat.id, slotIndex)"
              >
                <!-- 悬浮提示（所有格子都显示） -->
                <div
                  v-if="hoverCell && hoverCell.seat.id === seat.id && hoverCell.slotIndex === slotIndex"
                  class="absolute bg-black text-white text-xs p-1 rounded shadow-lg z-50 -top-10 left-1/2 -translate-x-1/2 whitespace-nowrap"
                >
                  <div>座位：{{ seat.row }}-{{ seat.col }}</div>
                  <div>时间：{{ slot.start }}-{{ slot.end }}</div>

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

    <!-- 详情面板：点击某个单元格后显示 -->
    <div v-if="selectedCell" class="p-4 border rounded space-y-2 bg-gray-50">
      <h3 class="font-bold">预约详情</h3>

      <!-- 有预约时显示完整信息 -->
      <template v-if="selectedBooking">
        <div class="space-y-1 text-sm">
          <div><strong>预约 ID：</strong>{{ selectedBooking.id }}</div>
          <div><strong>用户：</strong>{{ selectedBooking.user?.userName }}</div>
          <div><strong>用户 ID：</strong>{{ selectedBooking.userId }}</div>

          <div><strong>房间：</strong>{{ selectedBooking.seat?.room?.name }}</div>
          <div><strong>房间 ID：</strong>{{ selectedBooking.seat?.room?.id }}</div>

          <div>
            <strong>座位：</strong>
            {{ selectedBooking.seat?.row }}-{{ selectedBooking.seat?.col }}
          </div>
          <div><strong>座位 ID：</strong>{{ selectedBooking.seatId }}</div>

          <div><strong>开始时间：</strong>{{ selectedBooking.startTime }}</div>
          <div><strong>结束时间：</strong>{{ selectedBooking.endTime }}</div>

          <div><strong>创建时间：</strong>{{ selectedBooking.createTime }}</div>
          <div><strong>签到时间：</strong>{{ selectedBooking.checkInTime }}</div>
          <div><strong>签退时间：</strong>{{ selectedBooking.checkOutTime }}</div>

          <div><strong>状态：</strong>{{ selectedBooking.state }}</div>
        </div>

        <div class="flex gap-2 mt-3">
          <Button class="bg-red-500 text-white" @click="cancelBooking(selectedBooking.id)">
            取消预约
          </Button>
          <Button class="bg-green-500 text-white" @click="checkIn(selectedBooking.id)">
            签到
          </Button>
          <Button class="bg-blue-500 text-white" @click="checkOut(selectedBooking.id)">
            签退
          </Button>
        </div>
      </template>

      <!-- 无预约时 -->
      <template v-else>
        <div class="text-muted-foreground">
          该时间段当前无预约（空闲）
        </div>
      </template>
    </div>

  </div>
</template>

<!-- 新增：优化滚动条样式，提升 UI 体验 -->
<style scoped>
::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}
::-webkit-scrollbar-thumb {
  background-color: #e0e0e0;
  border-radius: 3px;
}

/* 隐藏原生日期选择器的默认图标，避免和自定义 CalendarIcon 重复 */
input[type="date"]::-webkit-calendar-picker-indicator {
  opacity: 0;
}
/* 给日历图标添加 pointer-events-none，防止遮挡输入框点击 */
.absolute .text-gray-500 {
  pointer-events: none;
}
</style>