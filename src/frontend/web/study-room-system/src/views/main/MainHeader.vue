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
} from "@/components/ui/dialog"
import { Menu, UserRound } from 'lucide-vue-next';

var user = JSON.parse(localStorage.getItem('user')!) as User | null;



</script>
<template>
  <div class="flex flex-row p-4 items-center bg-accent text-lg w-full">
    <Menu class="size-6 mr-2 hover:cursor-pointer"></Menu>
    <div>智慧自习室预约管理系统</div>
    <div class="ml-auto flex flex-row items-center gap-x-2 text-base">
      <div>{{ user?.displayName }}</div>
      <Dialog key="userDialog">
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
        </DialogContent>
      </Dialog>
    </div>
  </div>
</template>