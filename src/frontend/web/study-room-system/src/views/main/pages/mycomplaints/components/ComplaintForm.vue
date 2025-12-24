<script setup lang="ts">
import { computed, watch, reactive, ref } from "vue";
import dayjs from "dayjs";
import { toast } from "vue-sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { CalendarIcon } from "lucide-vue-next";
import { Calendar } from "@/components/ui/calendar";
import type { Room } from "@/lib/types/Room";
import type { Complaint } from "@/lib/types/Complaint";
import { roomRequest } from "@/lib/api/roomRequest";
import type { CalendarRootProps } from "reka-ui";
import { parseDate } from "@internationalized/date";

type Mode = "create" | "edit";
type CalendarModelValue = CalendarRootProps["modelValue"];

interface FormState {
  roomId: string;
  seatId: string;
  seatManual: string;
  type: string;
  content: string;
  hour: string;
  minute: string;
}

const props = defineProps<{
  mode: Mode;
  rooms: Room[];
  loadingSeats: boolean;
  complaint?: Complaint | null;
}>();

const emit = defineEmits<{
  (e: "submit", payload: { seatId?: string; type: string; content: string; targetTime?: string | null }): void;
  (e: "delete"): void;
  (e: "cancel"): void;
}>();

const form = reactive<FormState>({
  roomId: "",
  seatId: "",
  seatManual: "",
  type: "",
  content: "",
  hour: "",
  minute: "",
});

// 日历当前值（与 Calendar v-model 绑定）
const calendarValue = ref<CalendarModelValue>();
const dateText = computed(() => {
  const ymd = getYMD(calendarValue.value);
  if (!ymd) return "选择日期";
  const mm = String(ymd.month).padStart(2, "0");
  const dd = String(ymd.day).padStart(2, "0");
  return `${ymd.year}-${mm}-${dd}`;
});

const titleText = computed(() => (props.mode === "create" ? "发起投诉" : "修改投诉"));
const submitText = computed(() => (props.mode === "create" ? "提交投诉" : "保存修改"));

const roomOptions = computed(() => props.rooms ?? []);
const seatOptions = ref<{ id: string; label: string }[]>([]);
const seatLoading = ref(false);

async function loadSeatsForRoom(roomId: string) {
  if (!roomId) {
    seatOptions.value = [];
    return;
  }
  seatLoading.value = true;
  try {
    const res = await roomRequest.getRoom(roomId);
    const room = (res as any)?.data;
    const seats = room?.seats ?? [];
    seatOptions.value = seats
      .slice()
      .sort((a: any, b: any) => {
        const cols = room?.cols ?? 0;
        const idxA = (a.row ?? 0) * cols + (a.col ?? 0);
        const idxB = (b.row ?? 0) * cols + (b.col ?? 0);
        return idxA - idxB;
      })
      .map((s: any) => {
        const idx = (s.row ?? 0) * (room?.cols ?? 0) + (s.col ?? 0) + 1;
        return { id: s.id, label: `${room?.name ?? ""} - ${idx}` };
      });
  } catch (err) {
    console.error("加载房间座位失败", err);
    seatOptions.value = [];
    toast.error("加载座位列表失败，请重试");
  } finally {
    seatLoading.value = false;
  }
}

// 房间切换时重置座位
watch(
  () => form.roomId,
  () => {
    form.seatId = "";
    if (props.mode === "create") {
      loadSeatsForRoom(form.roomId);
    }
  }
);

// 初始化房间默认值
watch(
  () => roomOptions.value,
  (list) => {
    if (props.mode === "create" && list.length > 0) {
      if (!form.roomId) {
        form.roomId = list[0]?.id ?? "";
      } else {
        loadSeatsForRoom(form.roomId);
      }
    }
  },
  { immediate: true }
);

watch(
  () => props.complaint,
  (c) => {
    if (props.mode === "edit" && c) {
      form.seatId = c.seatId;
      form.seatManual = "";
      form.type = c.type || "";
      form.content = c.sendContent || "";
      fillTargetFromComplaint(c.targetTime);
    } else if (props.mode === "create") {
      reset();
    }
  },
  { immediate: true }
);

function reset() {
  form.roomId = roomOptions.value[0]?.id ?? "";
  form.seatId = "";
  form.seatManual = "";
  form.type = "";
  form.content = "";
  form.hour = "";
  form.minute = "";
  calendarValue.value = undefined;
}

function handleSubmit() {
  const seatId = props.mode === "create" ? form.seatId || form.seatManual.trim() : undefined;
  const targetTime = buildTargetIso();
  emit("submit", {
    seatId,
    type: form.type,
    content: form.content,
    targetTime: targetTime ?? undefined,
  });
}

function buildTargetIso(): string | null {
  if (!calendarValue.value) return null;
  const ymd = getYMD(calendarValue.value);
  if (!ymd) return null;

  const hourNum = form.hour ? Number(form.hour) : 0;
  const minuteNum = form.minute ? Number(form.minute) : 0;

  const localDate = new Date(ymd.year, ymd.month - 1, ymd.day, hourNum, minuteNum, 0, 0);
  return dayjs(localDate).toISOString();
}

function fillTargetFromComplaint(target?: string | null) {
  if (!target) {
    calendarValue.value = undefined;
    form.hour = "";
    form.minute = "";
    return;
  }

  const d = dayjs(target);
  if (!d.isValid()) {
    calendarValue.value = undefined;
    form.hour = "";
    form.minute = "";
    return;
  }

  const y = d.year();
  const m = d.month() + 1;
  const day = d.date();
  const mm = String(m).padStart(2, "0");
  const dd = String(day).padStart(2, "0");

  calendarValue.value = parseDate(`${y}-${mm}-${dd}`) as unknown as CalendarModelValue;
  form.hour = d.format("HH");
  form.minute = d.format("mm");
}

function onDateChange(val: CalendarModelValue) {
  calendarValue.value = val;
}

function getYMD(v: CalendarModelValue | undefined): { year: number; month: number; day: number } | null {
  if (!v) return null;
  const single = Array.isArray(v) ? v[0] : v;
  const anyV = single as any;
  if (typeof anyV?.year === "number" && typeof anyV?.month === "number" && typeof anyV?.day === "number") {
    return { year: anyV.year, month: anyV.month, day: anyV.day };
  }
  if (anyV instanceof Date) {
    const d = dayjs(anyV);
    return { year: d.year(), month: d.month() + 1, day: d.date() };
  }
  return null;
}
</script>

<template>
  <div class="border rounded-lg p-4 bg-background space-y-4">
    <div class="flex items-center justify-between">
      <div class="text-base font-semibold">{{ titleText }}</div>
      <slot name="badge"></slot>
    </div>

    <div v-if="mode === 'create'" class="space-y-2">
      <div class="space-y-1">
        <div class="text-sm text-muted-foreground">房间</div>
        <Select v-model="form.roomId">
          <SelectTrigger class="h-10 w-full">
            <SelectValue placeholder="选择房间" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem v-for="r in roomOptions" :key="r.id" :value="r.id">
              {{ r.name }}
            </SelectItem>
          </SelectContent>
        </Select>
      </div>

      <div class="space-y-1">
        <div class="text-sm text-muted-foreground">座位（必填）</div>
        <Select v-model="form.seatId" :disabled="seatOptions.length === 0 || seatLoading">
          <SelectTrigger class="h-10 w-full">
            <SelectValue :placeholder="seatLoading ? '加载中...' : seatOptions.length ? '选择座位' : '该房间暂无座位'" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem v-for="s in seatOptions" :key="s.id" :value="s.id">
              {{ s.label }}
            </SelectItem>
          </SelectContent>
        </Select>
      </div>
    </div>

    <div v-else class="space-y-1 text-sm">
      <div class="text-muted-foreground">座位</div>
      <div class="font-medium">
        <slot name="seat" />
      </div>
    </div>

    <div class="space-y-2">
      <div class="text-sm text-muted-foreground">投诉标题（必填）</div>
      <Input v-model="form.type" placeholder="如：座位被占用 / 环境嘈杂" />
    </div>

    <div class="space-y-2">
      <div class="text-sm font-medium text-muted-foreground">目标时间（可选）</div>
      <div class="grid grid-cols-1 sm:grid-cols-[1fr_auto_auto] gap-2 items-center">
        <Popover>
          <PopoverTrigger as-child>
            <Button variant="outline" class="w-full justify-start text-left font-normal">
              <CalendarIcon class="mr-2 h-4 w-4" />
              <span>
                {{ dateText }}
              </span>
            </Button>
          </PopoverTrigger>
          <PopoverContent class="p-0">
            <Calendar :model-value="calendarValue" @update:modelValue="onDateChange" />
          </PopoverContent>
        </Popover>

        <Select v-model="form.hour">
          <SelectTrigger class="w-full sm:w-20">
            <SelectValue placeholder="小时" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem v-for="h in 24" :key="h" :value="String(h - 1).padStart(2, '0')">
              {{ String(h - 1).padStart(2, "0") }}
            </SelectItem>
          </SelectContent>
        </Select>

        <Select v-model="form.minute">
          <SelectTrigger class="w-full sm:w-20">
            <SelectValue placeholder="分钟" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem v-for="m in 60" :key="m" :value="String(m - 1).padStart(2, '0')">
              {{ String(m - 1).padStart(2, "0") }}
            </SelectItem>
          </SelectContent>
        </Select>
      </div>
      <div class="text-xs text-muted-foreground">
        用于说明问题发生的时间点，可只选日期或精确到分钟。
      </div>
    </div>

    <div class="space-y-2">
      <div class="text-sm text-muted-foreground">投诉内容（必填）</div>
      <textarea
        v-model="form.content"
        rows="4"
        placeholder="请描述问题..."
        class="w-full rounded-md border border-input bg-background px-3 py-2 text-sm shadow-xs transition-[color,box-shadow] focus-visible:border-ring focus-visible:ring-ring/50 focus-visible:ring-[3px] outline-none disabled:opacity-50 disabled:cursor-not-allowed"
      />
    </div>

    <div class="flex flex-col gap-2 md:flex-row">
      <Button v-if="mode === 'edit'" variant="outline" class="w-full md:flex-1" @click="emit('cancel')">
        取消
      </Button>
      <Button class="w-full md:flex-1" @click="handleSubmit">
        {{ submitText }}
      </Button>
      <Button
        v-if="mode === 'edit'"
        variant="destructive"
        class="w-full md:flex-1"
        @click="emit('delete')"
      >
        删除投诉
      </Button>
    </div>
  </div>
</template>
