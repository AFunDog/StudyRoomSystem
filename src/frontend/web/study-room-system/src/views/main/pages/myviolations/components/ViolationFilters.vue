<script setup lang="ts">
import { Button } from "@/components/ui/button";

const props = defineProps<{
  typeFilter: string;
  loading: boolean;
}>();

const emit = defineEmits<{
  (e: "update:type-filter", value: string): void;
  (e: "refresh"): void;
}>();

const typeOptions = [
  { value: "all", label: "全部类型" },
  { value: "超时", label: "超时" },
  { value: "强制取消", label: "强制取消" },
  { value: "管理员", label: "管理员" },
];
</script>

<template>
  <div class="flex flex-col gap-3 md:flex-row md:items-center md:gap-4">
    <div class="flex items-center gap-2 flex-wrap">
      <span class="text-sm text-muted-foreground">违规类型</span>
      <div class="flex flex-wrap gap-2">
        <Button
          v-for="item in typeOptions"
          :key="item.value"
          :variant="props.typeFilter === item.value ? 'default' : 'outline'"
          size="sm"
          @click="emit('update:type-filter', item.value)"
        >
          {{ item.label }}
        </Button>
      </div>
    </div>

    <div class="flex items-center gap-2 md:ml-auto">
      <Button variant="ghost" size="sm" :disabled="loading" @click="emit('refresh')">
        刷新
      </Button>
    </div>
  </div>
</template>
