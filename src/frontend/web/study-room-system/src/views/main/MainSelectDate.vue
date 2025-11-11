<script setup lang="ts">
import dayjs from 'dayjs';
import { onUnmounted, ref } from 'vue';


const now = ref(dayjs().format('YYYY-MM-DD HH:mm:ss'))
const timer = setInterval(() => {
  now.value = dayjs().format('YYYY-MM-DD HH:mm:ss')
}, 100);

onUnmounted(() => {
  clearInterval(timer);
});

</script>
<template>
  <div class="h-fit w-full min-w-0">
    <div class="flex flex-row items-center justify-center bg-accent h-full w-full rounded-xl p-2 px-4 min-w-0">
      <div class="text-sm text-muted-foreground">
        {{ now }}
      </div>
    </div>
  </div>
</template>