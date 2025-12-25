<script setup lang="ts">
// 用户管理
import { ref, onMounted } from 'vue';
import { userRequest } from '@/lib/api/userRequest';
import { toast } from 'vue-sonner';
import { Button } from '@/components/ui/button';
import { Edit, Trash, Loader2, Eye, Copy } from "lucide-vue-next";
import type { User, UserEditInput } from "@/lib/types/User";

// 表单相关
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog';


// 引入表单组件
import AddUserDialog from '@/components/user/AddUserDialog.vue'
import EditUserDialog from '@/components/user/EditUserDialog.vue'
import AdminResetPasswordDialog from '@/components/user/AdminResetPasswordDialog.vue'

// 复制函数
function copyText(text?: string) {
  if (!text) return;
  navigator.clipboard.writeText(text);
  toast.success("复制成功");
}

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

const users = ref<User[]>([]);
const page = ref(1);
const pageSize = ref(20);
const total = ref(0);

// 页面加载时获取用户列表
async function loadUsers() {
  try {
    const res = await userRequest.getAllUsers(page.value, pageSize.value); 
    // 后端返回 { page, pageSize, total, items }
    users.value = res.data.items ?? [];
    page.value = res.data.page ?? 1;
    pageSize.value = res.data.pageSize ?? 10;
    total.value = res.data.total ?? users.value.length;
  } catch {
    toast.error('获取用户列表失败');
  }
}

onMounted(loadUsers);

// 分页刷新函数：避免直接 page++ 覆盖
async function changePage(newPage: number) {
  page.value = newPage;
  await loadUsers();
}

// 添加用户弹窗状态
const showAddDialog = ref(false);
const addRole = ref<'user' | 'admin'>('user');
// 打开添加用户弹窗
function openAdd(role: 'user' | 'admin') {
  addRole.value = role
  showAddDialog.value = true
}

// 查看详情
const showDetailDialog = ref(false);
const detailUser = ref<User | null>(null);

function openDetail(user: User) {
  detailUser.value = user;
  showDetailDialog.value = true;
}


// 编辑弹窗状态
const showEditDialog = ref(false);
const editingUser = ref<UserEditInput | null>(null);
// 打开编辑用户弹窗
function handleEdit(user: User) {
  editingUser.value = {
    id: user.id,
    displayName: user.displayName,
    campusId: user.campusId,
    phone: user.phone,
    email: user.email ?? '',
    role: user.role as 'User' | 'Admin',
  }
  showEditDialog.value = true
}

// 删除相关状态
const isDeleteDialogOpen = ref(false);          // 删除确认弹窗显隐
const pendingDeleteUserId = ref<string | null>(null); // 待删除的用户ID
const deletingUserId = ref<string | null>(null);      // 当前正在删除的用户ID
// 打开删除确认弹窗
function openDeleteDialog(id: string) {
  pendingDeleteUserId.value = id;
  isDeleteDialogOpen.value = true;
}
// 确认删除
async function confirmDeleteUser() {
  if (!pendingDeleteUserId.value) return;
  deletingUserId.value = pendingDeleteUserId.value;
  try {
    const res = await userRequest.deleteUser(pendingDeleteUserId.value);
    if (res.status === 200 || res.status === 204) {
      users.value = users.value.filter(u => u.id !== pendingDeleteUserId.value);
      toast.success('用户已删除');
      await loadUsers(); // 删除后刷新列表，保证 total 等数据正确
    } 
  } catch (err) {
    console.error('删除失败:', err);
    toast.error('删除失败');
  } finally {
    deletingUserId.value = null;
    pendingDeleteUserId.value = null;
    isDeleteDialogOpen.value = false;
  }
}

// 修改密码相关状态
const showChangePasswordDialog = ref(false)
const changingUser = ref<User | null>(null)
// 打开修改密码弹窗
function openChangePassword(user: User) {
  changingUser.value = user
  showChangePasswordDialog.value = true
}

</script>

<template>
  <div>
    <h2 class="text-xl font-bold mb-4">用户管理</h2>
    <div class="flex justify-end gap-x-6 mb-4">
      <Button class="hover:brightness-110" @click="openAdd('user')">添加普通用户</Button>
      <Button class="hover:brightness-110" @click="openAdd('admin')">添加管理员</Button>
    </div>

    <div class="overflow-x-auto overflow-y-auto max-h-[75vh] border border-gray-300 rounded-lg relative">
      <!-- 用户表格 -->
      <table class="w-full  border-separate border-spacing-0">
        <thead class="sticky top-0 z-50 bg-gray-100">
          <tr class="bg-gray-100">
            <th class="border p-2">用户名</th>
            <th class="border p-2">昵称</th>
            <th class="border p-2">角色</th>
            <th class="border p-2">学号/工号</th>
            <th class="border p-2">手机号</th>
            <th class="border p-2">邮箱</th>
            <th class="border p-2 sticky right-0 bg-gray-100 z-60 shadow-sm">操作</th>
          </tr>
        </thead>
        <tbody>
          <!-- 空状态提示 -->
          <tr v-if="users.length === 0">
            <td colspan="7" class="text-center text-gray-500 p-4">暂无用户</td>
          </tr>
          <!-- 用户列表 -->
          <tr v-for="user in users" :key="user.id">
            <td class="border p-2">{{ user.userName }}</td>
            <td class="border p-2">{{ user.displayName }}</td>
            <td class="border p-2">{{ user.role }}</td>
            <td class="border p-2">{{ user.campusId }}</td>
            <td class="border p-2">{{ user.phone }}</td>
            <td class="border p-2">{{ user.email }}</td>
            <td class="border-t border-b border-l sticky right-0 bg-white z-10 p-0">
              <div class="h-full flex items-center gap-x-2 px-4 py-3">
                <Button class="bg-green-500 hover:bg-green-500 hover:brightness-110 text-white px-2 py-1 rounded flex items-center gap-x-1"
                        @click="openDetail(user)">
                  <Eye class="size-4" /> 查看
                </Button>
                <Button 
                  class="bg-yellow-500 hover:bg-yellow-500 hover:brightness-110 text-white px-2 py-1 rounded flex items-center gap-x-1"
                  @click="handleEdit(user)">
                  <Edit class="size-4" />编辑
                </Button>
                <Button
                  class="bg-red-600 hover:bg-red-600 hover:brightness-110 text-white px-2 py-1 rounded flex items-center gap-x-1"
                  @click="openDeleteDialog(user.id)">
                  <Trash class="size-4" />删除
                </Button>
                <Button
                  class=" hover:brightness-110 text-white px-2 py-1 rounded flex items-center gap-x-1"
                  @click="openChangePassword(user)">
                  修改密码
                </Button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- 分页控件 -->
    <div class="flex justify-between items-center mt-4">
      <Button
        variant="default"
        size="sm"
        class="bg-primary text-white hover:bg-primary/80"
        :disabled="page <= 1"
        @click="changePage(page - 1)"
      >
        上一页
      </Button>
      <span class="text-sm text-muted-foreground">
        第 {{ page }} 页 / 共 {{ Math.ceil(total / pageSize) }} 页
      </span>
      <Button
        variant="default"
        size="sm"
        class="bg-primary text-white hover:bg-primary/80"
        :disabled="page >= Math.ceil(total / pageSize)"
        @click="changePage(page + 1)"
      >
        下一页
      </Button>
    </div>

    <!-- 添加用户弹窗组件 -->
    <AddUserDialog
      v-model:show="showAddDialog"
      :role="addRole"
      @success="loadUsers"
    />

    <!-- 查看详情弹窗 -->
    <Dialog v-model:open="showDetailDialog">
      <DialogContent>
        <DialogHeader>
          <DialogTitle>用户详情</DialogTitle>
        </DialogHeader>
        <div v-if="detailUser" class="flex flex-col gap-y-3 text-sm">
          <!-- 头像 -->
          <div class="flex items-center gap-2">
            <span class="font-semibold">头像：</span>
            <img
              :src="detailUser.avatar"
              alt="用户头像"
              class="w-16 h-16 rounded-full border object-cover"
            />
          </div>
          <!-- 用户 ID -->
          <div class="flex items-center gap-2">
            <span class="font-semibold">用户 ID：</span>
            <span>{{ detailUser.id }}</span>
            <Copy class="size-4 cursor-pointer" @click="copyText(detailUser.id)" />
          </div>
          <!-- 用户名 -->
          <div class="flex items-center gap-2">
            <span class="font-semibold">用户名：</span>
            <span>{{ detailUser.userName }}</span>
            <Copy class="size-4 cursor-pointer" @click="copyText(detailUser.userName)" />
          </div>
          <!-- 昵称 -->
          <div class="flex items-center gap-2">
            <span class="font-semibold">昵称：</span>
            <span>{{ detailUser.displayName }}</span>
            <Copy class="size-4 cursor-pointer" @click="copyText(detailUser.displayName)" />
          </div>
          <!-- 角色（不允许复制） -->
          <div class="flex items-center gap-2">
            <span class="font-semibold">角色：</span>
            <span>{{ detailUser.role }}</span>
          </div>
          <!-- 学号/工号 -->
          <div class="flex items-center gap-2">
            <span class="font-semibold">学号/工号：</span>
            <span>{{ detailUser.campusId }}</span>
            <Copy class="size-4 cursor-pointer" @click="copyText(detailUser.campusId)" />
          </div>
          <!-- 手机号 -->
          <div class="flex items-center gap-2">
            <span class="font-semibold">手机号：</span>
            <span>{{ detailUser.phone }}</span>
            <Copy class="size-4 cursor-pointer" @click="copyText(detailUser.phone)" />
          </div>
          <!-- 邮箱 -->
          <div class="flex items-center gap-2">
            <span class="font-semibold">邮箱：</span>
            <span>{{ detailUser.email }}</span>
            <Copy class="size-4 cursor-pointer" @click="copyText(detailUser.email)" />
          </div>
                    <!-- 创建时间 -->
          <div class="flex items-center gap-2">
            <span class="font-semibold">创建时间：</span>
            <span>{{ formatDate(detailUser.createTime) }}</span>
          </div>
        </div>
        <DialogFooter>
          <Button variant="secondary" @click="showDetailDialog = false">
            关闭
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>

    <!-- 编辑用户弹窗组件 -->
    <EditUserDialog
      v-model:show="showEditDialog"
      :user="editingUser"
      @success="loadUsers"
    />

    <AdminResetPasswordDialog
      v-model:show="showChangePasswordDialog"
      :user="changingUser"
      @success="loadUsers"
    />

    <!-- 删除确认弹窗 -->
    <Dialog v-model:open="isDeleteDialogOpen">
      <DialogContent>
        <DialogHeader>
          <DialogTitle>确认删除</DialogTitle>
        </DialogHeader>
        <p class="text-sm text-muted-foreground">确定要删除这个用户吗？此操作不可恢复。</p>
        <DialogFooter>
          <Button class="hover:brightness-90" variant="secondary" @click="isDeleteDialogOpen = false">取消</Button>
          <Button
            class="bg-red-600 hover:bg-red-600 hover:brightness-90 text-white flex items-center gap-x-2"
            :disabled="deletingUserId === pendingDeleteUserId"
            @click="confirmDeleteUser"
          >
            <Loader2 v-if="deletingUserId === pendingDeleteUserId" class="size-4 animate-spin" />
            {{ deletingUserId === pendingDeleteUserId ? '删除中...' : '确认删除' }}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
    
  </div>
</template>
