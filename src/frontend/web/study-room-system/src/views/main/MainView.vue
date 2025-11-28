<script setup lang="ts">
import MainHeader from './home/MainHeader.vue'
import MainContent from './home/MainContent.vue'
import MainBottom from './home/MainBottom.vue'
import { provide, ref, watch } from 'vue';
import type { Room } from '@/lib/types/room';
import { SELECT_ROOM } from './define';
import BottomTag from '@/components/ui/bottomTag/BottomTag.vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const selectRoom = ref<Room | null>(null);
const viewTransition = ref('');
const curIndex = ref(router.currentRoute.value.meta.index as number)

watch(selectRoom, (room) => {
  console.log('selectRoom', room);
});

// 路由切换动画
router.beforeEach((to, from) => {
  curIndex.value = to.meta.index as number
  if ((to.meta.index as number) > (from.meta.index as number)) {
    viewTransition.value = 'slide-right'
  }
  else if ((to.meta.index as number) < (from.meta.index as number)) {
    viewTransition.value = 'slide-left'
  }
  else {
    viewTransition.value = ''
  }
})



provide(SELECT_ROOM, selectRoom);
</script>

<template>

  <!-- 子元素设置 min-w-0 能解决宽度溢出的问题 -->
  <div class="flex flex-col w-full h-full gap-y-4 *:min-w-0">
    <!-- <MainHeader /> -->
    <!-- <MainContent /> -->
    <!-- <MainBottom /> -->
    <div class="flex flex-row w-full h-full">

      <RouterView v-slot="{ Component, route }" class="">
        <Transition :name="viewTransition">
          <component :is="Component" />
        </Transition>
      </RouterView>
    </div>
      <BottomTag />
  </div>
</template>

<style scoped>
.slide-left-enter-active,
.slide-left-leave-active,
.slide-right-enter-active,
.slide-right-leave-active {
  position: relative;
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