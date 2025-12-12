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
import { editUserSchema } from '@/lib/validation/userSchema';
import { userRequest } from '@/lib/api/userRequest';
import { toast } from 'vue-sonner'
import type { UserEditInput } from "@/lib/types/User";
import { watch } from 'vue'
import { Loader, Loader2 } from "lucide-vue-next";

// 父组件传入的属性：弹窗显隐和当前编辑的用户
const props = defineProps<{
    show: boolean
    user: UserEditInput | null
}>()

// 父组件监听的事件：关闭弹窗、成功回调
const emit = defineEmits<{
    (e: 'update:show', value: boolean): void
    (e: 'success'): void
}>()

// 表单校验规则
const EditFormSchema = toTypedSchema(editUserSchema)
// 创建表单实例
const EditForm = useForm({ validationSchema: EditFormSchema })

// 当 props.user 变化时，重置表单初始值
watch(() => props.user, (u) => {
    if (u) {
        EditForm.resetForm({ values: u })
    }
})

const isEditingUser = ref(false); //加载动画状态

// 提交逻辑：更新用户信息和角色
const onEditSubmit = EditForm.handleSubmit(async (values) => {
    try {
        const payload = {
            id: values.id,
            displayName: values.displayName?.trim() || props.user!.displayName,  
            campusId: values.campusId,
            phone: values.phone,
            email: values.email,
            role: values.role
        };
        await userRequest.updateUser(payload);
        await userRequest.updateUserRole({ id: values.id, role: values.role });
        toast.success('用户信息已更新')
        emit('update:show', false) // 关闭弹窗
        emit('success')            // 通知父组件刷新列表
    } catch {
        toast.error('更新失败')
    } finally {
        isEditingUser.value = false; // 结束加载
    }
})
</script>

<template>
  <Dialog :open="props.show" @update:open="emit('update:show', $event)">
    <DialogContent v-if="props.user">
      <DialogHeader>编辑用户信息</DialogHeader>

      <!-- 表单绑定 EditForm -->
      <form :form="EditForm" class="flex flex-col gap-y-4" @submit="onEditSubmit">
        
        <FormField name="role" v-slot="{ componentField }">
          <FormItem>
            <FormLabel>角色</FormLabel>
            <FormControl>
              <select v-bind="componentField">
                <option value="User">User</option>
                <option value="Admin">Admin</option>
              </select>
            </FormControl>
            <FormMessage />
          </FormItem>
        </FormField>

        <FormField name="displayName" v-slot="{ componentField }">
          <FormItem>
            <FormLabel>昵称</FormLabel>
            <FormControl>
              <Input v-bind="componentField" />
            </FormControl>
            <FormMessage />
          </FormItem>
        </FormField>

        <FormField name="campusId" v-slot="{ componentField }">
          <FormItem>
            <FormLabel>学号/工号</FormLabel>
            <FormControl>
              <Input v-bind="componentField" />
            </FormControl>
            <FormMessage />
          </FormItem>
        </FormField>

        <FormField name="phone" v-slot="{ componentField }">
          <FormItem>
            <FormLabel>手机号</FormLabel>
            <FormControl>
              <Input v-bind="componentField" />
            </FormControl>
            <FormMessage />
          </FormItem>
        </FormField>

        <FormField name="email" v-slot="{ componentField }">
          <FormItem>
            <FormLabel>邮箱</FormLabel>
            <FormControl>
              <Input v-bind="componentField" />
            </FormControl>
            <FormMessage />
          </FormItem>
        </FormField>

        <DialogFooter class="flex justify-end gap-x-2 mt-4">
          <Button class="hover:brightness-90" variant="secondary" @click="emit('update:show', false)">取消</Button>
          <Button class="bg-primary hover:brightness-90 text-white flex items-center gap-x-2" 
            :disabled="isEditingUser"
            @click="onEditSubmit">
            <Loader2 v-if="isEditingUser" class="size-4 animate-spin mr-2" />
            {{ isEditingUser ? '保存中...' : '保存' }}
          </Button>
        </DialogFooter>
      </form>
    </DialogContent>
  </Dialog>
</template>
