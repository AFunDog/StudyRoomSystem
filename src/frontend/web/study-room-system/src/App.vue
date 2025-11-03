<script setup lang="ts">
import { onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { http } from './lib/utils';


const router = useRouter();
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
  return await http.get('/api/v1/auth/check').then(res => {
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
  }
});

</script>

<template>
  <div>
    <main class="w-screen h-screen">
      <RouterView v-slot="{ Component, route }">
        <Transition name="viewTransition">
          <component :is="Component" :key="route.path" />
        </Transition>
      </RouterView>
    </main>
  </div>
</template>