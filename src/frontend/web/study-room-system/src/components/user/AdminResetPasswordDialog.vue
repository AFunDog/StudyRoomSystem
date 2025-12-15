<script setup lang="ts">
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage
} from '@/components/ui/form'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Loader2 } from "lucide-vue-next"

import { ref, watch } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { userCenterPasswordSchema } from '@/lib/validation'
import { userRequest } from '@/lib/api/userRequest'
import { toast } from 'vue-sonner'
import type { User } from '@/lib/types/User'

// props
const props = defineProps<{
  show: boolean
  user: User | null
}>()

// emits
const emit = defineEmits<{
  (e: 'update:show', value: boolean): void
  (e: 'success'): void
}>()

// schema
const ResetPwdSchema = toTypedSchema(userCenterPasswordSchema)
const ResetPwdForm = useForm({ validationSchema: ResetPwdSchema })

// loading
const isSubmitting = ref(false)

// 当用户变化时，重置表单
watch(() => props.user, () => {
  ResetPwdForm.resetForm({
    values: {
      oldPassword: '',
      newPassword: '',
      confirmPassword: ''
    }
  })
})

// 提交逻辑
const onSubmit = ResetPwdForm.handleSubmit(async (values) => {
  if (!props.user) return

  isSubmitting.value = true
  try {
    await userRequest.updatePassword({
      id: props.user.id,
      oldPassword: values.oldPassword,
      newPassword: values.newPassword
    })

    toast.success('密码修改成功')

    emit('update:show', false)
    emit('success')
  } catch (err) {
    console.error(err)
    toast.error('修改密码失败，请检查旧密码是否正确')
  } finally {
    isSubmitting.value = false
  }
})
</script>

<template>
  <Dialog :open="props.show" @update:open="emit('update:show', $event)">
    <DialogContent>
      <DialogHeader>
        <DialogTitle>修改密码</DialogTitle>
      </DialogHeader>

      <form :form="ResetPwdForm" class="flex flex-col gap-y-4" @submit="onSubmit">

        <!-- 旧密码 -->
        <FormField name="oldPassword" v-slot="{ componentField }">
          <FormItem>
            <FormLabel>旧密码 <span class="text-red-500">*</span></FormLabel>
            <FormControl>
              <Input type="password" v-bind="componentField" autocomplete="current-password" placeholder="请输入旧密码" />
            </FormControl>
            <FormMessage />
          </FormItem>
        </FormField>

        <!-- 新密码 -->
        <FormField name="newPassword" v-slot="{ componentField }">
          <FormItem>
            <FormLabel>新密码 <span class="text-red-500">*</span></FormLabel>
            <FormControl>
              <Input type="password" v-bind="componentField" autocomplete="new-password" placeholder="请输入新密码" />
            </FormControl>
            <FormMessage />
          </FormItem>
        </FormField>

        <!-- 确认新密码 -->
        <FormField name="confirmPassword" v-slot="{ componentField }">
          <FormItem>
            <FormLabel>确认新密码 <span class="text-red-500">*</span></FormLabel>
            <FormControl>
              <Input type="password" v-bind="componentField" autocomplete="new-password" placeholder="请再次输入新密码" />
            </FormControl>
            <FormMessage />
          </FormItem>
        </FormField>

        <DialogFooter class="flex justify-end gap-x-2 mt-4">
          <Button variant="secondary" @click="emit('update:show', false)">取消</Button>
          <Button class="bg-primary text-white flex items-center gap-x-2"
                  :disabled="isSubmitting"
                  @click="onSubmit">
            <Loader2 v-if="isSubmitting" class="size-4 animate-spin" />
            {{ isSubmitting ? '提交中...' : '确认修改' }}
          </Button>
        </DialogFooter>
      </form>
    </DialogContent>
  </Dialog>
</template>
