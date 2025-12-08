<script setup lang="ts">
// 用户管理
import { ref, onMounted } from 'vue';
import { userRequest } from '@/lib/api/userRequest';
import { authRequest } from '@/lib/api/authRequest';
import { toast } from 'vue-sonner';
import { Button } from '@/components/ui/button';
import { Edit, Trash } from "lucide-vue-next";
import type { User, UserCreateInput } from "@/lib/types/User";

// 表单相关
import {
  Form, FormField, FormItem, FormLabel, FormControl, FormMessage
} from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { toTypedSchema } from '@vee-validate/zod';
import { useForm } from 'vee-validate';
import { adminAddUserSchema } from '@/lib/validation/userSchema';

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

// 添加用户弹窗状态
// 弹窗显隐控制
const showAddDialog = ref(false);
// 角色类型（普通用户/管理员）
const addRole = ref<'user' | 'admin'>('user');

// 使用封装好的 userSchema
const formSchema = toTypedSchema(adminAddUserSchema);
const form = useForm({validationSchema: formSchema});


// 打开弹窗
function openAdd(role: 'user' | 'admin') {
  addRole.value = role;
  showAddDialog.value = true;
}

// 提交逻辑
const onAddSubmit = async (values: UserCreateInput) => {
  try {
    const payload = {
      userName: values.userName,
      password: values.password,
      campusId: values.campusId,
      phone: values.phone,
      email: values.email,
      displayName: values.displayName
    };
    // 根据角色调用不同接口
    if (addRole.value === 'user') {
      await authRequest.register(payload);
    } else {
      await authRequest.registerAdmin(payload);
    }
    toast.success('用户添加成功');
    showAddDialog.value = false;
    loadUsers();
  } catch {
    toast.error('添加失败');
  }
};

// 编辑弹窗状态
const editingUser = ref<any | null>(null);
const showEditDialog = ref(false);
// 提交修改
function handleEdit(user: any) {
  editingUser.value = { ...user };
  showEditDialog.value = true;
}
// 保存修改
async function saveEdit() {
  try {
    await userRequest.updateUser(editingUser.value);
    toast.success('用户信息已更新');
    showEditDialog.value = false;
    loadUsers();
  } catch {
    toast.error('更新失败');
  }
}

// 删除用户
async function handleDelete(id: string) {
  try {
    await userRequest.deleteUser(id);
    users.value = users.value.filter(u => u.id !== id);
    toast.success('用户已删除');
  } catch {
    toast.error('删除失败');
  }
}
</script>

<template>
  <div>
    <h2 class="text-xl font-bold mb-4">用户管理</h2>
    <div class="flex justify-end mb-4">
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
            <Button class="bg-red-600 hover:bg-red-600 hover:brightness-110 text-white px-2 py-1 rounded flex items-center gap-x-1"
                  @click="handleDelete(user.id)"><Trash class="size-4" />删除</Button>
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
        @click="page--; loadUsers()"
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
        @click="page++; loadUsers()"
      >
        下一页
      </Button>
    </div>

    <!-- 添加用户弹窗 -->
    <Dialog v-model:open="showAddDialog">
      <DialogContent>
        <DialogHeader>
          <DialogTitle>添加{{ addRole === 'user' ? '普通用户' : '管理员' }}</DialogTitle>
        </DialogHeader>
        <Form :form="form" class="flex flex-col gap-y-4" @submit="form.handleSubmit(onAddSubmit)">

          <!-- 用户名 -->
          <FormField name="userName" v-slot="{ componentField }" validate-on-blur validate-on-input>
            <FormItem>
              <FormLabel>用户名 <span class="text-red-500">*</span></FormLabel>
              <FormControl>
                <Input v-bind="componentField" autocomplete="username" placeholder="请输入用户名" />
              </FormControl>
              <FormMessage />
            </FormItem>
          </FormField>
        
          <!-- 昵称（可选） -->
          <FormField name="displayName" v-slot="{ componentField }" validate-on-blur>
            <FormItem>
              <FormLabel>昵称</FormLabel>
              <FormControl>
                <Input v-bind="componentField" autocomplete="nickname" placeholder="请输入昵称（可选）" />
              </FormControl>
              <FormMessage />
            </FormItem>
          </FormField>
        
          <!-- 学号/工号 -->
          <FormField name="campusId" v-slot="{ componentField }" validate-on-blur validate-on-input>
            <FormItem>
              <FormLabel>学号/工号 <span class="text-red-500">*</span></FormLabel>
              <FormControl>
                <Input v-bind="componentField" placeholder="请输入学号/工号" />
              </FormControl>
              <FormMessage />
            </FormItem>
          </FormField>
        
          <!-- 手机号 -->
          <FormField name="phone" v-slot="{ componentField }" validate-on-blur validate-on-input>
            <FormItem>
              <FormLabel>手机号 <span class="text-red-500">*</span></FormLabel>
              <FormControl>
                <Input v-bind="componentField" autocomplete="tel" placeholder="请输入手机号" />
              </FormControl>
              <FormMessage />
            </FormItem>
          </FormField>
        
          <!-- 邮箱（可选） -->
          <FormField name="email" v-slot="{ componentField }" validate-on-blur>
            <FormItem>
              <FormLabel>邮箱</FormLabel>
              <FormControl>
                <Input v-bind="componentField" autocomplete="email" placeholder="请输入邮箱（可选）" />
              </FormControl>
              <FormMessage />
            </FormItem>
          </FormField>
        
          <!-- 密码 -->
          <FormField name="password" v-slot="{ componentField }" validate-on-blur validate-on-input>
            <FormItem>
              <FormLabel>密码 <span class="text-red-500">*</span></FormLabel>
              <FormControl>
                <Input v-bind="componentField" type="password" autocomplete="new-password" placeholder="请输入密码" />
              </FormControl>
              <FormMessage />
            </FormItem>
          </FormField>
        
          <!-- 确认密码 -->
          <FormField name="confirmPassword" v-slot="{ componentField }" validate-on-blur validate-on-input>
            <FormItem>
              <FormLabel>确认密码 <span class="text-red-500">*</span></FormLabel>
              <FormControl>
                <Input v-bind="componentField" type="password" autocomplete="new-password" placeholder="请再次输入密码" />
              </FormControl>
              <FormMessage />
            </FormItem>
          </FormField>

          <!-- 提交按钮 -->
          <div class="flex justify-end gap-2 mt-4">
            <Button type="button" variant="secondary" @click="showAddDialog = false">取消</Button>
            <Button type="submit">提交</Button>
          </div>
        </Form>
      </DialogContent>
    </Dialog>

    
  </div>
</template>
