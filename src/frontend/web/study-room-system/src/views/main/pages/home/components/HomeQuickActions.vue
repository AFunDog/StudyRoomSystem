<script setup lang="ts">
import { Card } from "@/components/ui/card";

export interface QuickLink {
  label: string;
  path: string;
  value: string;
  hint: string;
  icon?: any;
}

const props = defineProps<{
  quickLinks: QuickLink[];
}>();

const emit = defineEmits<{
  (e: "select", path: string): void;
}>();
</script>

<template>
  <!-- 移动端 1 列，sm 2 列，lg 4 列 -->
  <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
    <Card
      v-for="item in quickLinks"
      :key="item.path"
      class="hover:-translate-y-0.5 hover:shadow-md transition cursor-pointer border border-slate-200/80 rounded-xl bg-white"
      @click="emit('select', item.path)"
    >
      <!-- 用自定义容器替代 CardHeader/CardContent，更好控制移动端排布 -->
      <div class="p-4 h-full flex flex-col gap-3">
        <!-- 顶部 -->
        <div class="flex items-start justify-between gap-3">
          <div class="flex items-center gap-2 min-w-0">
            <component
              v-if="item.icon"
              :is="item.icon"
              class="w-4 h-4 text-slate-700 shrink-0"
            />
            <div class="text-[15px] sm:text-base font-semibold text-slate-900 leading-snug truncate">
              {{ item.label }}
            </div>
          </div>

          <span
            v-if="item.value"
            class="shrink-0 text-[11px] px-2 py-1 rounded-full bg-slate-100 text-slate-600 leading-none"
          >
            {{ item.value }}
          </span>
        </div>

        <!-- 底部 -->
        <div class="text-sm text-slate-500 leading-relaxed mt-auto">
          {{ item.hint }}
        </div>
      </div>
    </Card>
  </div>
</template>
