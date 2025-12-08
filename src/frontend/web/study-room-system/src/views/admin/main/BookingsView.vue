<script setup lang="ts">
import { ref, onMounted } from "vue";
import { toast } from "vue-sonner";
import { bookingRequest } from "@/lib/api/bookingRequest";
import { Button } from "@/components/ui/button";
import { Trash } from "lucide-vue-next";

const bookings = ref<any[]>([]);
const isLoading = ref(false);

// 获取预约列表
async function loadBookings() {
  isLoading.value = true;
  try {
    const res = await bookingRequest.getMyBookings();
    bookings.value = res;
  } catch {
    toast.error("获取预约列表失败");
  } finally {
    isLoading.value = false;
  }
}

// 强制取消预约
async function handleCancel(id: string) {
  try {
    await bookingRequest.cancelBooking(id, true);
    toast.success("预约已取消");
    await loadBookings();
  } catch {
    toast.error("取消失败");
  }
}

onMounted(() => {
  loadBookings();
});
</script>

<template>
  <div>
    <h2 class="text-xl font-bold mb-4">预约管理</h2>
    <div v-if="isLoading" class="text-muted-foreground">加载中...</div>
    <table v-else class="w-full border-collapse border border-gray-300">
      <thead>
        <tr class="bg-gray-100">
          <th class="border p-2">用户</th>
          <th class="border p-2">房间</th>
          <th class="border p-2">座位</th>
          <th class="border p-2">开始时间</th>
          <th class="border p-2">结束时间</th>
          <th class="border p-2">状态</th>
          <th class="border p-2">操作</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="b in bookings" :key="b.id">
          <td class="border p-2">{{ b.user?.userName }}</td>
          <td class="border p-2">{{ b.seat?.room?.name }}</td>
          <td class="border p-2">{{ b.seat?.row }}-{{ b.seat?.col }}</td>
          <td class="border p-2">{{ b.startTime }}</td>
          <td class="border p-2">{{ b.endTime }}</td>
          <td class="border p-2">{{ b.state }}</td>
          <td class="border p-2">
            <Button class="bg-red-500 text-white px-2 py-1 rounded flex items-center gap-x-1"
              @click="handleCancel(b.id)">
              <Trash class="size-4" /> 取消
            </Button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
