<script setup lang="ts">
import { computed } from "vue";
import dayjs from "dayjs";
import type { Complaint, ComplaintState } from "@/lib/types/Complaint";
import { localizeComplaintState } from "@/lib/types/Complaint";
import type { Room } from "@/lib/types/Room";
import { Button } from "@/components/ui/button";
import { Pencil } from "lucide-vue-next";

const props = defineProps<{
  complaints: Complaint[];
  loading: boolean;
  hasMore: boolean;
  loadingMore: boolean;
  stateFilter: string;
  rooms?: Room[];
}>();

const emit = defineEmits<{
  (e: "update:stateFilter", val: string): void;
  (e: "select", c: Complaint): void;
  (e: "load-more"): void;
}>();

const filters: Array<"all" | ComplaintState> = ["all", "已发起", "已处理", "已关闭"];

const sortedComplaints = computed(() =>
  [...props.complaints].sort((a, b) => dayjs(b.createTime).valueOf() - dayjs(a.createTime).valueOf())
);

function seatDisplay(c: Complaint) {
  const roomFromEntity = c.seat?.room?.name;
  const colsFromEntity = c.seat?.room?.cols ?? c.seat?.room?.cols;

  // 若后端未返回 room，尝试从已加载的房间列表匹配
  let room = roomFromEntity;
  let cols = colsFromEntity ?? 0;
  let row = c.seat?.row ?? 0;
  let col = c.seat?.col ?? 0;

  if (!room && props.rooms?.length) {
    for (const r of props.rooms) {
      const found = r.seats?.find((s) => s.id === c.seatId);
      if (found) {
        room = r.name;
        cols = r.cols ?? cols;
        row = found.row ?? row;
        col = found.col ?? col;
        break;
      }
    }
  }

  room = room || "未知房间";
  const seatIdx = row * (cols || 0) + col + 1;
  return `${room} - ${seatIdx}`;
}

function stateBadgeClass(state: ComplaintState) {
  const base = "inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium border";
  switch (state) {
    case "已发起":
      return base + " bg-sky-50 text-sky-700 border-sky-200";
    case "已处理":
      return base + " bg-emerald-50 text-emerald-700 border-emerald-200";
    case "已关闭":
      return base + " bg-slate-50 text-slate-600 border-slate-200";
    default:
      return base + " bg-muted text-muted-foreground border-border";
  }
}

function cardClass(state: ComplaintState) {
  switch (state) {
    case "已发起":
      return "border-l-4 border-l-sky-400/80 border border-sky-100";
    case "已处理":
      return "border-l-4 border-l-emerald-500/80 border border-emerald-100";
    case "已关闭":
      return "border-l-4 border-l-slate-400/80 border border-slate-100";
    default:
      return "border";
  }
}
</script>

<template>
  <div class="flex flex-col gap-3 h-full">
    <div class="flex flex-col gap-3 md:flex-row md:items-center md:gap-4">
      <div class="flex items-center gap-2 flex-wrap">
        <span class="text-sm text-muted-foreground">状态</span>
        <div class="flex flex-wrap gap-2">
          <Button
            v-for="item in filters"
            :key="item"
            :variant="stateFilter === item ? 'default' : 'outline'"
            size="sm"
            @click="emit('update:stateFilter', item)"
          >
            {{ item === "all" ? "全部" : item }}
          </Button>
        </div>
      </div>
    </div>

    <div class="flex flex-col min-h-0 h-full">
      <div v-if="loading" class="flex h-full items-center justify-center text-sm text-muted-foreground">
        正在加载投诉...
      </div>
      <div
        v-else-if="sortedComplaints.length === 0"
        class="flex h-full items-center justify-center text-sm text-muted-foreground"
      >
        暂无投诉记录
      </div>
      <div
        v-else
        class="flex flex-col min-w-0 gap-y-2 max-w-full flex-nowrap overflow-y-auto flex-1 rounded-md"
      >
        <div
          v-for="(c, idx) in sortedComplaints"
          :key="c.id"
          class="rounded-lg p-3 bg-background cursor-pointer hover:bg-accent/60 transition-colors"
          :class="cardClass(c.state)"
          @click="emit('select', c)"
        >
          <div class="flex items-start gap-2">
            <div class="text-base text-slate-600 font-semibold min-w-[1.5rem]">
              {{ idx + 1 }}.
            </div>
            <div class="text-base font-medium flex-1">
              {{ c.type || "无" }}
            </div>
            <span class="ml-auto" :class="stateBadgeClass(c.state)">{{ localizeComplaintState(c.state) }}</span>
          </div>
          <div class="mt-1 text-sm text-muted-foreground">
            位置：{{ seatDisplay(c) }}
          </div>
          <div class="mt-2 text-xs text-muted-foreground flex flex-wrap gap-3">
            <span>创建：{{ dayjs(c.createTime).format("YYYY/MM/DD HH:mm") }}</span>
            <span v-if="c.handleTime">处理：{{ dayjs(c.handleTime).format("YYYY/MM/DD HH:mm") }}</span>
            <span v-if="c.handleContent">处理内容：{{ c.handleContent }}</span>
          </div>
          <div class="mt-2 flex items-center text-xs text-primary gap-1">
            <Pencil class="w-3 h-3" />
            点击修改或查看详情
          </div>
        </div>
      </div>

      <div v-if="hasMore" class="mt-3 flex justify-center">
        <Button variant="outline" size="sm" :disabled="loadingMore" @click="emit('load-more')">
          {{ loadingMore ? "加载中..." : "加载更多" }}
        </Button>
      </div>
      <div v-else class="mt-1 flex justify-center text-sm text-muted-foreground">
        没有更多了
      </div>
    </div>
  </div>
</template>
