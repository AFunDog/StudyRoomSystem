<script setup lang="ts">
import BottomTag from '@/components/ui/bottomTag/BottomTag.vue';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { bookingRequest } from '@/lib/api/BookingRequest';
import type { Booking } from '@/lib/types/Booking';
import { http } from '@/lib/Utils';
import { onMounted, ref } from 'vue';
import dayjs from 'dayjs';
import type { User } from '@/lib/types/User';

const user = JSON.parse(localStorage.getItem('user')!) as User;


</script>
<template>
    <div class="flex flex-col items-stretch justify-between h-full">
    <div class="flex-1 p-4">
      <Card class="h-full bg-muted">
        <CardContent class="px-2">
          <div class="flex flex-col">
            <div class="flex flex-row">
              <div>
                {{ user.displayName }}
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
    <BottomTag />
  </div>
</template> 