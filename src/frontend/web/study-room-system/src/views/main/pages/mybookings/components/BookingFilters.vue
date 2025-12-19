<script setup lang="ts">
import { computed } from "vue";
import { Button } from "@/components/ui/button";
import {
  Select,
  SelectTrigger,
  SelectContent,
  SelectItem,
  SelectValue,
} from "@/components/ui/select";

const props = defineProps<{
  status: string;
  date: string;
  room: string;          
  roomOptions: string[]; 
}>();

const emit = defineEmits<{
  (e: "update:status", value: string): void;
  (e: "update:date", value: string): void;
  (e: "update:room", value: string): void;
}>();

const statusOptions = [
  { value: "all", label: "全部" },
  { value: "Booked", label: "已预约" },
  { value: "CheckIn", label: "已签到" },
  { value: "Checkout", label: "已签退" },
  { value: "Canceled", label: "已取消" },
];

const dateOptions = [
  { value: "all", label: "全部时间" },
  { value: "today", label: "今天" },
  { value: "future", label: "未来" },
];

const roomModel = computed({
  get: () => props.room,
  set: (val: string) => emit("update:room", val),
});
</script>

<template>
  <div class="flex flex-col gap-3 md:flex-row md:items-center md:gap-4">
    <!-- 状态筛选 -->
    <div class="flex items-center gap-2 flex-wrap">
      <span class="text-sm text-muted-foreground">状态</span>
      <div class="flex flex-wrap gap-2">
        <Button
          v-for="item in statusOptions"
          :key="item.value"
          :variant="props.status === item.value ? 'default' : 'outline'"
          size="sm"
          @click="emit('update:status', item.value)"
        >
          {{ item.label }}
        </Button>
      </div>
    </div>

    <!-- 时间筛选 -->
    <div class="flex items-center gap-2 flex-wrap">
      <span class="text-sm text-muted-foreground">时间</span>
      <div class="flex flex-wrap gap-2">
        <Button
          v-for="item in dateOptions"
          :key="item.value"
          :variant="props.date === item.value ? 'default' : 'outline'"
          size="sm"
          @click="emit('update:date', item.value)"
        >
          {{ item.label }}
        </Button>
      </div>
    </div>

    <!-- 房间筛选 -->
    <div class="flex items-center gap-2 flex-wrap">
      <span class="text-sm text-muted-foreground">房间</span>

      <Select v-model="roomModel">
        <SelectTrigger class="h-9 w-[120px]">
          <SelectValue placeholder="全部" />
        </SelectTrigger>
        <SelectContent>
          <!-- 特殊的“全部” -->
          <SelectItem value="all">全部</SelectItem>

          <!-- 具体房间 -->
          <SelectItem
            v-for="name in roomOptions"
            :key="name"
            :value="name"
          >
            {{ name }}
          </SelectItem>
        </SelectContent>
      </Select>
    </div>
  </div>
</template>
