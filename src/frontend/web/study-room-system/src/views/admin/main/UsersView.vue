<script setup lang="ts">

//用户管理

import { ref, onMounted } from 'vue';
import { userRequest } from '@/lib/api/userRequest';
import { toast } from 'vue-sonner';
// 页面加载时获取用户列表
const users = ref<any[]>([]);

onMounted(async () => {
    try {
        const res = await userRequest.getUsers();
        users.value = Array.isArray(res.data) ? res.data : [res.data];
    } catch {
        toast.error('获取用户列表失败');
    }
});
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
//锁定用户
async function handleBlock(id: string) {
    try {
        await userRequest.blockUser(id);
        toast.success('用户已锁定');
    } catch {
        toast.error('锁定失败');
    }
}
</script>

<template>
    <div>
        <h2 class="text-xl font-bold mb-4">用户管理</h2>
        <table class="w-full border-collapse border border-gray-300">
            <thead>
                <tr class="bg-gray-100">
                    <th class="border p-2">用户名</th>
                    <th class="border p-2">角色</th>
                    <th class="border p-2">学号/工号</th>
                    <th class="border p-2">手机号</th>
                    <th class="border p-2">操作</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="user in users" :key="user.id">
                    <td class="border p-2">{{ user.userName }}</td>
                    <td class="border p-2">{{ user.role }}</td>
                    <td class="border p-2">{{ user.campusId }}</td>
                    <td class="border p-2">{{ user.phone }}</td>
                    <td class="border p-2 flex gap-x-2">
                        <button class="bg-red-500 text-white px-2 py-1 rounded" @click="handleDelete(user.id)">删除</button>
                        <button class="bg-yellow-500 text-white px-2 py-1 rounded" @click="handleBlock(user.id)">锁定</button>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
</template>
