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
import { UserRound } from "lucide-vue-next";

import type { User } from "@/lib/types/User";
import { userRequest } from "@/lib/api/userRequest"; 
import { logout } from "@/lib/utils";
import { useRouter } from "vue-router";
import { onMounted, ref } from "vue";

const router = useRouter();


const user = ref<User | null>(null);


const rawUser = localStorage.getItem("user");
if (rawUser) {
  try {
    user.value = JSON.parse(rawUser) as User;
  } catch (e) {
    console.error("解析本地 user 失败", e);
  }
}


async function fetchUser() {
  try {
    const res = await userRequest.getUser(); 
    const data = res.data as User;
    user.value = data;
    // 同步更新本地缓存
    localStorage.setItem("user", JSON.stringify(data));
  } catch (err) {
    console.error("获取用户信息失败", err);
  }
}

onMounted(() => {
  fetchUser();
});

// 登出
async function handleLogout() {
  try {
    await logout();
  } finally {
    localStorage.removeItem("user");
    router.push("/login");
  }
}
</script>

<template>
  <SheetContent class="flex flex-col h-full">
    <SheetHeader>
      <SheetTitle class="flex flex-row items-center gap-x-4">
        <!-- 头像 -->
        <Avatar class="hover:cursor-pointer aspect-square w-20 h-20">
          <AvatarImage :src="user?.avatar ?? ''" />
          <AvatarFallback>
            <UserRound class="h-full w-full" />
          </AvatarFallback>
        </Avatar>

        <!-- 名称 + 角色 -->
        <div class="flex flex-col items-start gap-y-1">
          <div class="text-2xl font-semibold">
            {{ user?.displayName || user?.userName || "未命名用户" }}
          </div>
          <!-- <div
            class="inline-flex items-center rounded-full border px-2 py-0.5 text-xs text-muted-foreground bg-background"
          >
            角色：{{ user?.role || "未设置" }}
          </div> -->
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
      <Button variant="destructive" class="w-full" @click="handleLogout">
        退出登录
      </Button>
    </SheetFooter>
  </SheetContent>
</template>
