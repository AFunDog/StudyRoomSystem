<script setup lang="ts">
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import { Sheet } from "@/components/ui/sheet"
import { UserRound } from 'lucide-vue-next'
import type { User } from '@/lib/types/User'
import { ref } from 'vue'
import MainUserSheetContent from './MainUserSheetContent.vue'

const user = JSON.parse(localStorage.getItem('user') || 'null') as User | null

const isSheetOpen = ref(false)
</script>

<template>
  <header class="flex flex-row items-center w-full h-14 px-4 bg-accent text-lg">
    <div class="font-semibold">
      智慧自习室预约管理系统
    </div>

    <div class="ml-auto flex flex-row items-center gap-x-2 text-base">
      <Avatar class="hover:cursor-pointer" @click="isSheetOpen = true">
        <AvatarImage :src="user?.avatar ?? ''" />
        <AvatarFallback>
          <UserRound />
        </AvatarFallback>
      </Avatar>

      <Sheet v-model:open="isSheetOpen">
        <MainUserSheetContent />
      </Sheet>
    </div>
  </header>
</template>
