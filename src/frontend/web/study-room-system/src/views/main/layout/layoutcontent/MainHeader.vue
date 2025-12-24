<script setup lang="ts">
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import { Sheet } from "@/components/ui/sheet"
import { UserRound } from 'lucide-vue-next'
import type { User } from '@/lib/types/User'
import { computed, ref } from 'vue'
import MainUserSheetContent from './MainUserSheetContent.vue'

const avatarCacheKey = 'userAvatar'
const user = ref<User | null>(null)
const isSheetOpen = ref(false)

function loadUserFromCache() {
  const raw = localStorage.getItem('user')
  const cachedAvatar = localStorage.getItem(avatarCacheKey)
  if (!raw) {
    user.value = null
    return
  }
  try {
    const parsed = JSON.parse(raw) as User
    if (!parsed.avatar && cachedAvatar) {
      parsed.avatar = cachedAvatar
    }
    user.value = parsed
  } catch {
    user.value = null
  }
}

const avatarSrc = computed(() => user.value?.avatar || localStorage.getItem(avatarCacheKey) || '')

loadUserFromCache()

function handleSheetOpen(val: boolean) {
  isSheetOpen.value = val
  if (!val) {
    loadUserFromCache()
  }
}
</script>

<template>
  <header class="flex flex-row items-center w-full h-14 px-4 bg-accent text-lg">
    <div class="font-semibold">
      智慧自习室预约管理系统
    </div>

    <div class="ml-auto flex flex-row items-center gap-x-2 text-base">
      <Avatar class="hover:cursor-pointer" @click="isSheetOpen = true">
        <AvatarImage :src="avatarSrc" />
        <AvatarFallback>
          <UserRound />
        </AvatarFallback>
      </Avatar>

      <Sheet v-model:open="isSheetOpen" @update:open="handleSheetOpen">
        <MainUserSheetContent />
      </Sheet>
    </div>
  </header>
</template>
