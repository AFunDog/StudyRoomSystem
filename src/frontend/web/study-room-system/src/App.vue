<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { http } from './lib/Utils';
import { Toaster } from '@/components/ui/sonner';
import 'vue-sonner/style.css'
import { useColorMode } from '@vueuse/core';
import { toast } from 'vue-sonner';
import { authRequest } from './lib/api/AuthRequest';


const router = useRouter();
const viewTransition = ref('main');
const curIndex = ref(0);
// router.push('/login');


// 检查登录状态
router.beforeEach(async (to, from, next) => {
  // if(to.path === '/login') return;
  console.log(to, from);
  const allowUrls = ['/login', '/admin/login','/register'];

  if (allowUrls.includes(to.path)) return next();

  if (await CheckLogin() === false) {
    console.log('登录失效');
    if (to.path.startsWith('/admin')) {
      return next('/admin/login');
    }
    else {
      return next('/login');
    }
  }
  return next();
});

// 路由切换动画
// router.beforeEach((to, from) => {
//   curIndex.value = to.meta.index as number
//   if ((to.meta.index as number) > (from.meta.index as number)) {
//     viewTransition.value = 'slide-right'
//   }
//   else if ((to.meta.index as number) < (from.meta.index as number)) {
//     viewTransition.value = 'slide-left'
//   }
//   else {
//     viewTransition.value = ''
//   }
// })


async function CheckLogin() {
  return await authRequest.check().then(res => {
    console.log(res);
    return res ?? false;
  }).catch(error => false);
}

</script>

<template>
  <Toaster class="pointer-events-auto" rich-colors theme="system" />
  <div class="w-screen h-screen flex flex-col justify-center">
    <!-- 给 main 设置 overflow: hidden; position: relative;，保证过渡元素始终在容器内 -->
    <main class="flex-1 overflow-hidden relative">
      <RouterView v-slot="{ Component, route }">
        <Transition :name="viewTransition">
          <component :is="Component"  />
        </Transition>
      </RouterView>
    </main>
  </div>
</template>
<style scoped>
/* 淡入淡出动画 */
.main-enter-active,
.main-leave-active {
  position: absolute;
  width: 100%;
  transition: all .3s cubic-bezier(.215, .61, .355, 1);
}

.main-enter-from {
  opacity: 0;
  transform: translateY(32px);
}

.main-leave-to {
  opacity: 0;
  transform: translateY(-32px);
}


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