<script setup lang="ts">
import { useRoute, useRouter } from 'vue-router'
import { House, CalendarDays, UserRound} from 'lucide-vue-next'
import { Button } from '@/components/ui/button'

const router = useRouter()
const route = useRoute()

const navItems = [
  { label: '座位预约', path: '/seatbooking', icon: House },
  { label: "用户中心", path: "/mybookings", icon: CalendarDays },
  { label: "用户中心", path: "/usercenter", icon: UserRound },

  // 弃用
  // { label: '我的预约', path: '/calendar', icon: CalendarDays },
]

function goTo(path: string) {
  if (route.path !== path) {
    router.push(path)
  }
}
</script>

<template>
  <div class="h-full">
    <nav
      class="h-full bg-accent border-r flex flex-col items-center py-4 w-14 md:w-48 transition-all duration-200 gap-y-4"
    >
      <Button
        v-for="item in navItems"
        :key="item.path"
        @click="goTo(item.path)"
        variant="ghost"
        class="flex items-center px-3 py-2 w-full "
        :class="route.path === item.path ? 'font-medium' : ''"
      >
        <component :is="item.icon" class="size-6 shrink-0" :class="route.path === item.path ? 'text-primary':''" />

        <span class="hidden md:inline-block whitespace-nowrap text-sm">
          {{ item.label }}
        </span>
      </Button>
    </nav>
  </div>
</template>
