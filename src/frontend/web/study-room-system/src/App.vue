<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { http } from './lib/utils';
import { Toaster } from '@/components/ui/sonner';
import 'vue-sonner/style.css'
import { useColorMode } from '@vueuse/core';
import { toast } from 'vue-sonner';
import { authRequest } from './lib/api/authRequest';


const router = useRouter();
// const color = useColorMode();
// const theme = computed(() => {

//   if (color.value === 'light') {
//     return 'light';
//   }
//   else if(color.value === 'dark') {
//     return 'dark';
//   }
//   else {
//     return 'system';
//   }
// });
const viewTransition = ref('v');
// router.push('/login');
// router.beforeEach(async (to, from, next) => {
//   // if(to.path === '/login') return;
//   console.log(to, from);

//   if (to.path === '/login') return next();

//   if (await CheckLogin() === false) {
//     console.log('no token');
//     return next('/login');
//   }
//   return next();
// });



async function CheckLogin() {
  return await authRequest.check().then(res => {
    console.log(res);
    if (res.status === 200) {
      return true;
    }
    else {
      return false;
    }
  }).catch(error => false);
}

onMounted(async () => {
  if (await CheckLogin() === false) {
    console.log('no token');
    router.push('/login');
    toast.error('登录失效，请先登录');
  }
});

</script>

<template>
  <Toaster class="pointer-events-auto" rich-colors theme="system" />
  <div class="w-screen h-screen flex flex-col justify-center">
    <main class="flex-1">
      <!-- TODO 切换动画 -->
      <RouterView v-slot="{ Component, route }">
        <Transition :name="viewTransition">
          <component :is="Component" :key="route.path" />
        </Transition>
      </RouterView>
    </main>
  </div>
</template>
<style scoped>
.slide-left-enter-active,
.slide-left-leave-active,
.slide-right-enter-active,
.slide-right-leave-active {
  position: absolute;
  width: 100%;
  transition: transform .3s cubic-bezier(.215, .61, .355, 1);
}

.slide-left-enter-active {
  left: -100%;
}

.slide-left-enter-to {
  transform: translateX(100%);

}

.slide-left-leave-active {
  left: 0;
}

.slide-left-leave-to {
  transform: translateX(100%);

}

.slide-right-enter-active {
  left: 100%;
}

.slide-right-enter-to {
  transform: translateX(-100%);
}

.slide-right-leave-active {
  left: 0;
}

.slide-right-leave-to {
  transform: translateX(-100%);
}
</style>