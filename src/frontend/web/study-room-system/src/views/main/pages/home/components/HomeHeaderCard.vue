<script setup lang="ts">
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { CalendarDays, RotateCw } from "lucide-vue-next";

const props = defineProps<{
  todayText: string;
  displayName: string;
  creditScore: number | null;
  creditText: string;
  creditHint: string;
  creditBoxClass: string;
  todayCount: number;
  weekCount: number;
  canceledCount: number;
  refreshing: boolean;
}>();

const emit = defineEmits<{
  (e: "refresh"): void;
}>();
</script>

<template>
  <Card class="rounded-2xl shadow-sm border border-slate-200 bg-white">
    <div class="p-5 sm:p-6 space-y-4">
      <div class="flex items-start justify-between gap-3">
        <div class="min-w-0">
          <div class="flex items-center gap-2 text-slate-600">
            <CalendarDays class="w-4 h-4" />
            <p class="text-sm">{{ todayText }}</p>
          </div>
          <h1 class="mt-1 text-lg sm:text-2xl font-bold text-slate-900 truncate">
            你好，{{ displayName || "同学" }}
          </h1>
          <p class="text-sm text-slate-500 mt-1">
            首页展示你的当前日程、信用分与关键入口。
          </p>
        </div>

        <div class="flex flex-wrap items-center justify-end gap-2 shrink-0">
          <div
            class="px-3 py-2 rounded-xl border"
            :class="creditBoxClass"
            :title="creditHint"
          >
            <div class="text-xs opacity-80">信用分</div>
            <div class="flex items-end gap-2">
              <div class="text-lg font-semibold leading-none">
                {{ creditScore != null ? creditScore : "—" }}
              </div>
              <div class="text-xs px-2 py-0.5 rounded-full bg-white/60 border border-white/60">
                {{ creditText }}
              </div>
            </div>
            <div class="text-[11px] opacity-75 mt-1">
              {{ creditHint }}
            </div>
          </div>

          <!-- <div class="flex items-center gap-2">
            <Button
              variant="ghost"
              size="sm"
              class="flex items-center gap-1 md:hidden
                      bg-gray-100 hover:bg-gray-200
                      text-gray-600 border border-gray-200
                      disabled:opacity-60 disabled:cursor-not-allowed"
              :disabled="refreshing"
              @click="emit('refresh')"
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
              @click="emit('refresh')"
            >
              <RotateCw class="w-4 h-4" :class="refreshing ? 'animate-spin' : ''" />
            </Button>
          </div> -->
        </div>
      </div>

      <div class="grid grid-cols-3 gap-3">
        <div class="rounded-lg border border-slate-200 bg-slate-50 p-3">
          <p class="text-xs text-slate-500">今日预约</p>
          <p class="text-lg font-semibold text-slate-900">{{ todayCount }}</p>
        </div>
        <div class="rounded-lg border border-slate-200 bg-slate-50 p-3">
          <p class="text-xs text-slate-500">本周预约</p>
          <p class="text-lg font-semibold text-slate-900">{{ weekCount }}</p>
        </div>
        <div class="rounded-lg border border-slate-200 bg-slate-50 p-3">
          <p class="text-xs text-slate-500">取消/超时</p>
          <p class="text-lg font-semibold text-slate-900">{{ canceledCount }}</p>
        </div>
      </div>
    </div>
  </Card>
</template>
