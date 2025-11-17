<script setup lang="ts">
import { useConfig } from '@/lib/Config';
import { cn } from '@/lib/Utils';
import { CalendarClock, CircleUserRound, House, Plus, Settings } from 'lucide-vue-next';
import { onMounted, onUnmounted, ref } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const config = useConfig();
const bottomTag = router.getRoutes().filter(x => x.name === 'main')[0]?.children;
console.log(bottomTag)
const currentRoute = ref('');

onMounted(() =>{
  currentRoute.value = router.currentRoute.value.path;
  router.afterEach((to,from)=>{
    currentRoute.value = to.path;
  });
});


</script>
<template>
  <div id="bottom" class="px-4 mb-4 flex items-center justify-center">
    <div
      class="flex flex-row items-center rounded-full justify-center bg-accent px-4 py-2 gap-6 *:hover:cursor-pointer *:z-1 shadow-sm">
      <div v-for="t in bottomTag" :class="cn([currentRoute == t.path ? 'text-primary' : ''])">
        <component :is="t.meta?.icon" @click="router.push(t.path)"></component>
      </div>
      <!-- <CalendarClock></CalendarClock> -->
      <!-- <div class="bg-primary rounded-full p-2 absolute size-8 -z-1">
      </div> -->
      <!-- <House></House> -->
      <!-- <Settings></Settings> -->
    </div>
  </div>
</template>