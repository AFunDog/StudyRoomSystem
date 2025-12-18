<script setup lang="ts">
import type { Violation } from "@/lib/types/Violation";
import { localizeViolationType } from "@/lib/types/Violation";
import { Card, CardHeader, CardDescription } from "@/components/ui/card";
import dayjs from "dayjs";

const props = defineProps<{
  violations: Violation[];
  loading: boolean;
}>();

function badgeClass(type: Violation["type"]) {
  const base =
    "inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium border";
  switch (type) {
    case "超时":
      return base + " bg-amber-50 text-amber-700 border-amber-200";
    case "强制取消":
      return base + " bg-red-50 text-red-600 border-red-200";
    case "管理员":
      return base + " bg-slate-50 text-slate-600 border-slate-200";
    default:
      return base + " bg-muted text-muted-foreground border-border";
  }
}

function cardClass(type: Violation["type"]) {
  switch (type) {
    case "超时":
      return "border-l-4 border-l-amber-500/80";
    case "强制取消":
      return "border-l-4 border-l-red-500/80";
    case "管理员":
      return "border-l-4 border-l-slate-400/80";
    default:
      return "";
  }
}

// TODO: 处理房间和座位位置的相关信息，考虑使用按guid查询预约条目获取房间和座位信息
function formatCreate(t?: string | null) {
  return t ? dayjs(t).format("YYYY/MM/DD HH:mm") : "--";
}

function formatBookingRange(start?: string | null, end?: string | null) {
  if (!start || !end) return "";
  return `${dayjs(start).format("YYYY/MM/DD HH:mm")} - ${dayjs(end).format("HH:mm")}`;
}

function seatNumber(cols?: number | null, row?: number | null, col?: number | null) {
  if (cols == null || row == null || col == null) return "未知座位";
  return row * cols + col + 1;
}
</script>

<template>
  <div class="flex-1 min-h-0 h-full">
    <div
      v-if="loading"
      class="w-full h-full flex items-center justify-center text-sm text-muted-foreground"
    >
      正在加载违规记录...
    </div>

    <div
      v-else-if="violations.length === 0"
      class="w-full h-full flex items-center justify-center text-sm text-muted-foreground"
    >
      暂无违规记录
    </div>

    <div
      v-else
      class="flex flex-col min-w-0 gap-y-2 max-w-full flex-nowrap overflow-y-auto h-full rounded-md"
    >
      <div v-for="v in violations" :key="v.id">
        <Card
          class="py-2 px-2 bg-background/70 transition-colors"
          :class="cardClass(v.type)"
        >

          <CardHeader>
            <div class="flex flex-row gap-x-2 items-center">
              <div class="text-base font-semibold">
                {{ localizeViolationType(v.type) }}
              </div>
              <span :class="badgeClass(v.type)">
                {{ v.type }}
              </span>
              <div class="ml-auto text-xs text-muted-foreground">
                记录时间：{{ formatCreate(v.createTime) }}
              </div>
            </div>

            <CardDescription class="text-sm text-muted-foreground mt-1 whitespace-pre-line break-words">
              {{ v.content }}
            </CardDescription>

            <div class="mt-2 text-xs text-muted-foreground flex flex-wrap gap-x-3 gap-y-1">
              <span>
                房间：{{ v.booking?.seat?.room?.name || "未知房间" }}
              </span>
              <span>
                座位：
                {{
                  seatNumber(
                    v.booking?.seat?.room?.cols ?? null,
                    v.booking?.seat?.row ?? null,
                    v.booking?.seat?.col ?? null
                  )
                }}
              </span>
              <span v-if="v.booking?.startTime && v.booking?.endTime">
                预约时间：{{ formatBookingRange(v.booking?.startTime, v.booking?.endTime) }}
              </span>
            </div>
          </CardHeader>
        </Card>
      </div>
    </div>
  </div>
</template>
