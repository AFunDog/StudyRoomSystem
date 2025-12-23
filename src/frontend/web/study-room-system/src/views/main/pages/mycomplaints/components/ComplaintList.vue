<script setup lang="ts">
import { computed } from "vue";
import dayjs from "dayjs";
import type { Complaint, ComplaintState } from "@/lib/types/Complaint";
import { localizeComplaintState } from "@/lib/types/Complaint";
import { Button } from "@/components/ui/button";
import { Pencil } from "lucide-vue-next";

const props = defineProps<{
  complaints: Complaint[];
  loading: boolean;
  hasMore: boolean;
  loadingMore: boolean;
  stateFilter: string;
}>();

const emit = defineEmits<{
  (e: "update:stateFilter", val: string): void;
  (e: "select", c: Complaint): void;
  (e: "load-more"): void;
}>();

const filters = ["all", "已发起", "已处理", "已关闭"];

const sortedComplaints = computed(() =>
  [...props.complaints].sort(
    (a, b) => dayjs(b.createTime).valueOf() - dayjs(a.createTime).valueOf()
  )
);

function seatDisplay(c: Complaint) {
  const room = c.seat?.room?.name || "未知房间";
  const seatIdx =
    (c.seat?.row ?? 0) * (c.seat?.room?.cols ?? 0) + (c.seat?.col ?? 0) + 1;
  return `${room} · ${seatIdx}`;
}

function stateBadgeClass(state: ComplaintState) {
  const base =
    "inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium border";
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
</script>

<template>
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
          {{ item === 'all' ? '全部' : item }}
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
      class="flex flex-col min-w-0 gap-y-2 max-w-full flex-nowrap overflow-y-auto h-full rounded-md"
    >
      <div
        v-for="c in sortedComplaints"
        :key="c.id"
        class="border rounded-lg p-3 bg-background cursor-pointer hover:bg-accent/60 transition-colors"
        @click="emit('select', c)"
      >
        <div class="flex items-start gap-2">
          <div class="text-base font-medium">{{ seatDisplay(c) }}</div>
          <div class="flex-1" />
          <span :class="stateBadgeClass(c.state)">{{ localizeComplaintState(c.state) }}</span>
        </div>
        <div class="mt-1 text-sm text-muted-foreground">
          标题：{{ c.type }}
        </div>
        <div class="mt-1 text-sm text-muted-foreground whitespace-pre-wrap">
          {{ c.sendContent }}
        </div>
        <div class="mt-2 text-xs text-muted-foreground flex flex-wrap gap-3">
          <span>创建：{{ dayjs(c.createTime).format("YYYY/MM/DD HH:mm") }}</span>
          <span v-if="c.handleTime">处理：{{ dayjs(c.handleTime).format("YYYY/MM/DD HH:mm") }}</span>
          <span v-if="c.handleContent">处理内容：{{ c.handleContent }}</span>
        </div>
        <div class="mt-2 flex items-center text-xs text-primary gap-1">
          <Pencil class="w-3 h-3" />
          点击修改
        </div>
      </div>
    </div>

    <div v-if="hasMore" class="mt-3 flex justify-center">
      <Button
        variant="outline"
        size="sm"
        :disabled="loadingMore"
        @click="emit('load-more')"
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
  </div>
</template>
