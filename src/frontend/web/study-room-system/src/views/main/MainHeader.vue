<script setup lang="ts">
import { Button } from '@/components/ui/button';
import type { User } from '@/lib/types/user';

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
import { Menu, UserRound } from 'lucide-vue-next';
import { useRouter } from 'vue-router';

const router = useRouter();
var user = JSON.parse(localStorage.getItem('user')!) as User | null;

function logout() {
  localStorage.removeItem('token');
  localStorage.removeItem('user');
  router.push('/login');
}

</script>
<template>
  <div class="flex flex-row p-4 items-center bg-accent text-lg w-full">
    <Menu class="size-6 mr-2 hover:cursor-pointer"></Menu>
    <div>智慧自习室预约管理系统</div>
    <div class="ml-auto flex flex-row items-center gap-x-2 text-base">
      <div>{{ user?.displayName }}</div>
      <!-- <Dialog key="userDialog">
        <DialogTrigger as-child>
          <UserRound class="hover:cursor-pointer"></UserRound>
        </DialogTrigger>
        <DialogContent class="sm:max-w-[425px]"> 
          <DialogHeader>
            <DialogTitle>用户信息</DialogTitle>
            <DialogDescription>
              <div>用户名：{{ user?.userName }}</div>
              <div>用户角色：{{ user?.role }}</div>
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="destructive" @click="logout()">退出登录</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog> -->
      <Sheet>
        <SheetTrigger as-child>
          <UserRound class="hover:cursor-pointer"></UserRound>
        </SheetTrigger>
        <SheetContent>
          <SheetHeader>
            <SheetTitle class="flex flex-row items-center justify-center gap-x-2 text-2xl">
              <div>{{ user?.displayName }}</div>
            </SheetTitle>
            <SheetDescription>
              <div>用户名：{{ user?.userName }}</div>
              <!-- <div>Id：{{ user?.id }}</div> -->
               <div>学号/工号：{{ user?.campusId }}</div>
              <div>手机号：{{ user?.phone }}</div>
              <div>邮箱：{{ user?.email }}</div>
              <div>角色：{{ user?.role }}</div>
            </SheetDescription>
          </SheetHeader>
        </SheetContent>
      </Sheet>
    </div>
  </div>
</template>