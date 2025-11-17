<script setup lang="ts">
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import {
  Sheet,
  SheetClose,
  SheetContent,
  SheetDescription,
  SheetFooter,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "@/components/ui/sheet";
import { Button } from "@/components/ui/button";
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import type { User } from "@/lib/types/User";
import { Menu, UserRound } from 'lucide-vue-next';
import { AspectRatio } from "@/components/ui/aspect-ratio";
import { logout } from "@/lib/Utils";
import { useRouter } from "vue-router";
const router = useRouter();

const user = JSON.parse(localStorage.getItem('user')!) as User | null;
</script>
<template>
  <SheetContent>
    <SheetHeader>
      <SheetTitle class="flex flex-row items-center gap-x-2">
        <Avatar class="hover:cursor-pointer aspect-square size-auto w-1/3">
          <AvatarImage :src="user?.avatar ?? ''" />
          <AvatarFallback>
            <UserRound class="h-full w-full"></UserRound>
          </AvatarFallback>
        </Avatar>
        <div class="flex flex-col">
          <div class="text-2xl">{{ user?.displayName }}</div>
          <div class="flex flex-col text-sm text-muted-foreground font-normal">
            <div>用户名：{{ user?.userName }}</div>
            <div>学号/工号：{{ user?.campusId }}</div>
            <div>手机号：{{ user?.phone }}</div>
            <div>邮箱：{{ user?.email }}</div>
            <div>角色：{{ user?.role }}</div>

          </div>
        </div>
      </SheetTitle>
      <SheetDescription>
        <!-- <div>Id：{{ user?.id }}</div> -->
      </SheetDescription>
    </SheetHeader>
    <SheetFooter class="">
      <Button variant="destructive" @click="{ logout(); router.push('/login'); }">退出登录</Button>
    </SheetFooter>
  </SheetContent>
</template>