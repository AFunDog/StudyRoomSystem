<script setup lang="ts">
import { computed } from "vue";
import dayjs from "dayjs";
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import type { Booking } from "@/lib/types/Booking";

const props = defineProps<{
  open: boolean;
  booking: Booking | null;
}>();

// 透出交互事件
const emit = defineEmits<{
  (e: "update:open", value: boolean): void;
  (e: "check-in", booking: Booking): void;
  (e: "check-out", booking: Booking): void;
  (e: "cancel", booking: Booking): void;
}>();

const canCheckIn = computed(() => props.booking?.state === "Booked");
const canCheckOut = computed(() => props.booking?.state === "CheckIn");
const canCancel = computed(() => props.booking?.state === "Booked");

const innerOpen = computed({
  get: () => props.open,
  set: (v) => emit("update:open", v),
});

function formatRange() {
  if (!props.booking) return "";
  return `${dayjs(props.booking.startTime).format("YYYY/MM/DD HH:mm")} - ${dayjs(
    props.booking.endTime
  ).format("HH:mm")}`;
}
</script>

<template>
  <Dialog v-model:open="innerOpen">
    <DialogContent>
      <DialogHeader>
        <DialogTitle>预约详情</DialogTitle>
        <div class="flex flex-col gap-y-2">
          <div class="flex flex-row gap-x-2 items-center">
            <div class="text-lg">
              房间：{{ booking?.seat?.room?.name || "未知房间" }}
            </div>
            <div class="text-lg">-</div>
            <div class="text-lg">
              {{
                (booking?.seat?.row ?? 0) * (booking?.seat?.room?.cols ?? 0) +
                (booking?.seat?.col ?? 0) + 1
              }}
            </div>
          </div>
          <div class="text-left text-muted-foreground">
            预约时间：{{ formatRange() }}
          </div>
        </div>
      </DialogHeader>
      <DialogFooter>
        <div class="flex flex-wrap items-center justify-center gap-2">
          <Button variant="outline" @click="emit('update:open', false)">
            关闭
          </Button>

          <Button variant="default" :disabled="!canCheckIn" @click="booking && emit('check-in', booking)">
            签到
          </Button>
          <Button variant="secondary" :disabled="!canCheckOut" @click="booking && emit('check-out', booking)">
            签退
          </Button>
          <Button variant="destructive" :disabled="!canCancel" @click="booking && emit('cancel', booking)">
            取消预约
          </Button>
        </div>
      </DialogFooter>
    </DialogContent>
  </Dialog>
</template>
