<script setup lang="ts">
// import { ref, onMounted } from "vue";
// import { toast } from "vue-sonner";
// import { fileRequest } from "@/lib/api/fileRequest";
// import { Button } from "@/components/ui/button";
// import { Trash } from "lucide-vue-next";

// const files = ref<any[]>([]);
// const isLoading = ref(false);

// // 获取文件列表
// async function loadFiles() {
//   isLoading.value = true;
//   try {
//     const res = await fileRequest.getFiles();
//     files.value = res.data;
//   } catch {
//     toast.error("获取文件失败");
//   } finally {
//     isLoading.value = false;
//   }
// }

// // 删除文件
// async function handleDelete(file: string) {
//   try {
//     await fileRequest.deleteFile(file);
//     files.value = files.value.filter(f => f.name !== file);
//     toast.success("文件已删除");
//   } catch {
//     toast.error("删除失败");
//   }
// }

// // 上传文件（示例）
// async function handleUpload(event: Event) {
//   const target = event.target as HTMLInputElement;
//   if (!target.files || target.files.length === 0) return;
//   const file = target.files[0];
//   try {
//     await fileRequest.uploadFile(file);
//     toast.success("文件上传成功");
//     await loadFiles();
//   } catch {
//     toast.error("上传失败");
//   }
// }

// onMounted(() => {
//   loadFiles();
// });
</script>

<template>
  <!-- <div>
    <h2 class="text-xl font-bold mb-4">文件管理</h2> -->

    <!-- 上传文件 -->
    <!-- <div class="mb-4">
      <input type="file" @change="handleUpload" />
    </div>

    <div v-if="isLoading" class="text-muted-foreground">加载中...</div>
    <table v-else class="w-full border-collapse border border-gray-300">
      <thead>
        <tr class="bg-gray-100">
          <th class="border p-2">文件名</th>
          <th class="border p-2">操作</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="f in files" :key="f.name">
          <td class="border p-2">{{ f.name }}</td>
          <td class="border p-2">
            <Button class="bg-red-500 text-white px-2 py-1 rounded flex items-center gap-x-1"
              @click="handleDelete(f.name)">
              <Trash class="size-4" /> 删除
            </Button>
          </td>
        </tr>
      </tbody>
    </table>
  </div> -->
</template>
