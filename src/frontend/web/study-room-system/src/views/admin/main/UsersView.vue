<script setup lang="ts">
// 用户管理
import { ref, onMounted } from 'vue';
import { userRequest } from '@/lib/api/userRequest';
import { toast } from 'vue-sonner';
import { Button } from '@/components/ui/button';
import { Edit, Trash, Loader2 } from "lucide-vue-next";
import type { User, UserEditInput } from "@/lib/types/User";

// 表单相关
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog';


// 引入表单组件
import AddUserDialog from '@/components/user/AddUserDialog.vue'
import EditUserDialog from '@/components/user/EditUserDialog.vue'

const users = ref<User[]>([]);
const page = ref(1);
const pageSize = ref(10);
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

// 编辑弹窗状态
const showEditDialog = ref(false);
const editingUser = ref<UserEditInput | null>(null);

// 打开添加用户弹窗
function openAdd(role: 'user' | 'admin') {
  addRole.value = role
  showAddDialog.value = true
}

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
</script>

<template>
  <div>
    <h2 class="text-xl font-bold mb-4">用户管理</h2>
    <div class="flex justify-end gap-x-6 mb-4">
      <Button class="hover:brightness-110" @click="openAdd('user')">添加普通用户</Button>
      <Button class="hover:brightness-110" @click="openAdd('admin')">添加管理员</Button>
    </div>

    <!-- 用户表格 -->
    <table class="w-full border-collapse border border-gray-300 rounded-lg overflow-hidden shadow-sm">
      <thead>
        <tr class="bg-gray-100">
          <th class="border p-2">用户名</th>
          <th class="border p-2">昵称</th>
          <th class="border p-2">角色</th>
          <th class="border p-2">学号/工号</th>
          <th class="border p-2">手机号</th>
          <th class="border p-2">邮箱</th>
          <th class="border p-2">操作</th>
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
          <td class="border p-2 flex gap-x-2">
            <Button class="bg-yellow-500 hover:bg-yellow-500 hover:brightness-110 text-white px-2 py-1 rounded flex items-center gap-x-1"
                  @click="handleEdit(user)"><Edit class="size-4" />编辑</Button>
            <Button
              class="bg-red-600 hover:bg-red-600 hover:brightness-110 text-white px-2 py-1 rounded flex items-center gap-x-1"
              @click="openDeleteDialog(user.id)"
            >
              <Trash class="size-4" />删除
            </Button>
          </td>
        </tr>
      </tbody>
    </table>

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

    <!-- 编辑用户弹窗组件 -->
    <EditUserDialog
      v-model:show="showEditDialog"
      :user="editingUser"
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
