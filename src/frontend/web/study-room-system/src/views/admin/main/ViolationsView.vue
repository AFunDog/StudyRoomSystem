<script setup lang="ts">
import { ref, reactive, onMounted, computed } from "vue";
import { toast } from "vue-sonner";
import { violationRequest } from "@/lib/api/violationRequest";
import type { Violation, ViolationCreateDto, ViolationUpdateDto } from "@/lib/types/Violation";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Select, SelectTrigger, SelectValue, SelectContent, SelectGroup, SelectItem, SelectLabel } from "@/components/ui/select";
import { Eye, Edit, Trash, Loader2, Copy } from "lucide-vue-next";

/* -----------------------------
  列表数据
------------------------------ */
const violations = ref<Violation[]>([]);
const page = ref(1);
const pageSize = ref(20);
const total = ref(0);

const keyword = ref("");

const totalPages = computed(() => Math.ceil(total.value / pageSize.value));

// 日期转换函数
function formatDate(dateStr: string) {
  if (!dateStr) return "";
  const date = new Date(dateStr);
  return date.toLocaleString("zh-CN", {
    year: "numeric",
    month: "long",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit"
  });
}

/* -----------------------------
  获取列表
------------------------------ */
async function fetchList() {
  const res = await violationRequest.getAllViolations({
    page: page.value,
    pageSize: pageSize.value
  });
  console.log(res); //测试违规记录返回

  let items = res.items ?? [];

  // 客户端搜索
  if (keyword.value.trim()) {
    const kw = keyword.value.toLowerCase();
    items = items.filter(v =>
      (v.user?.displayName ?? v.user?.userName ?? "").toLowerCase().includes(kw) ||
      (v.content ?? "").toLowerCase().includes(kw)
    );
  }

  violations.value = items;
  total.value = res.total;
}

onMounted(fetchList);

/* -----------------------------
  分页
------------------------------ */
function changePage(p: number) {
  page.value = p;
  fetchList();
}

/* -----------------------------
  查看详情
------------------------------ */
const isDetailDialogOpen = ref(false);
const currentViolation = ref<Violation | null>(null);

function viewDetail(v: Violation) {
  currentViolation.value = v;
  isDetailDialogOpen.value = true;
}

// 复制文本函数
function copyText(text: string) { 
  if (!text) return; 
  navigator.clipboard.writeText(text); 
  toast.success("复制成功"); 
}

/* -----------------------------
  编辑违约记录
------------------------------ */
const isEditDialogOpen = ref(false);

const editForm = reactive<Partial<ViolationCreateDto & ViolationUpdateDto>>({
  id: undefined,
  userId: "",
  bookingId: null,
  type: "超时",
  content: ""
});

const bookingIdString = computed({
  get: () => editForm.bookingId ?? "",
  set: (val: string) => {
    editForm.bookingId = val === "" ? null : val;
  }
});


// 修改违约记录确认弹窗
const isConfirmViolationDialogOpen = ref(false);
// 正在保存违约修改
const isSavingViolation = ref(false);
// 当前正在编辑的违约记录
const editingViolation = ref(null as Violation | null);

//打开确认弹窗方法
function openConfirmViolationDialog(violation: Violation) {
  editingViolation.value = violation;
  isConfirmViolationDialogOpen.value = true;
}

function openEdit(v: Violation) {
  Object.assign(editForm, {
    id: v.id,
    userId: v.userId,
    bookingId: v.bookingId ?? null,
    type: v.type,
    content: v.content
  });
  isEditDialogOpen.value = true;
}


async function submitEdit() {
  // 打开确认弹窗 
  isConfirmViolationDialogOpen.value = true;
}

//执行保存函数
async function confirmEditViolation() {
  isSavingViolation.value = true;

  try {
    const payload: ViolationUpdateDto = {
      id: String(editForm.id),
      type: editForm.type as any,
      content: editForm.content ?? null
    };

    await violationRequest.updateViolation(payload);

    toast.success("保存成功");

    isConfirmViolationDialogOpen.value = false;
    isEditDialogOpen.value = false;
    fetchList();

  } catch (err: any) {
    toast.error(err.message || "保存失败");
  } finally {
    isSavingViolation.value = false;
  }
}



/* -----------------------------
  删除
------------------------------ */
// 删除违约记录弹窗
const isDeleteViolationDialogOpen = ref(false);
// 正在删除的违约记录 ID
const deletingViolationId = ref<string | null>(null);
// 准备删除的违约记录 ID
const pendingDeleteViolationId = ref<string | null>(null);

// 打开删除弹窗方法
function openDeleteViolationDialog(id: string) {
  pendingDeleteViolationId.value = id;
  isDeleteViolationDialogOpen.value = true;
}

// 确认删除方法
async function confirmDeleteViolation() {
  if (!pendingDeleteViolationId.value) return;

  deletingViolationId.value = pendingDeleteViolationId.value;

  try {
    await violationRequest.deleteViolation(pendingDeleteViolationId.value);

    toast.success("删除成功");

    // 关闭弹窗
    isDeleteViolationDialogOpen.value = false;

    // 清空状态
    pendingDeleteViolationId.value = null;
    deletingViolationId.value = null;

    // 刷新列表
    fetchList();

  } catch (err: any) {
    toast.error(err.message || "删除失败");
  } finally {
    deletingViolationId.value = null;
  }
}

</script>

<template>
  <div>
    <!-- 页面标题 -->
    <h2 class="text-xl font-bold mb-4">违规记录</h2>

    <!-- 顶部工具栏 -->
    <div class="flex items-center justify-between mb-4">
      <Input
        v-model="keyword"
        placeholder="搜索用户 / 内容"
        class="w-64"
      />
    </div>

    <div class="overflow-x-auto overflow-y-auto max-h-[75vh] border border-gray-300 rounded-lg relative">
      <!-- 表格 -->
      <table class="w-full  border-separate border-spacing-0">
        <thead class="sticky top-0 z-50 bg-gray-100">
          <tr class="bg-gray-100">
            <th class="border p-2">用户名</th>
            <th class="border p-2">类型</th>
            <th class="border p-2">内容</th>
            <th class="border p-2">违规时间</th>
            <th class="border p-2">操作</th>
          </tr>
        </thead>

        <tbody>
          <tr v-for="v in violations" :key="v.id">
            <td class="border p-2">{{ v.user?.userName}}</td>
            <td class="border p-2">{{ v.type }}</td>
            <td class="border p-2">{{ v.content }}</td>
            <td class="border p-2">{{ formatDate(v.createTime) }}</td>

            <td class="border p-2 flex gap-x-2">
              <Button class="bg-green-500 hover:bg-green-500 hover:brightness-110 text-white px-2 py-1 rounded flex items-center gap-x-1"
                      @click="viewDetail(v)">
                <Eye class="size-4" /> 查看
              </Button>
              <Button class="bg-yellow-500 hover:bg-yellow-500 hover:brightness-110 text-white px-2 py-1 rounded flex items-center gap-x-1"
                      @click="openEdit(v)">
                <Edit class="size-4" /> 编辑
              </Button>
              <Button class="bg-red-600 hover:bg-red-600 hover:brightness-110 text-white px-2 py-1 rounded flex items-center gap-x-1"
                      @click="openDeleteViolationDialog(v.id)">
                <Trash class="size-4" />删除
              </Button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- 分页 -->
    <div class="flex justify-between items-center mt-4">
      <Button
        class="bg-primary text-white px-3 py-1"
        :disabled="page <= 1"
        @click="changePage(page - 1)"
      >
        上一页
      </Button>

      <span class="text-sm text-muted-foreground">
        第 {{ page }} 页 / 共 {{ totalPages }} 页
      </span>

      <Button
        class="bg-primary text-white px-3 py-1"
        :disabled="page >= totalPages"
        @click="changePage(page + 1)"
      >
        下一页
      </Button>
    </div>
  </div>

  <!-- 查看详情弹窗 -->
  <Dialog v-model:open="isDetailDialogOpen">
    <DialogContent>
      <DialogHeader>
        <DialogTitle>违规详情</DialogTitle>
      </DialogHeader>

      <div v-if="currentViolation" class="flex flex-col gap-y-3 text-sm">
      
        <div class="flex items-center gap-2">
          <span class="font-semibold">用户名：</span>
          <span>{{ currentViolation.user?.userName }}</span>
          <Copy class="size-4 cursor-pointer text-gray-500 hover:text-black"
                @click="copyText(currentViolation.user?.userName)" />
        </div>
      
        <div class="flex items-center gap-2">
          <span class="font-semibold">用户ID：</span>
          <span>{{ currentViolation.user?.id }}</span>
          <Copy class="size-4 cursor-pointer text-gray-500 hover:text-black"
                @click="copyText(currentViolation.user?.id)" />
        </div>
      
        <div>
          <span class="font-semibold">类型：</span>
          {{ currentViolation.type }}
        </div>
      
        <div>
          <span class="font-semibold">内容：</span>
          {{ currentViolation.content }}
        </div>
      
        <div>
          <span class="font-semibold">违规时间：</span>
          {{ formatDate(currentViolation.createTime) }}
        </div>
      
        <div class="flex items-center gap-2">
          <span class="font-semibold">房间：</span>
          <span>{{ currentViolation.booking?.seat?.room?.name }}</span>
        </div>
      
        <div class="flex items-center gap-2">
          <span class="font-semibold">房间ID：</span>
          <span>{{ currentViolation.booking?.seat?.room?.id }}</span>
          <Copy class="size-4 cursor-pointer text-gray-500 hover:text-black"
                @click="copyText(currentViolation.booking?.seat?.room?.id ?? '')" />
        </div>
      
        <div class="flex items-center gap-2">
          <span class="font-semibold">座位：</span>
          <span>{{ currentViolation.booking?.seat?.row }}-{{ currentViolation.booking?.seat?.col }}</span>
        </div>
      
        <div class="flex items-center gap-2">
          <span class="font-semibold">座位ID：</span>
          <span>{{ currentViolation.booking?.seatId }}</span>
          <Copy class="size-4 cursor-pointer text-gray-500 hover:text-black"
                @click="copyText(currentViolation.booking?.seatId ?? '')" />
        </div>
      
        <div class="flex items-center gap-2">
          <span class="font-semibold">关联预约：</span>
          <span v-if="currentViolation.booking">
            {{ currentViolation.booking.id }}
          </span>
          <Copy v-if="currentViolation.booking"
                class="size-4 cursor-pointer text-gray-500 hover:text-black"
                @click="copyText(currentViolation.booking.id)" />
        </div>
      
      </div>


      <DialogFooter>
        <Button class="bg-primary hover:bg-primary hover:brightness-110 text-white flex items-center gap-x-2" variant="secondary" @click="isDetailDialogOpen = false">关闭</Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>

  <!-- 编辑违规记录弹窗 -->
  <Dialog v-model:open="isEditDialogOpen">
    <DialogContent>
      <DialogHeader>
        <DialogTitle>编辑违规记录</DialogTitle>
      </DialogHeader>

      <div class="flex flex-col gap-y-3">
        <!-- 用户 ID（只读） -->
        <div>
          <label class="text-sm font-medium">用户 ID</label>
          <Input v-model="editForm.userId" disabled />
        </div>

        <!-- 违规类型 -->
        <div>
          <label class="text-sm font-medium">违规类型</label>
          <Select v-model="editForm.type">
            <SelectTrigger>
              <SelectValue placeholder="选择类型" />
            </SelectTrigger>
            <SelectContent>
              <SelectGroup>
                <SelectItem value="超时">超时</SelectItem>
                <SelectItem value="强制取消">强制取消</SelectItem>
                <SelectItem value="管理员">管理员</SelectItem>
              </SelectGroup>
            </SelectContent>
          </Select>
        </div>

        <!-- 关联预约 ID（只读） -->
        <div>
          <label class="text-sm font-medium">关联预约 ID</label>
          <Input v-model="bookingIdString" disabled />
        </div>

        <!-- 内容 -->
        <div>
          <label class="text-sm font-medium">内容</label>
          <Input v-model="editForm.content" placeholder="违规内容描述" />
        </div>
      </div>

      <DialogFooter>
        <Button class="bg-primary text-white hover:brightness-110"
                @click="submitEdit">
          保存
        </Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>

  <!-- 确认修改弹窗 -->
  <Dialog v-model:open="isConfirmViolationDialogOpen">
    <DialogContent>
      <DialogHeader>
        <DialogTitle>确认保存</DialogTitle>
      </DialogHeader>

      <p class="text-sm text-muted-foreground">确定要保存修改吗？</p>

      <DialogFooter>
        <Button class="hover:brightness-90" variant="secondary" @click="isConfirmViolationDialogOpen = false">
          取消
        </Button>
        <Button
          class="bg-primary hover:brightness-90 text-white flex items-center gap-x-2"
          :disabled="isSavingViolation"
          @click="confirmEditViolation"
        >
          <Loader2 v-if="isSavingViolation" class="size-4 animate-spin" />
          {{ isSavingViolation ? "保存中..." : "确定" }}
        </Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>

  <!-- 删除违约记录确认弹窗 -->
  <Dialog v-model:open="isDeleteViolationDialogOpen">
    <DialogContent>
      <DialogHeader>
        <DialogTitle>确认删除</DialogTitle>
      </DialogHeader>

      <p class="text-sm text-muted-foreground">
        确定要删除这条违规记录吗？此操作不可恢复。
      </p>

      <DialogFooter>
        <Button
          class="hover:brightness-90"
          variant="secondary"
          @click="isDeleteViolationDialogOpen = false"
        >
          取消
        </Button>

        <Button
          class="bg-red-600 hover:bg-red-600 hover:brightness-90 text-white flex items-center gap-x-2"
          :disabled="deletingViolationId === pendingDeleteViolationId"
          @click="confirmDeleteViolation"
        >
          <Loader2
            v-if="deletingViolationId === pendingDeleteViolationId"
            class="size-4 animate-spin"
          />
          {{ deletingViolationId === pendingDeleteViolationId ? "删除中..." : "确认删除" }}
        </Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>

</template>