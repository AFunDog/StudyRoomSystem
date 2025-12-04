<script setup lang="ts">
// import { ref, onMounted } from "vue";
// import { toast } from "vue-sonner";
// import { complaintRequest } from "@/lib/api/complaintRequest";
// import { Button } from "@/components/ui/button";
// import { Edit, Trash } from "lucide-vue-next";

// const complaints = ref<any[]>([]);

// async function loadComplaints() {
//   try {
//     const res = await complaintRequest.getComplaints();
//     complaints.value = res.data;
//   } catch {
//     toast.error("获取投诉列表失败");
//   }
// }

// async function handleDelete(id: string) {
//   try {
//     await complaintRequest.deleteComplaint(id);
//     complaints.value = complaints.value.filter(c => c.id !== id);
//     toast.success("投诉已删除");
//   } catch {
//     toast.error("删除失败");
//   }
// }

// onMounted(() => {
//   loadComplaints();
// });
</script>

<template>
  <!-- <div>
    <h2 class="text-xl font-bold mb-4">投诉处理</h2>
    <table class="w-full border-collapse border border-gray-300">
      <thead>
        <tr class="bg-gray-100">
          <th class="border p-2">投诉人</th>
          <th class="border p-2">内容</th>
          <th class="border p-2">状态</th>
          <th class="border p-2">操作</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="c in complaints" :key="c.id">
          <td class="border p-2">{{ c.user?.userName }}</td>
          <td class="border p-2">{{ c.content }}</td>
          <td class="border p-2">{{ c.state }}</td>
          <td class="border p-2 flex gap-x-2">
            <Button class="bg-yellow-500 text-white px-2 py-1 rounded flex items-center gap-x-1">
              <Edit class="size-4" /> 修改
            </Button>
            <Button class="bg-red-500 text-white px-2 py-1 rounded flex items-center gap-x-1"
              @click="handleDelete(c.id)">
              <Trash class="size-4" /> 删除
            </Button>
          </td>
        </tr>
      </tbody>
    </table>
  </div> -->
</template>
