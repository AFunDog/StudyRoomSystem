<script setup lang="ts">
import dayjs from "dayjs";
import { Card, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Clock3, MapPin, ArrowRight, LogIn, LogOut } from "lucide-vue-next";
import type { Booking } from "@/lib/types/Booking";

const props = defineProps<{
  activeBooking: Booking | null;
  activeCountdown: string;
  acting: boolean;
  seatRowColText: (b: Booking) => string;
  seatNoText: (b: Booking) => number | null;
  statusText: (b: Booking) => string;
  statusBadgeClass: (b: Booking) => string;
}>();

const emit = defineEmits<{
  (e: "goBookings"): void;
  (e: "goSeatBooking"): void;
  (e: "checkIn"): void;
  (e: "checkOut"): void;
}>();
</script>

<template>
  <Card class="rounded-2xl shadow-sm border border-slate-200 bg-white">
    <div class="p-5 sm:p-6">
      <div class="flex items-center justify-between">
        <CardTitle class="text-lg">当前日程</CardTitle>

        <span
          v-if="activeBooking"
          class="text-xs px-2 py-1 rounded-full"
          :class="statusBadgeClass(activeBooking)"
        >
          {{ statusText(activeBooking) }}
        </span>

      </div>

      <div v-if="!activeBooking" class="mt-3 text-sm text-slate-600 space-y-2">
        <div class="text-base font-semibold text-slate-800">暂无可展示的日程</div>
        <p>当前没有“已预约/已签到且未结束”的预约。</p>
        <div class="flex flex-wrap gap-2 pt-1">
          <Button size="sm" variant="outline" @click="emit('goBookings')">
            去我的预约
          </Button>
          <Button size="sm" class="bg-orange-500 hover:bg-orange-600 text-white" @click="emit('goSeatBooking')">
            去预约
            <ArrowRight class="w-4 h-4 ml-1" />
          </Button>
        </div>
      </div>

      <div v-else class="mt-4 space-y-2">
        <div class="text-lg font-semibold text-slate-900 truncate">
          房间 : {{ activeBooking.seat?.room?.name || "未知房间" }}
          <span class="text-slate-400 mx-1">-</span>
          {{ seatRowColText(activeBooking) }}
          <span v-if="seatNoText(activeBooking) != null" class="text-slate-400 mx-1">-</span>
          <span v-if="seatNoText(activeBooking) != null" class="text-sm text-slate-700">
            座位号 {{ seatNoText(activeBooking) }}
          </span>
        </div>

        <div class="text-sm text-slate-700 flex items-center gap-2 flex-wrap">
          <span class="inline-flex items-center gap-1">
            <Clock3 class="w-4 h-4" />
            {{ dayjs(activeBooking.startTime).format("MM/DD HH:mm") }} - {{ dayjs(activeBooking.endTime).format("HH:mm") }}
          </span>
          <span v-if="activeCountdown" class="text-xs text-slate-500">
            · {{ activeCountdown }}
          </span>
        </div>

        <!-- <div class="text-sm text-slate-700 flex items-center gap-1">
          <MapPin class="w-4 h-4" />
          房间：{{ activeBooking.seat?.room?.name || "未知" }}
        </div> -->

        <div class="flex flex-wrap gap-2 pt-3">
          <Button size="sm" variant="outline" @click="emit('goBookings')">
            查看详情
          </Button>

          <Button
            v-if="activeBooking.state === '已预约'"
            size="sm"
            class="bg-orange-500 hover:bg-orange-600 text-white gap-1"
            :disabled="acting"
            @click="emit('checkIn')"
          >
            <LogIn class="w-4 h-4" />
            签到
          </Button>

          <Button
            v-else-if="activeBooking.state === '已签到'"
            size="sm"
            class="bg-orange-500 hover:bg-orange-600 text-white gap-1"
            :disabled="acting"
            @click="emit('checkOut')"
          >
            <LogOut class="w-4 h-4" />
            签退
          </Button>

          <Button
            size="sm"
            variant="secondary"
            class="bg-slate-100 text-slate-800 hover:bg-slate-200"
            :disabled="acting"
            @click="emit('goBookings')"
          >
            去操作页
          </Button>
        </div>
      </div>
    </div>
  </Card>
</template>
