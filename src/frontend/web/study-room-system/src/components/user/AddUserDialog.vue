<script setup lang="ts">
import {
    Form,
    FormControl,
    FormDescription,
    FormField,
    FormItem,
    FormLabel,
    FormMessage
} from '@/components/ui/form'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog';
import { ref } from 'vue';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { toTypedSchema } from '@vee-validate/zod'
import { useForm } from 'vee-validate'
import { adminAddUserSchema } from '@/lib/validation/userSchema';
import { authRequest } from '@/lib/api/authRequest';
import { toast } from 'vue-sonner'
import { Loader2 } from "lucide-vue-next";

// 父组件传入的属性：弹窗显隐和角色类型
const props = defineProps<{
    show: boolean
    role: 'user' | 'admin'
}>()

// 父组件监听的事件：关闭弹窗、成功回调
const emit = defineEmits<{
    (e: 'update:show', value: boolean): void
    (e: 'success'): void
}>()

// 使用封装好的 userSchema
const AddFormSchema = toTypedSchema(adminAddUserSchema)
const AddForm = useForm({ validationSchema: AddFormSchema })

const isAddingUser = ref(false); //加载动画状态

// 提交逻辑
const onAddSubmit = AddForm.handleSubmit(async (values) => {
    isAddingUser.value = true; // 开始加载
    try {
        const payload = {
        userName: values.userName,
        password: values.password,
        campusId: values.campusId,
        phone: values.phone,
        email: values.email,
        displayName: values.displayName,
    }

        let res
        if (props.role === 'user') {
            res = await authRequest.register(payload)
        } else {
            res = await authRequest.registerAdmin(payload)
        }

        // 根据返回结果直接判断
        if (res?.status === 409) {
            toast.error(`添加失败：${res.title}`)
        } else {
            toast.success('用户添加成功')
            emit('update:show', false)
            emit('success')
        }
    } finally {
        isAddingUser.value = false; // 结束加载
    }
})
</script>

<template>
  <Dialog :open="props.show" @update:open="emit('update:show', $event)">
    <DialogContent>
      <DialogHeader>
        <DialogTitle>添加{{ props.role === 'user' ? '普通用户' : '管理员' }}</DialogTitle>
      </DialogHeader>

      <!-- 表单绑定 AddForm -->
      <form :form="AddForm" class="flex flex-col gap-y-4" @submit="onAddSubmit">
        
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
          <Button class="hover:brightness-90" variant="secondary" @click="emit('update:show', false)">取消</Button>
          <Button class="bg-primary hover:brightness-90 text-white flex items-center gap-x-2" 
            :disabled="isAddingUser"
            @Click="onAddSubmit">
            <Loader2 v-if="isAddingUser" class="size-4 animate-spin mr-2" />
            {{ isAddingUser ? '添加中...' : '添加' }}
          </Button>
        </div>
      </form>
    </DialogContent>
  </Dialog>
</template>