<script setup lang="ts">
import { House, Plus,Settings } from 'lucide-vue-next';


</script>
<template>
  <div id="bottom" class="px-4 pb-4 flex items-center justify-center">
    <div class="flex flex-row items-center rounded-full justify-center bg-accent px-4 py-2 gap-6 *:hover:cursor-pointer">
      <House></House>
      <div class="bg-primary rounded-full p-2">
        <Plus class="scale-150"></Plus>
      </div>
      <Settings></Settings>
    </div>
  </div>
</template>