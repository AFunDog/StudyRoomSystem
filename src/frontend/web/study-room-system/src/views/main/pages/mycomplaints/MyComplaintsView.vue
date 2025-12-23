<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import dayjs from "dayjs";
import { toast } from "vue-sonner";
import { Button } from "@/components/ui/button";
import { RotateCw, ArrowLeft } from "lucide-vue-next";
import type { Complaint, ComplaintState } from "@/lib/types/Complaint";
import type { Room } from "@/lib/types/Room";
import { complaintRequest } from "@/lib/api/complaintRequest";
import { roomRequest } from "@/lib/api/roomRequest";
import { seatRequest } from "@/lib/api/seatRequest";
import ComplaintList from "./components/ComplaintList.vue";
import ComplaintForm from "./components/ComplaintForm.vue";

type StateFilter = "all" | ComplaintState;
type Mode = "list" | "create" | "edit";

const mode = ref<Mode>("list");
const selectedComplaint = ref<Complaint | null>(null);

const complaints = ref<Complaint[]>([]);
const loading = ref(false);
const loadingMore = ref(false);
const page = ref(1);
const pageSize = 20;
const hasMore = ref(true);

const stateFilter = ref<StateFilter>("all");

const rooms = ref<Room[]>([]);
const loadingSeats = ref(false);

const filteredComplaints = computed(() =>
  complaints.value
    .filter((c) => stateFilter.value === "all" || c.state === stateFilter.value)
    .sort((a, b) => dayjs(b.createTime).valueOf() - dayjs(a.createTime).valueOf())
);

async function loadSeats() {
  loadingSeats.value = true;
  try {
    const res = await roomRequest.getRooms();
    const data: any = res.data;
    rooms.value = Array.isArray(data) ? data : data?.items ?? [];
  } catch (err) {
    console.error("获取座位列表失败", err);
    toast.error("获取座位列表失败，请稍后重试");
  } finally {
    loadingSeats.value = false;
  }
}

async function loadComplaints(reset = false) {
  if (reset) {
    page.value = 1;
    hasMore.value = true;
    complaints.value = [];
  }
  if (!hasMore.value && !reset) return;

  if (page.value === 1) {
    loading.value = true;
  } else {
    loadingMore.value = true;
  }

  try {
    const res = await complaintRequest.getMyComplaints({
      page: page.value,
      pageSize,
    });

    if (res.items.length < pageSize || page.value * pageSize >= res.total) {
      hasMore.value = false;
    }

    const merged = page.value === 1 ? res.items : [...complaints.value, ...res.items];
    complaints.value = merged;

    // 补全缺失的座位/房间信息
    const targets = (page.value === 1 ? complaints.value : res.items).filter(
      (c) => !c.seat?.room?.name
    );
    if (targets.length) {
      await Promise.all(targets.map((c) => enrichSeat(c)));
    }
  } catch (err) {
    console.error("获取投诉列表失败", err);
    toast.error("获取投诉列表失败，请稍后重试");
  } finally {
    loading.value = false;
    loadingMore.value = false;
  }
}

async function loadMore() {
  if (!hasMore.value) return;
  page.value += 1;
  await loadComplaints(false);
}

function seatDisplay(c: Complaint | null) {
  if (!c) return "";
  const room = c.seat?.room?.name || "未知房间";
  const cols = c.seat?.room?.cols ?? 0;
  const seatIdx = (c.seat?.row ?? 0) * cols + (c.seat?.col ?? 0) + 1;
  return `${room} - ${seatIdx}`;
}

function openList() {
  mode.value = "list";
  selectedComplaint.value = null;
}

function openCreate() {
  mode.value = "create";
  selectedComplaint.value = null;
}

function openEdit(c: Complaint) {
  selectedComplaint.value = c;
  mode.value = "edit";
}

async function handleCreate(payload: { seatId?: string; type: string; content: string; targetTime?: string | null }) {
  const seatId = (payload.seatId || "").trim();
  const type = payload.type?.trim() || "";
  const content = payload.content?.trim() || "";

  if (!seatId) {
    toast.error("请选择或输入座位");
    return;
  }
  if (!type) {
    toast.error("请填写投诉标题");
    return;
  }
  if (!content) {
    toast.error("请填写投诉内容");
    return;
  }

  try {
    await complaintRequest.createComplaint({
      seatId,
      type,
      content,
      targetTime: payload.targetTime ? new Date(payload.targetTime).toISOString() : null,
    });
    toast.success("投诉已提交");
    await loadComplaints(true);
    mode.value = "list";
  } catch (err) {
    console.error("提交投诉失败", err);
    toast.error("提交投诉失败，请稍后重试");
  }
}

async function handleEdit(payload: { type: string; content: string; targetTime?: string | null }) {
  if (!selectedComplaint.value) return;

  const type = payload.type?.trim() || "";
  const content = payload.content?.trim() || "";
  if (!type) {
    toast.error("请填写投诉标题");
    return;
  }
  if (!content) {
    toast.error("请填写投诉内容");
    return;
  }

  try {
    await complaintRequest.editComplaint({
      id: selectedComplaint.value.id,
      type,
      content,
      targetTime: payload.targetTime ? new Date(payload.targetTime).toISOString() : null,
    });
    toast.success("投诉已修改");
    await loadComplaints(true);
    openList();
  } catch (err) {
    console.error("修改投诉失败", err);
    toast.error("修改投诉失败，请稍后重试");
  }
}

// 尝试补全单条投诉的座位/房间信息
async function enrichSeat(c: Complaint) {
  // 已有房间信息则不处理
  if (c.seat?.room?.name) return;

  // 若 rooms 中包含座位数据则尝试补充（兼容后端不返回 room 的情况）
  if (rooms.value.length) {
    for (const r of rooms.value) {
      const seat = r.seats?.find((s) => s.id === c.seatId);
      if (seat) {
        c.seat = {
          ...seat,
          room: { ...r, seats: undefined },
        } as any;
        return;
      }
    }
  }

  // 兜底：调用 seat 接口获取座位和房间
  try {
    const seatRes = await seatRequest.getSeat(c.seatId);
    const seatData: any = (seatRes as any).data ?? seatRes;
    c.seat = seatData;
  } catch (err) {
    console.warn("获取座位信息失败", c.seatId, err);
  }
}

onMounted(() => {
  loadComplaints(true);
  loadSeats();
});
</script>

<template>
  <div class="flex flex-col h-full w-full px-4 py-4 gap-4 min-h-0">
    <div class="flex items-center justify-between gap-2">
      <div class="text-lg font-semibold">
        {{ mode === "create" ? "发起投诉" : mode === "edit" ? "修改投诉" : "我的投诉" }}
      </div>

      <Button
        variant="ghost"
        size="icon"
        class="bg-gray-100 hover:bg-gray-200 text-gray-600 border border-gray-200 rounded-full disabled:opacity-60"
        :disabled="loading"
        @click="loadComplaints(true)"
      >
        <RotateCw class="w-4 h-4" />
      </Button>
    </div>

    <div class="flex items-center gap-2">
      <Button
        variant="outline"
        size="sm"
        :class="mode === 'list' ? 'bg-primary/10 text-primary' : ''"
        @click="openList"
      >
        我的投诉
      </Button>
      <Button
        variant="outline"
        size="sm"
        :class="mode === 'create' ? 'bg-primary/10 text-primary' : ''"
        @click="openCreate"
      >
        发起投诉
      </Button>
      <Button
        v-if="mode === 'edit'"
        variant="ghost"
        size="sm"
        class="flex items-center gap-1"
        @click="openList"
      >
        <ArrowLeft class="w-4 h-4" />
        返回列表
      </Button>
    </div>

    <template v-if="mode === 'list'">
      <div class="flex-1 min-h-0 h-full overflow-hidden">
        <ComplaintList
          :complaints="filteredComplaints"
          :loading="loading"
          :has-more="hasMore"
          :loading-more="loadingMore"
          :state-filter="stateFilter"
          :rooms="rooms"
          @update:stateFilter="stateFilter = $event as StateFilter"
          @select="openEdit"
          @load-more="loadMore"
        />
      </div>
    </template>

    <template v-else-if="mode === 'create'">
      <ComplaintForm
        mode="create"
        :rooms="rooms"
        :loading-seats="loadingSeats"
        @submit="handleCreate"
        @cancel="openList"
      />
    </template>

    <template v-else>
      <ComplaintForm
        mode="edit"
        :rooms="rooms"
        :loading-seats="loadingSeats"
        :complaint="selectedComplaint"
        @submit="handleEdit"
        @cancel="openList"
      >
        <template #badge>
          <span v-if="selectedComplaint" class="text-xs text-muted-foreground">
            {{ selectedComplaint.state }}
          </span>
        </template>
        <template #seat>
          {{ seatDisplay(selectedComplaint) }}
        </template>
      </ComplaintForm>
    </template>
  </div>
</template>
