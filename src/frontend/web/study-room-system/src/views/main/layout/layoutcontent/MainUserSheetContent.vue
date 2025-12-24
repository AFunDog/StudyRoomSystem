<script setup lang="ts">
import {
  SheetContent,
  SheetDescription,
  SheetFooter,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet";
import { Button } from "@/components/ui/button";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { UserRound, RotateCw } from "lucide-vue-next";
import { toast } from "vue-sonner";

import type { User } from "@/lib/types/User";
import { userRequest } from "@/lib/api/userRequest";
import { logout } from "@/lib/utils";
import { useRouter } from "vue-router";
import { onMounted, ref } from "vue";

const router = useRouter();

const user = ref<User | null>(null);
const uploadingAvatar = ref(false);
const avatarPreview = ref<string | null>(null);
const loadingUser = ref(false);
const fileInput = ref<HTMLInputElement | null>(null);
const avatarCacheKey = "userAvatar";

const rawUser = localStorage.getItem("user");
const cachedAvatar = localStorage.getItem(avatarCacheKey);
if (rawUser) {
  try {
    const parsed = JSON.parse(rawUser) as User;
    if (cachedAvatar && !parsed.avatar) {
      parsed.avatar = cachedAvatar;
    }
    user.value = parsed;
    avatarPreview.value = parsed.avatar ?? null;
  } catch (e) {
    console.error("解析本地 user 失败", e);
  }
}

async function fetchUser() {
  loadingUser.value = true;
  try {
    const res = await userRequest.getUser();
    const data = res.data as User;
    if (!data.avatar && cachedAvatar) {
      data.avatar = cachedAvatar;
    }
    user.value = data;
    avatarPreview.value = data.avatar ?? null;
    localStorage.setItem("user", JSON.stringify(data));
    if (data.avatar) {
      localStorage.setItem(avatarCacheKey, data.avatar);
    }
  } catch (err) {
    console.error("获取用户信息失败", err);
    toast.error("获取用户信息失败，请稍后重试");
  } finally {
    loadingUser.value = false;
  }
}

onMounted(() => {
  fetchUser();
});

async function handleLogout() {
  try {
    await logout();
  } finally {
    localStorage.removeItem("user");
    router.push("/login");
  }
}

function triggerUpload() {
  fileInput.value?.click();
}

function handleFileChange(event: Event) {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0];
  if (!file) return;

  const maxSize = 2 * 1024 * 1024;
  if (!file.type.startsWith("image/")) {
    toast.error("请选择图片文件");
    input.value = "";
    return;
  }
  if (file.size > maxSize) {
    toast.error("图片大小需小于 2MB");
    input.value = "";
    return;
  }

  const reader = new FileReader();
  reader.onload = async () => {
    const result = reader.result as string;
    avatarPreview.value = result;
    await uploadAvatar(result);
    input.value = "";
  };
  reader.onerror = () => {
    toast.error("读取图片失败");
    input.value = "";
  };
  reader.readAsDataURL(file);
}

async function uploadAvatar(dataUrl: string) {
  if (!user.value) return;
  uploadingAvatar.value = true;
  try {
    const payload = {
      id: user.value.id,
      displayName: user.value.displayName,
      campusId: user.value.campusId,
      phone: user.value.phone,
      email: user.value.email,
      avatar: dataUrl,
    };
    const res = await userRequest.updateUser(payload);
    const updated = res.data as User;
    updated.avatar = updated.avatar || dataUrl;
    user.value = updated;
    avatarPreview.value = updated.avatar ?? dataUrl;
    localStorage.setItem("user", JSON.stringify(updated));
    localStorage.setItem(avatarCacheKey, updated.avatar ?? dataUrl);
    toast.success("头像已更新");
  } catch (err) {
    console.error("上传头像失败", err);
    toast.error("上传头像失败，请稍后重试");
    avatarPreview.value = user.value?.avatar ?? null;
  } finally {
    uploadingAvatar.value = false;
  }
}
</script>

<template>
  <SheetContent class="flex flex-col h-full">
    <SheetHeader>
      <SheetTitle class="flex flex-row items-center gap-x-4">
        <div class="relative group">
          <Avatar class="aspect-square w-20 h-20">
            <AvatarImage :src="avatarPreview || user?.avatar || ''" />
            <AvatarFallback>
              <UserRound class="h-full w-full" />
            </AvatarFallback>
          </Avatar>
          <button
            type="button"
            class="absolute inset-0 rounded-full bg-black/45 text-white text-xs opacity-0 group-hover:opacity-100 transition flex items-center justify-center"
            :disabled="uploadingAvatar"
            @click="triggerUpload"
          >
            {{ uploadingAvatar ? "上传中..." : "更换头像" }}
          </button>
          <input
            ref="fileInput"
            type="file"
            accept="image/*"
            class="hidden"
            @change="handleFileChange"
          />
        </div>

        <div class="flex flex-col items-start gap-y-1">
          <div class="text-2xl font-semibold">
            {{ user?.displayName || user?.userName || "未命名用户" }}
          </div>
        </div>
      </SheetTitle>

      <SheetDescription class="mt-4">
        <div
          class="grid grid-cols-[auto,1fr] gap-x-3 gap-y-1 text-sm text-muted-foreground"
        >
          <span class="font-medium text-foreground">用户名</span>
          <span class="truncate">
            {{ user?.userName || "未知" }}
          </span>

          <span class="font-medium text-foreground">学号/工号</span>
          <span class="truncate">
            {{ user?.campusId || "未填写" }}
          </span>

          <span class="font-medium text-foreground">手机号</span>
          <span class="truncate">
            {{ user?.phone || "未填写" }}
          </span>

          <span class="font-medium text-foreground">角色</span>
          <span class="truncate">
            {{ user?.role || "未设置" }}
          </span>
        </div>
      </SheetDescription>
    </SheetHeader>

    <SheetFooter class="mt-auto pt-4">
      <!-- <div class="flex items-center justify-between w-full">
        <div class="text-sm text-muted-foreground" v-if="loadingUser">
          正在加载...
        </div>
        <Button
          variant="ghost"
          size="icon"
          class="bg-gray-100 hover:bg-gray-200 text-gray-600 border border-gray-200 rounded-full disabled:opacity-60"
          :disabled="loadingUser"
          @click="fetchUser"
        >
          <RotateCw class="w-4 h-4" />
        </Button>
      </div> -->
      <Button variant="destructive" class="w-full mt-3" @click="handleLogout">
        退出登录
      </Button>
    </SheetFooter>
  </SheetContent>
</template>
