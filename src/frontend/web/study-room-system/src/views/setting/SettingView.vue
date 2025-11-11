<script setup lang="ts">
import BottomTag from '@/components/bottomTag/BottomTag.vue';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { bookingRequest } from '@/lib/api/bookingRequest';
import type { Booking } from '@/lib/types/booking';
import { http } from '@/lib/utils';
import { onMounted, ref } from 'vue';
import dayjs from 'dayjs';
</script>

<template>
    <div class="flex flex-col items-stretch justify-between h-full">
    <div class="flex-1 p-4">
      <Card class="h-full bg-muted">
        <CardHeader>
          <CardTitle>
            设置
          </CardTitle>
        </CardHeader>
        <CardContent class="px-2">

        </CardContent>
      </Card>
    </div>
    <BottomTag />
  </div>
</template>