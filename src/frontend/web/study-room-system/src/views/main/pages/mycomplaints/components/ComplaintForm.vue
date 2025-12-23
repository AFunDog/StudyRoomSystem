<script setup lang="ts">
import { computed, watch, reactive } from "vue";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import type { Complaint } from "@/lib/types/Complaint";

type Mode = "create" | "edit";

const props = defineProps<{
  mode: Mode;
  seatOptions: { id: string; label: string }[];
  loadingSeats: boolean;
  complaint?: Complaint | null;
}>();

const emit = defineEmits<{
  (e: "submit", payload: { seatId?: string; type: string; content: string; targetTime?: string | null }): void;
  (e: "cancel"): void;
}>();

const form = reactive({
  seatId: "",
  seatManual: "",
  type: "",
  content: "",
  targetTime: "",
});

const titleText = computed(() => (props.mode === "create" ? "发起投诉" : "修改投诉"));
const submitText = computed(() => (props.mode === "create" ? "提交投诉" : "保存修改"));

watch(
  () => props.complaint,
  (c) => {
    if (props.mode === "edit" && c) {
      form.seatId = c.seatId;
      form.seatManual = "";
      form.type = c.type || "";
      form.content = c.sendContent || "";
      form.targetTime = toDatetimeLocal(c.targetTime);
    } else if (props.mode === "create") {
      reset();
    }
  },
  { immediate: true }
);

function toDatetimeLocal(str?: string | null) {
  if (!str) return "";
  const d = new Date(str);
  if (isNaN(d.getTime())) return "";
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(
    d.getMinutes()
  )}`;
}

function reset() {
  form.seatId = "";
  form.seatManual = "";
  form.type = "";
  form.content = "";
  form.targetTime = "";
}

function handleSubmit() {
  const seatId = props.mode === "create" ? form.seatId || form.seatManual.trim() : undefined;
  emit("submit", {
    seatId,
    type: form.type,
    content: form.content,
    targetTime: form.targetTime ? form.targetTime : undefined,
  });
}
</script>

<template>
  <div class="border rounded-lg p-4 bg-background space-y-4">
    <div class="flex items-center justify-between">
      <div class="text-base font-semibold">{{ titleText }}</div>
      <slot name="badge"></slot>
    </div>

    <div v-if="mode === 'create'" class="space-y-2">
      <div class="text-sm text-muted-foreground">座位（必填）</div>
      <Select v-model="form.seatId">
        <SelectTrigger class="h-10">
          <SelectValue placeholder="选择开放座位" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem
            v-for="s in seatOptions"
            :key="s.id"
            :value="s.id"
          >
            {{ s.label }}
          </SelectItem>
        </SelectContent>
      </Select>
      <div class="text-xs text-muted-foreground">
        若列表找不到，可手动输入座位 ID
      </div>
      <Input
        v-model="form.seatManual"
        placeholder="手动输入座位ID"
        class="mt-1"
      />
    </div>

    <div v-else class="space-y-1 text-sm">
      <div class="text-muted-foreground">座位</div>
      <div class="font-medium">
        <slot name="seat" />
      </div>
    </div>

    <div class="space-y-2">
      <div class="text-sm text-muted-foreground">投诉标题（必填）</div>
      <Input
        v-model="form.type"
        placeholder="如：座位被占用 / 环境嘈杂"
      />
    </div>

    <div class="space-y-2">
      <div class="text-sm text-muted-foreground">目标时间（可选）</div>
      <Input
        v-model="form.targetTime"
        type="datetime-local"
      />
    </div>

    <div class="space-y-2">
      <div class="text-sm text-muted-foreground">投诉内容（必填）</div>
      <textarea
        v-model="form.content"
        rows="4"
        placeholder="请描述问题..."
        class="w-full rounded-md border border-input bg-background px-3 py-2 text-sm shadow-xs transition-[color,box-shadow] focus-visible:border-ring focus-visible:ring-ring/50 focus-visible:ring-[3px] outline-none disabled:opacity-50 disabled:cursor-not-allowed"
      />
    </div>

    <div class="flex gap-2">
      <Button v-if="mode === 'edit'" variant="outline" class="flex-1" @click="emit('cancel')">
        取消
      </Button>
      <Button class="w-full" @click="handleSubmit">
        {{ submitText }}
      </Button>
    </div>
  </div>
</template>
