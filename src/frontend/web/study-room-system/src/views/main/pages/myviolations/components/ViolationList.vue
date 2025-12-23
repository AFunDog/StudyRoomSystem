<script setup lang="ts">
import type { Violation } from "@/lib/types/Violation";
import { localizeViolationType } from "@/lib/types/Violation";
import { Card, CardHeader } from "@/components/ui/card";
import dayjs from "dayjs";

const props = defineProps<{
  violations: Violation[];
  loading: boolean;
}>();

const emit = defineEmits<{
  (e: "view-detail", v: Violation): void;
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

function formatCreate(t?: string | null) {
  return t ? dayjs(t).format("YYYY/MM/DD HH:mm") : "--";
}

function hasBooking(v: Violation) {
  return !!v.bookingId;
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
          @click="emit('view-detail', v)"
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

            <div class="mt-2 text-xs text-muted-foreground">
              {{ hasBooking(v) ? "有关联预约，点击查看详情" : "无关联预约，点击查看详情" }}
            </div>
          </CardHeader>
        </Card>
      </div>
    </div>
  </div>
</template>
