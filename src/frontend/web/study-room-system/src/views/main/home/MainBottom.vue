<script setup lang="ts">
import { cn } from '@/lib/Utils';
import { CalendarClock, House, Plus, Settings } from 'lucide-vue-next';
import { onMounted, onUnmounted, ref } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const bottomTag = [{ route: '/bookings', icon: CalendarClock }, { route: '/', icon: House },{ route : '/settings',icon : Settings }];
const currentRoute = ref('');

onMounted(() =>{
  currentRoute.value = router.currentRoute.value.path;
  router.afterEach((to,from)=>{
    currentRoute.value = to.path;
  });
});


</script>
<template>
  <div id="bottom" class="px-4 pb-4 flex items-center justify-center">
    <div
      class="flex flex-row items-center rounded-full justify-center bg-accent px-4 py-2 gap-6 *:hover:cursor-pointer *:z-1">
      <div v-for="t in bottomTag" :class="cn([currentRoute == t.route ? 'text-primary' : ''])">
        <component :is="t.icon" @click="router.push(t.route)"></component>
      </div>
      <!-- <CalendarClock></CalendarClock> -->
      <!-- <div class="bg-primary rounded-full p-2 absolute size-8 -z-1">
      </div> -->
      <!-- <House></House> -->
      <!-- <Settings></Settings> -->
    </div>
  </div>
</template>