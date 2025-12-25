<script setup lang="ts">
import { ref, reactive, onMounted, watch } from "vue";
import { complaintRequest } from "@/lib/api/complaintRequest";
import { userRequest } from "@/lib/api/userRequest";
import { toast } from "vue-sonner";
import type { Complaint } from "@/lib/types/Complaint";
import { Copy, Loader2, Eye, CheckCircle, XCircle } from "lucide-vue-next";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";

/* -----------------------------
  数据与分页
------------------------------ */
const complaints = ref<Complaint[]>([]);
const total = ref(0);
const page = ref(1);
const pageSize = ref(20);

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

async function fetchComplaints() {
  const res = await complaintRequest.getAllComplaints({
    page: page.value,
    pageSize: pageSize.value,
  });
  complaints.value = res.items;
  // 测试投诉信息
  console.log(complaints.value);
  total.value = res.total;
}

onMounted(fetchComplaints);

/* -----------------------------
  查看详情
------------------------------ */
const isDetailDialogOpen = ref(false);
const currentComplaint = ref<Complaint | null>(null);

function openDetail(c: any) {
  currentComplaint.value = c;
  isDetailDialogOpen.value = true;
}

function copyText(text?: string) {
  if (!text) return;
  navigator.clipboard.writeText(text);
  toast.success("复制成功");
}

/* -----------------------------
  忽略投诉
------------------------------ */
const isIgnoreDialogOpen = ref(false);
const pendingIgnoreId = ref<string | null>(null);
const ignoringId = ref<string | null>(null);

function openIgnoreDialog(id: string) {
  pendingIgnoreId.value = id;
  isIgnoreDialogOpen.value = true;
}

async function confirmIgnoreComplaint() {
  if (!pendingIgnoreId.value) return;

  ignoringId.value = pendingIgnoreId.value;

  try {
    await complaintRequest.ignoreComplaint({
      id: pendingIgnoreId.value,
      content: "管理员忽略投诉",
    });

    toast.success("已忽略投诉");

    isIgnoreDialogOpen.value = false;
    pendingIgnoreId.value = null;

    fetchComplaints();
  } catch (err: any) {
    toast.error(err.message || "忽略失败");
  } finally {
    ignoringId.value = null;
  }
}

/* -----------------------------
  处理投诉（添加违规）
------------------------------ */
const isHandleDialogOpen = ref(false);
const pendingHandleComplaint = ref<Complaint | null>(null);

// 处理表单
const handleForm = reactive({
  targetUserId: "",     // 管理员填写的处罚对象 ID
  targetUserName: "",   // 处罚用户（可编辑）
  type: "",             // 投诉标题
  sendContent: "",      // 投诉内容
  targetTime: "",       // 发生时间（格式化后）
  score: 5,             // 扣分
  violationContent: "", // 违规内容
  content: "",          // 处理备注
});

// 打开处理弹窗
function openHandleDialog(c: Complaint) {
  pendingHandleComplaint.value = c;

  handleForm.targetUserId = ""; // 让管理员填写
  handleForm.targetUserName = ""; //违规用户管理员填写
  handleForm.type = c.type;
  handleForm.sendContent = c.sendContent;
  handleForm.targetTime = formatDate(c.targetTime ?? "");
  handleForm.score = 5;
  handleForm.violationContent = "";
  handleForm.content = "";

  isHandleDialogOpen.value = true;
}

/* ----------------------------- 自动根据 targetUserId 查询用户 ------------------------------ */ 
watch(
  () => handleForm.targetUserId,
  async (newId) => {
    if (!newId) {
      handleForm.targetUserName = "";
      return;
    }

    try {
      console.log("newId:", JSON.stringify(newId)); // 测试返回数据
      const res = await userRequest.getUserById(newId);
      // console.log(res);
      const user = res.data;
      // console.log(user);
      handleForm.targetUserName = user.userName;
    } catch (err) {
      handleForm.targetUserName = "用户不存在";
    }
  }
);


const isHandling = ref(false);

// 提交处理
async function confirmHandleComplaint() {
  console.log(pendingHandleComplaint.value)
  if (!pendingHandleComplaint.value) return;

  console.log("处理表单数据：", JSON.parse(JSON.stringify(handleForm)));


  isHandling.value = true;

  try {
    await complaintRequest.handleComplaint({
      id: pendingHandleComplaint.value.id,
      content: handleForm.content,
      targetUserId: handleForm.targetUserId,
      score: handleForm.score,
      violationContent: handleForm.violationContent,
    });

    toast.success("投诉已处理并添加违规");

    isHandleDialogOpen.value = false;
    pendingHandleComplaint.value = null;

    fetchComplaints();
  } catch (err: any) {
    toast.error(err.message || "处理失败");
  } finally {
    isHandling.value = false;
  }
}
</script>


<template>
  <div>
    <h2 class="text-xl font-bold">投诉处理</h2>

    <!-- 投诉列表 -->
    <div class="overflow-x-auto overflow-y-auto max-h-[75vh] border border-gray-300 rounded-lg relative">
      <table class="w-full  border-separate border-spacing-0">
        <thead class="sticky top-0 z-50 bg-gray-100">
          <tr class="bg-gray-100">
            <th class="border p-2">用户</th>
            <th class="border p-2">标题</th>
            <th class="border p-2">内容</th>
            <th class="border p-2">发生时间</th>
            <th class="border p-2">投诉时间</th>
            <th class="border p-2">状态</th>
            <th class="border p-2">操作</th>
          </tr>
        </thead>

        <tbody>
          <tr v-for="c in complaints" :key="c.id">
            <td class="border p-2">{{ c.sendUser?.userName }}</td>
            <td class="border p-2">{{ c.type }}</td>
            <td class="border p-2">{{ c.sendContent }}</td>
            <td class="border p-2">{{ formatDate(c.targetTime ?? "") }}</td>
            <td class="border p-2">{{ formatDate(c.createTime) }}</td>

            <td class="border p-2 text-center">
              <span v-if="c.state === '已发起'" class="text-red-500">待处理</span>
              <span v-if="c.state === '已处理'" class="text-green-600">已处理</span>
              <span v-if="c.state === '已关闭'" class="text-gray-500">已忽略</span>
            </td>

            <td class="border p-2 flex gap-x-2">
              <Button class="bg-green-500 hover:bg-green-500 hover:brightness-110 text-white px-2 py-1 rounded flex items-center gap-x-1"
                @click="openDetail(c)"
              >
                <Eye class="size-4" /> 查看
              </Button>
              <Button
                class="hover:brightness-110 text-white px-2 py-1 rounded flex items-center gap-x-1"
                v-if="c.state === '已发起'"
                @click="openHandleDialog(c)"
              >
                <CheckCircle class="size-4" /> 处理
              </Button>

              <Button
                class="bg-gray-500 hover:bg-gray-500 hover:brightness-110 text-white px-2 py-1 rounded flex items-center gap-x-1"
                v-if="c.state === '已发起'"
                @click="openIgnoreDialog(c.id)"
              >
                <XCircle class="size-4" /> 忽略
              </Button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- 详情弹窗 -->
    <Dialog v-model:open="isDetailDialogOpen">
      <DialogContent>
        <DialogHeader>
          <DialogTitle>投诉详情</DialogTitle>
        </DialogHeader>

        <div v-if="currentComplaint" class="flex flex-col gap-y-3 text-sm">
          <div class="flex items-center gap-2">
            <span class="font-semibold">用户：</span>
            <span>{{ currentComplaint.sendUser?.userName }}</span>
            <Copy class="size-4 cursor-pointer" @click="copyText(currentComplaint.sendUser?.userName)" />
          </div>

          <div>
            <span class="font-semibold">标题：</span>
            {{ currentComplaint.type }}
          </div>

          <div>
            <span class="font-semibold">内容：</span>
            {{ currentComplaint.sendContent }}
          </div>

          <div>
            <span class="font-semibold">发生时间：</span>
            {{ formatDate(currentComplaint.targetTime ?? "") }}
          </div>

          <div>
            <span class="font-semibold">投诉时间：</span>
            {{ formatDate(currentComplaint.createTime) }}
          </div>

          <div>
            <span class="font-semibold">房间：</span>
            {{ currentComplaint.seat?.room?.name }}
          </div>

          <div class="flex items-center gap-2">
            <span class="font-semibold">房间ID：</span>
            <span>{{ currentComplaint.seat?.room?.id }}</span>
            <Copy class="size-4 cursor-pointer" @click="copyText(currentComplaint.seat?.room?.id)" />
          </div>

          <div>
            <span class="font-semibold">被投诉者座位：</span>
            {{ currentComplaint.seat?.row }}-{{ currentComplaint.seat?.col }}
          </div>

          <div class="flex items-center gap-2">
            <span class="font-semibold">被投诉者座位ID：</span>
            <span>{{ currentComplaint.seatId }}</span>
            <Copy class="size-4 cursor-pointer" @click="copyText(currentComplaint.seatId)" />
          </div>
        </div>

        <DialogFooter>
          <Button @click="isDetailDialogOpen = false">关闭</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>

    <!-- 忽略弹窗 -->
    <Dialog v-model:open="isIgnoreDialogOpen">
      <DialogContent>
        <DialogHeader>
          <DialogTitle>确认忽略</DialogTitle>
        </DialogHeader>

        <p class="text-sm text-muted-foreground">确定要忽略这条投诉吗？</p>

        <DialogFooter>
          <Button class="hover:brightness-90" variant="secondary" @click="isIgnoreDialogOpen = false">取消</Button>

          <Button
            class="bg-red-600 hover:bg-red-600 hover:brightness-90 text-white flex items-center gap-x-2"
            :disabled="ignoringId === pendingIgnoreId"
            @click="confirmIgnoreComplaint"
          >
            <Loader2 v-if="ignoringId === pendingIgnoreId" class="size-4 animate-spin" />
            {{ ignoringId === pendingIgnoreId ? "处理中..." : "确认忽略" }}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>

    <!-- 处理投诉弹窗 -->
    <Dialog v-model:open="isHandleDialogOpen">
      <DialogContent>
        <DialogHeader>
          <DialogTitle>处理投诉</DialogTitle>
        </DialogHeader>
      
        <div class="flex flex-col gap-y-4 text-sm">
        
          <!-- 处罚用户 ID -->
          <div>
            <label class="font-medium">处罚用户 ID</label>
            <Input
              v-model="handleForm.targetUserId"
              placeholder="请输入被处罚用户的 ID"
              class="mt-1"
            />
          </div>
          
          <!-- 自动填充的用户名 -->
          <div>
            <label class="font-medium">处罚用户姓名</label>
            <Input
              :value="handleForm.targetUserName"
              disabled
              class="bg-gray-100 mt-1"
            />
          </div>
        
          <!-- 投诉标题 -->
          <div>
            <label class="font-medium">投诉标题</label>
            <Input v-model="handleForm.type" disabled class="bg-gray-100 mt-1" />
          </div>
        
          <!-- 投诉内容 -->
          <div>
            <label class="font-medium">投诉内容</label>
            <textarea
              :value="handleForm.sendContent"
              disabled
              class="bg-gray-100 mt-1 w-full p-2 rounded border resize-none"
              rows="3"
            ></textarea>
          </div>
        
          <!-- 发生时间 -->

          <div>
            <label class="font-medium">发生时间</label>
            <Input v-model="handleForm.targetTime" disabled class="bg-gray-100 mt-1" />
          </div>
        
          <!-- 扣分 -->
          <div>
            <label class="font-medium">扣除信用分</label>
            <Input type="number" v-model="handleForm.score" class="mt-1" />
          </div>
        
          <!-- 违规内容 -->
          <div>
            <label class="font-medium">违规内容</label>
            <Input
              v-model="handleForm.violationContent"
              placeholder="例如：恶意占座"
              class="mt-1"
            />
          </div>
        
          <!-- 处理备注 -->
          <div>
            <label class="font-medium">处理备注</label>
            <Input
              v-model="handleForm.content"
              placeholder="管理员处理说明"
              class="mt-1"
            />
          </div>
        </div>
      
        <DialogFooter>
          <Button class="hover:brightness-90" variant="secondary" @click="isHandleDialogOpen = false">
            取消
          </Button>
        
          <Button
            class="bg-primary hover:brightness-90 text-white flex items-center gap-x-2"
            :disabled="isHandling"
            @click="confirmHandleComplaint"
          >
            <Loader2 v-if="isHandling" class="size-4 animate-spin" />
            {{ isHandling ? "处理中..." : "确认处理" }}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>

  </div>
</template>
