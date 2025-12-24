<script setup lang="ts">
import { ref, reactive, onMounted, computed } from "vue";
import { violationRequest } from "@/lib/api/violationRequest";
import type { Violation, ViolationCreateDto, ViolationUpdateDto } from "@/lib/types/Violation";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Select, SelectTrigger, SelectValue, SelectContent, SelectGroup, SelectItem, SelectLabel } from "@/components/ui/select";
import { Eye, Edit, Trash, ClipboardList, Armchair, Loader2 } from "lucide-vue-next";

/* -----------------------------
  列表数据
------------------------------ */
const violations = ref<Violation[]>([]);
const page = ref(1);
const pageSize = ref(20);
const total = ref(0);

const keyword = ref("");

const totalPages = computed(() => Math.ceil(total.value / pageSize.value));

/* -----------------------------
  获取列表
------------------------------ */
async function fetchList() {
  const res = await violationRequest.getAllViolations({
    page: page.value,
    pageSize: pageSize.value
  });

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

/* -----------------------------
  新建 / 编辑
------------------------------ */
const isEditDialogOpen = ref(false);
const editMode = ref<"create" | "edit">("create");

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


function openCreate() {
  editMode.value = "create";
  Object.assign(editForm, {
    id: undefined,
    userId: "",
    bookingId: null,
    type: "超时",
    content: ""
  });
  isEditDialogOpen.value = true;
}

function openEdit(v: Violation) {
  editMode.value = "edit";
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
  if (editMode.value === "create") {
    const payload: ViolationCreateDto = {
      userId: String(editForm.userId),
      bookingId: editForm.bookingId ?? null,
      type: editForm.type as any,
      content: editForm.content ?? ""
    };
    await violationRequest.createViolation(payload);
  } else {
    const payload: ViolationUpdateDto = {
      id: String(editForm.id),
      type: editForm.type as any,
      content: editForm.content ?? null
    };
    await violationRequest.updateViolation(payload);
  }

  isEditDialogOpen.value = false;
  fetchList();
}

/* -----------------------------
  删除
------------------------------ */
async function confirmDelete(id: string) {
  if (!confirm("确认删除该违规记录吗？")) return;
  await violationRequest.deleteViolation(id);
  fetchList();
}
</script>

<template>
  <div>
    <!-- 页面标题 -->
    <h2 class="text-xl font-bold mb-4">违规记录管理</h2>

    <!-- 顶部工具栏 -->
    <div class="flex items-center justify-between mb-4">
      <Input
        v-model="keyword"
        placeholder="搜索用户 / 内容"
        class="w-64"
      />

      <Button class="bg-primary text-white hover:brightness-110"
              @click="openCreate">
        添加违规记录
      </Button>
    </div>

    <div class="overflow-x-auto overflow-y-auto max-h-[75vh] border border-gray-300 rounded-lg relative">
      <!-- 表格 -->
      <table class="w-full  border-separate border-spacing-0">
        <thead>
          <tr class="bg-gray-100">
            <th class="border p-2">用户</th>
            <th class="border p-2">类型</th>
            <th class="border p-2">内容</th>
            <th class="border p-2">时间</th>
            <th class="border p-2">操作</th>
          </tr>
        </thead>

        <tbody>
          <tr v-for="v in violations" :key="v.id">
            <td class="border p-2">
              {{ v.user?.displayName ?? v.user?.userName ?? v.userId }}
            </td>
            <td class="border p-2">{{ v.type }}</td>
            <td class="border p-2">{{ v.content }}</td>
            <td class="border p-2">{{ v.createTime }}</td>

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
                      @click="confirmDelete(v.id)">
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

  <!-- 查看详情弹窗（只需要一份） -->
  <Dialog v-model:open="isDetailDialogOpen">
    <DialogContent>
      <DialogHeader>
        <DialogTitle>违规详情</DialogTitle>
      </DialogHeader>

      <div v-if="currentViolation" class="flex flex-col gap-y-3 text-sm">
        <div>
          <span class="font-semibold">用户：</span>
          {{ currentViolation.user?.displayName ?? currentViolation.user?.userName ?? currentViolation.userId }}
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
          <span class="font-semibold">时间：</span>
          {{ currentViolation.createTime }}
        </div>

        <div>
          <span class="font-semibold">关联预约：</span>
          <span v-if="currentViolation.booking">
            {{ currentViolation.booking.id }}（{{ currentViolation.booking.startTime }} ~ {{ currentViolation.booking.endTime }}）
          </span>
          <span v-else>无</span>
        </div>
      </div>

      <DialogFooter>
        <Button variant="secondary" @click="isDetailDialogOpen = false">关闭</Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>

  <!-- 新建 / 编辑违规记录弹窗（只需要一份） -->
  <Dialog v-model:open="isEditDialogOpen">
    <DialogContent>
      <DialogHeader>
        <DialogTitle>
          {{ editMode === 'create' ? '添加违规记录' : '编辑违规记录' }}
        </DialogTitle>
      </DialogHeader>

      <div class="flex flex-col gap-y-3">
        <div>
          <label class="text-sm font-medium">用户 ID</label>
          <Input v-model="editForm.userId" placeholder="请输入用户 ID" />
        </div>

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

        <div>
          <label class="text-sm font-medium">关联预约 ID（可选）</label>
          <Input v-model="bookingIdString" placeholder="可为空" />
        </div>

        <div>
          <label class="text-sm font-medium">内容</label>
          <Input v-model="editForm.content" placeholder="违规内容描述" />
        </div>
      </div>

      <DialogFooter>
        <Button variant="secondary" @click="isEditDialogOpen = false">取消</Button>
        <Button class="bg-primary text-white hover:brightness-110"
                @click="submitEdit">
          {{ editMode === 'create' ? '提交' : '保存修改' }}
        </Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>
</template>