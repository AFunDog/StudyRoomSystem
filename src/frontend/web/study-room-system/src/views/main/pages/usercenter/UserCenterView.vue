<script setup lang="ts">
import { ref, onMounted } from "vue"
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
  CardFooter,
} from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Button } from "@/components/ui/button"
import { toast } from "vue-sonner"

import { userRequest } from "@/lib/api/userRequest"
import type { User } from "@/lib/types/User"
import { userCenterProfileSchema, userCenterPasswordSchema } from "@/lib/validation"

interface BasicForm {
  id: string
  userName: string
  displayName: string
  campusId: string
  phone: string
  email: string
}

interface BasicErrors {
  displayName: string
  campusId: string
  phone: string
  email: string
}

interface PwdForm {
  oldPassword: string
  newPassword: string
  confirmPassword: string
}

interface PwdErrors {
  oldPassword: string
  newPassword: string
  confirmPassword: string
}

// 当前用户
const user = ref<User | null>(null)

// 基本信息表单
const basicForm = ref<BasicForm>({
  id: "",
  userName: "",
  displayName: "",
  campusId: "",
  phone: "",
  email: "",
})

// 基本信息错误提示
const basicErrors = ref<BasicErrors>({
  displayName: "",
  campusId: "",
  phone: "",
  email: "",
})

const savingBasic = ref(false)

// 修改密码表单
const pwdForm = ref<PwdForm>({
  oldPassword: "",
  newPassword: "",
  confirmPassword: "",
})

// 修改密码错误提示
const pwdErrors = ref<PwdErrors>({
  oldPassword: "",
  newPassword: "",
  confirmPassword: "",
})

const savingPwd = ref(false)

function resetBasicErrors() {
  basicErrors.value = {
    displayName: "",
    campusId: "",
    phone: "",
    email: "",
  }
}

function resetPwdErrors() {
  pwdErrors.value = {
    oldPassword: "",
    newPassword: "",
    confirmPassword: "",
  }
}

// 使用 zod 校验基本信息
function validateBasic() {
  resetBasicErrors()

  const result = userCenterProfileSchema.safeParse({
    displayName: basicForm.value.displayName,
    campusId: basicForm.value.campusId,
    phone: basicForm.value.phone,
    email: basicForm.value.email,
  })

  if (result.success) {
    return true
  }

  for (const issue of result.error.issues) {
    const field = issue.path[0] as keyof BasicErrors
    if (field && basicErrors.value[field] === "") {
      basicErrors.value[field] = issue.message
    }
  }

  return false
}

// 使用 zod 校验修改密码
function validatePwd() {
  resetPwdErrors()

  const result = userCenterPasswordSchema.safeParse({
    oldPassword: pwdForm.value.oldPassword,
    newPassword: pwdForm.value.newPassword,
    confirmPassword: pwdForm.value.confirmPassword,
  })

  if (result.success) {
    return true
  }

  for (const issue of result.error.issues) {
    const field = issue.path[0] as keyof PwdErrors
    if (field && pwdErrors.value[field] === "") {
      pwdErrors.value[field] = issue.message
    }
  }

  return false
}

// 从后端获取当前登录用户
async function fetchUser() {
  try {
    const res = await userRequest.getUser()
    const data = res.data as User
    user.value = data

    basicForm.value = {
      id: data.id,
      userName: data.userName,
      displayName: data.displayName ?? "",
      campusId: data.campusId ?? "",
      phone: data.phone ?? "",
      email: data.email ?? "",
    }

    localStorage.setItem("user", JSON.stringify(data))
  } catch (err) {
    console.error("获取用户信息失败", err)
    toast.error("获取用户信息失败，请稍后重试")
  }
}

// 保存基本信息
async function saveBasicInfo() {
  if (!user.value) return

  if (!validateBasic()) {
    return
  }

  savingBasic.value = true
  try {
    const payload = {
      id: basicForm.value.id,
      displayName: basicForm.value.displayName.trim(),
      campusId: basicForm.value.campusId.trim(),
      phone: basicForm.value.phone.trim(),
      // 邮箱可选：空字符串就不传，让后端保留原值
      email: basicForm.value.email.trim() || undefined,
    }

    const res = await userRequest.updateUser(payload)
    const updated = res.data as User
    user.value = updated

    basicForm.value.displayName = updated.displayName ?? ""
    basicForm.value.campusId = updated.campusId ?? ""
    basicForm.value.phone = updated.phone ?? ""
    basicForm.value.email = updated.email ?? ""

    localStorage.setItem("user", JSON.stringify(updated))

    toast.success("基本信息已更新")
  } catch (err) {
    console.error("更新基本信息失败", err)
    toast.error("更新失败，请稍后重试")
  } finally {
    savingBasic.value = false
  }
}

// 修改密码
async function changePassword() {
  if (!user.value) return

  if (!validatePwd()) {
    return
  }

  savingPwd.value = true
  try {
    const payload = {
      id: user.value.id,
      oldPassword: pwdForm.value.oldPassword,
      newPassword: pwdForm.value.newPassword,
    }

    await userRequest.updatePassword(payload)

    toast.success("密码修改成功")

    // 清空密码表单与错误
    pwdForm.value.oldPassword = ""
    pwdForm.value.newPassword = ""
    pwdForm.value.confirmPassword = ""
    resetPwdErrors()
  } catch (err: any) {
    console.error("修改密码失败", err)
    toast.error("修改密码失败，请检查原密码或稍后重试")
  } finally {
    savingPwd.value = false
  }
}

onMounted(() => {
  fetchUser()
})
</script>

<template>
  <div class="w-full h-full px-4 py-4 flex flex-col gap-4 overflow-auto">
    <h1 class="text-xl font-semibold">用户中心</h1>

    <!-- 基本信息 -->
    <Card>
      <CardHeader>
        <CardTitle>基本信息</CardTitle>
        <CardDescription>
          修改你的昵称、学号/工号和联系方式。
        </CardDescription>
      </CardHeader>
      <CardContent class="space-y-4">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <!-- 用户名只读 -->
          <div>
            <Label for="userName">用户名</Label>
            <Input
              id="userName"
              v-model="basicForm.userName"
              readonly
              class="mt-1 bg-muted/50"
            />
          </div>

          <div>
            <Label for="displayName">显示名称</Label>
            <Input
              id="displayName"
              v-model="basicForm.displayName"
              class="mt-1"
            />
            <p v-if="basicErrors.displayName" class="mt-1 text-xs text-red-500">
              {{ basicErrors.displayName }}
            </p>
          </div>

          <div>
            <Label for="campusId">学号/工号</Label>
            <Input
              id="campusId"
              v-model="basicForm.campusId"
              class="mt-1"
            />
            <p v-if="basicErrors.campusId" class="mt-1 text-xs text-red-500">
              {{ basicErrors.campusId }}
            </p>
          </div>

          <div>
            <Label for="phone">手机号</Label>
            <Input
              id="phone"
              v-model="basicForm.phone"
              class="mt-1"
            />
            <p v-if="basicErrors.phone" class="mt-1 text-xs text-red-500">
              {{ basicErrors.phone }}
            </p>
          </div>

          <div class="md:col-span-2">
            <Label for="email">邮箱（可选）</Label>
            <Input
              id="email"
              v-model="basicForm.email"
              class="mt-1"
            />
            <p v-if="basicErrors.email" class="mt-1 text-xs text-red-500">
              {{ basicErrors.email }}
            </p>
          </div>
        </div>
      </CardContent>
      <CardFooter class="flex justify-end">
        <Button :disabled="savingBasic" @click="saveBasicInfo">
          {{ savingBasic ? "保存中..." : "保存修改" }}
        </Button>
      </CardFooter>
    </Card>

    <!-- 修改密码 -->
    <Card>
      <CardHeader>
        <CardTitle>修改密码</CardTitle>
        <CardDescription>
          密码至少 8 位，请牢记新密码。
        </CardDescription>
      </CardHeader>
      <CardContent class="space-y-4">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div class="md:col-span-2">
            <Label for="oldPassword">原密码</Label>
            <Input
              id="oldPassword"
              v-model="pwdForm.oldPassword"
              type="password"
              autocomplete="current-password"
              class="mt-1"
            />
            <p v-if="pwdErrors.oldPassword" class="mt-1 text-xs text-red-500">
              {{ pwdErrors.oldPassword }}
            </p>
          </div>

          <div>
            <Label for="newPassword">新密码</Label>
            <Input
              id="newPassword"
              v-model="pwdForm.newPassword"
              type="password"
              autocomplete="new-password"
              class="mt-1"
            />
            <p v-if="pwdErrors.newPassword" class="mt-1 text-xs text-red-500">
              {{ pwdErrors.newPassword }}
            </p>
          </div>

          <div>
            <Label for="confirmPassword">确认新密码</Label>
            <Input
              id="confirmPassword"
              v-model="pwdForm.confirmPassword"
              type="password"
              autocomplete="new-password"
              class="mt-1"
            />
            <p
              v-if="pwdErrors.confirmPassword"
              class="mt-1 text-xs text-red-500"
            >
              {{ pwdErrors.confirmPassword }}
            </p>
          </div>
        </div>
      </CardContent>
      <CardFooter class="flex justify-end">
        <Button
          variant="outline"
          class="mr-2"
          @click="
            pwdForm.oldPassword = '';
            pwdForm.newPassword = '';
            pwdForm.confirmPassword = '';
            resetPwdErrors();
          "
        >
          重置
        </Button>
        <Button :disabled="savingPwd" variant="default" @click="changePassword">
          {{ savingPwd ? "提交中..." : "修改密码" }}
        </Button>
      </CardFooter>
    </Card>
  </div>
</template>
