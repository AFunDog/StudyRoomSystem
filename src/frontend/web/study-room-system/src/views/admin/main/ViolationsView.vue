<script setup lang="ts">
// import { ref, onMounted } from "vue";
// import { toast } from "vue-sonner";
// import { violationRequest } from "@/lib/api/violationRequest";
// import { Button } from "@/components/ui/button";
// import { Edit, Trash } from "lucide-vue-next";

// const violations = ref<any[]>([]);

// async function loadViolations() {
//   try {
//     const res = await violationRequest.getViolations();
//     violations.value = res.data;
//   } catch {
//     toast.error("获取违规记录失败");
//   }
// }

// async function handleDelete(id: string) {
//   try {
//     await violationRequest.deleteViolation(id);
//     violations.value = violations.value.filter(v => v.id !== id);
//     toast.success("违规记录已删除");
//   } catch {
//     toast.error("删除失败");
//   }
// }

// onMounted(() => {
//   loadViolations();
// });
</script>

<template>
  <!-- <div>
    <h2 class="text-xl font-bold mb-4">违规记录</h2>
    <table class="w-full border-collapse border border-gray-300">
      <thead>
        <tr class="bg-gray-100">
          <th class="border p-2">用户</th>
          <th class="border p-2">原因</th>
          <th class="border p-2">时间</th>
          <th class="border p-2">操作</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="v in violations" :key="v.id">
          <td class="border p-2">{{ v.user?.userName }}</td>
          <td class="border p-2">{{ v.reason }}</td>
          <td class="border p-2">{{ v.createdAt }}</td>
          <td class="border p-2 flex gap-x-2">
            <Button class="bg-yellow-500 text-white px-2 py-1 rounded flex items-center gap-x-1">
              <Edit class="size-4" /> 修改
            </Button>
            <Button class="bg-red-500 text-white px-2 py-1 rounded flex items-center gap-x-1"
              @click="handleDelete(v.id)">
              <Trash class="size-4" /> 删除
            </Button>
          </td>
        </tr>
      </tbody>
    </table>
  </div> -->
</template>
