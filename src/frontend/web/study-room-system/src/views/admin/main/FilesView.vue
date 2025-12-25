<script setup lang="ts">
import { ref, onMounted } from "vue";
import { fileRequest } from "@/lib/api/fileRequest";
import { toast } from "vue-sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Loader2, Eye, Trash } from "lucide-vue-next";

// 表单相关
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog';

const files = ref<any[]>([]);
const page = ref(1);
const pageSize = ref(10);
const total = ref(0);

const isUploading = ref(false);
const uploadFile = ref<File | null>(null);

// 加载文件列表
async function fetchFiles() {
  try {
    const res = await fileRequest.list(page.value, pageSize.value);
    files.value = res.items;
    total.value = res.total;
  } catch {
    toast.error("加载文件列表失败");
  }
}

onMounted(fetchFiles);

// 上传文件
async function handleUpload() {
  if (!uploadFile.value) {
    toast.error("请选择文件");
    return;
  }

  try {
    isUploading.value = true;
    await fileRequest.upload(uploadFile.value);
    toast.success("上传成功");
    uploadFile.value = null;
    await fetchFiles();
  } catch {
    toast.error("上传失败");
  } finally {
    isUploading.value = false;
  }
}

function onFileChange(e: Event) {
  const target = e.target as HTMLInputElement;
  uploadFile.value = target.files?.[0] ?? null;
}


const isDeleteDialogOpen = ref(false);
const pendingDeleteId = ref<string | null>(null);
const deletingId = ref<string | null>(null);

// 打开删除弹窗
function openDeleteDialog(id: string) {
  pendingDeleteId.value = id;
  isDeleteDialogOpen.value = true;
}

// 确认删除
async function confirmDelete() {
  if (!pendingDeleteId.value) return;

  deletingId.value = pendingDeleteId.value;

  try {
    await fileRequest.delete(pendingDeleteId.value);
    toast.success("删除成功");

    // 刷新列表
    fetchFiles();

    isDeleteDialogOpen.value = false;
    pendingDeleteId.value = null;
  } catch {
    toast.error("删除失败");
  } finally {
    deletingId.value = null;
  }
}


// 预览文件
function previewFile(id: string) {
  window.open(`/api/v1/file/${id}`, "_blank");
}
</script>

<template>
  <div class="p-4 space-y-4">
    <!-- 上传区域 -->
    <div class="flex items-center gap-x-4">
      <Input type="file" @change="onFileChange" />
      <Button :disabled="isUploading" @click="handleUpload">
        {{ isUploading ? "上传中..." : "上传文件" }}
      </Button>
    </div>

    <!-- 文件列表 -->
    <div class="overflow-x-auto border rounded-lg">
      <table class="w-full border-separate border-spacing-0">
        <thead class="bg-gray-100 sticky top-0">
          <tr>
            <th class="border p-2">文件 ID</th>
            <th class="border p-2">预览</th>
            <th class="border p-2">操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="f in files" :key="f.id">
            <td class="border p-2">{{ f.id }}</td>
            <td class="border p-2">
              <Button class="bg-green-500 hover:bg-green-500 hover:brightness-110 text-white px-2 py-1 rounded flex items-center gap-x-1"
                @click="previewFile(f.id)"
              >
                <Eye class="size-4" /> 查看
              </Button>
            </td>
            <td class="border p-2">
              <Button class="bg-red-600 hover:bg-red-600 hover:brightness-110 text-white px-2 py-1" @click="openDeleteDialog(f.id)">
                <Trash class="size-4" />删除
              </Button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- 分页 -->
    <div class="flex justify-between items-center mt-4">
      <Button :disabled="page <= 1" @click="page--; fetchFiles()">上一页</Button>
      <span>第 {{ page }} 页 / 共 {{ Math.ceil(total / pageSize) }} 页</span>
      <Button :disabled="page >= Math.ceil(total / pageSize)" @click="page++; fetchFiles()">
        下一页
      </Button>
    </div>

    <!-- 删除确认弹窗 -->
    <Dialog v-model:open="isDeleteDialogOpen">
      <DialogContent>
        <DialogHeader>
          <DialogTitle>确认删除</DialogTitle>
        </DialogHeader>
      
        <p class="text-sm text-muted-foreground">
          确定要删除这个文件吗？此操作不可恢复。
        </p>
      
        <DialogFooter>
          <Button
            class="hover:brightness-90"
            variant="secondary"
            @click="isDeleteDialogOpen = false"
          >
            取消
          </Button>
        
          <Button
            class="bg-red-600 hover:bg-red-600 hover:brightness-90 text-white flex items-center gap-x-2"
            :disabled="deletingId === pendingDeleteId"
            @click="confirmDelete"
          >
            <Loader2
              v-if="deletingId === pendingDeleteId"
              class="size-4 animate-spin"
            />
            {{ deletingId === pendingDeleteId ? "删除中..." : "确认删除" }}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>

  </div>
</template>
