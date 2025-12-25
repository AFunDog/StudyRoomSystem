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

// 保存确认弹窗
const isConfirmDialogOpen = ref(false)

// 保存加载动画
const isSaving = ref(false)

// 打开确认弹窗
function openConfirmDialog() {
  console.log("打开弹窗")
  isConfirmDialogOpen.value = true
}

// 真正执行保存
async function confirmSave() {
  isSaving.value = true

  try {
    const values = EditForm.values as UserEditInput

    const payload = {
      id: values.id,
      displayName: values.displayName?.trim() || props.user!.displayName,
      campusId: values.campusId,
      phone: values.phone,
      email: values.email,
      role: values.role
    }

    await userRequest.updateUser(payload)
    await userRequest.updateUserRole({ id: values.id, role: values.role })

    toast.success("用户信息已更新")

    // 关闭两个弹窗
    isConfirmDialogOpen.value = false
    emit("update:show", false)

    // 通知父组件刷新
    emit("success")
  } catch {
    toast.error("更新失败")
  } finally {
    isSaving.value = false
  }
}
</script>

<template>
  <Dialog :open="props.show" @update:open="emit('update:show', $event)">
    <DialogContent v-if="props.user">
      <DialogHeader>
        <DialogTitle>编辑用户信息</DialogTitle>
      </DialogHeader>

      <div class="flex flex-col gap-y-4">
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

        <!-- 底部按钮：只有保存 -->
        <DialogFooter>
          <Button type="button" class="hover:brightness-110" @click="openConfirmDialog">保存</Button>
        </DialogFooter>
      </div>
    </DialogContent>
  </Dialog>

    <!-- 确认保存弹窗 -->
  <Dialog v-model:open="isConfirmDialogOpen">
    <DialogContent>
      <DialogHeader>
        <DialogTitle>确认保存</DialogTitle>
      </DialogHeader>

      <p class="text-sm text-muted-foreground">是否确认保存对该用户的修改？</p>

      <DialogFooter class="flex justify-end gap-x-2 mt-4">
        <Button class="hover:brightness-90" variant="secondary" @click="isConfirmDialogOpen = false">取消</Button>

        <Button class="bg-primary hover:brightness-90 text-white flex items-center gap-x-2"
                :disabled="isSaving"
                @click="confirmSave">
          <Loader2 v-if="isSaving" class="size-4 animate-spin" />
          {{ isSaving ? "保存中..." : "确认保存" }}
        </Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>
</template>
