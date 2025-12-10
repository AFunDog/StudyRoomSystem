<script setup lang="ts">
import { useRoute, useRouter } from 'vue-router'
import { House, CalendarDays, UserRound} from 'lucide-vue-next'

const router = useRouter()
const route = useRoute()

const navItems = [
  { label: '座位预约', path: '/', icon: House },
  { label: '我的预约', path: '/calendar', icon: CalendarDays },
  { label: "用户中心", path: "/user", icon: UserRound },
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
      class="h-full bg-slate-50 border-r flex flex-col items-center py-4
             w-14 md:w-48 transition-all duration-200"
    >
      <button
        v-for="item in navItems"
        :key="item.path"
        @click="goTo(item.path)"
        class="flex items-center gap-3 px-3 py-2 w-full hover:bg-slate-100 text-slate-700"
        :class="route.path === item.path ? 'bg-slate-100 font-medium' : ''"
      >
        <component :is="item.icon" class="w-5 h-5 shrink-0" />

        <span class="hidden md:inline-block whitespace-nowrap text-sm">
          {{ item.label }}
        </span>
      </button>
    </nav>
  </div>
</template>
