<script setup lang="ts">
import type { Booking } from "@/lib/types/Booking";
import { Card, CardHeader, CardDescription } from "@/components/ui/card";
import dayjs from "dayjs";
import { localizeState } from "@/lib/types/Booking";

const props = defineProps<{
  bookings: Booking[];
  loading: boolean;
}>();

const emit = defineEmits<{
  (e: "select", booking: Booking): void;
}>();

function bookingCardClass(state: Booking["state"]) {
  switch (state) {
    case "已预约":
      return "border-l-4 border-l-sky-500/80 bg-background hover:bg-accent/40";
    case "已签到":
      return "border-l-4 border-l-emerald-500/80 bg-background hover:bg-accent/40";
    case "已签退":
      return "border-l-4 border-l-slate-400/80 bg-background opacity-80 hover:bg-accent/30";
    case "已取消":
    case "已超时":
      return "border-l-4 border-l-red-400/80 bg-background/60 opacity-70 hover:bg-accent/20";
    default:
      return "bg-background hover:bg-accent/40";
  }
}

function stateBadgeClass(state: Booking["state"]) {
  const base =
    "inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium border";

  switch (state) {
    case "已预约":
      return base + " bg-sky-50 text-sky-700 border-sky-200";
    case "已签到":
      return base + " bg-emerald-50 text-emerald-700 border-emerald-200";
    case "已签退":
      return base + " bg-slate-50 text-slate-600 border-slate-200";
    case "已取消":
    case "已超时":
      return base + " bg-red-50 text-red-600 border-red-200 line-through";
    default:
      return base + " bg-muted text-muted-foreground border-border";
  }
}

function formatRange(start?: string | null, end?: string | null) {
  if (!start || !end) return "";
  return `${dayjs(start).format("YYYY/MM/DD HH:mm")} - ${dayjs(end).format("HH:mm")}`;
}
</script>

<template>
  <div class="flex-1 min-h-0 h-full">
    <div v-if="loading" class="w-full h-full flex items-center justify-center text-sm text-muted-foreground">
      正在加载预约...
    </div>

    <div
      v-else-if="bookings.length === 0"
      class="w-full h-full flex items-center justify-center text-sm text-muted-foreground"
    >
      暂无相关预约记录
    </div>

    <div
      v-else
      class="flex flex-col min-w-0 gap-y-2 max-w-full flex-nowrap overflow-y-auto h-full rounded-md"
    >
      <div v-for="b in bookings" :key="b.id">
        <Card
          class="py-2 transition-colors cursor-pointer"
          :class="bookingCardClass(b.state)"
          @click="emit('select', b)"
        >
          <CardHeader>
            <div class="flex flex-row gap-x-2 items-center">
              <div class="text-lg">
                {{ b.seat?.room?.name || "未知房间" }}
              </div>
              <div class="text-lg">-</div>
              <div class="text-lg">
                {{
                  (b.seat?.row ?? 0) * (b.seat?.room?.cols ?? 0) +
                  (b.seat?.col ?? 0) + 1
                }}
              </div>

              <div class="flex flex-row items-center justify-center ml-auto text-sm">
                <span :class="stateBadgeClass(b.state)">
                  {{ localizeState(b.state) }}
                </span>
              </div>
            </div>

            <CardDescription>
              <div>
                {{ formatRange(b.startTime, b.endTime) }}
              </div>
            </CardDescription>
          </CardHeader>
        </Card>
      </div>
    </div>
  </div>
</template>
