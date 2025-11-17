<script setup lang="ts">
import BottomTag from '@/components/ui/bottomTag/BottomTag.vue';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { bookingRequest } from '@/lib/api/BookingRequest';
import type { Booking } from '@/lib/types/Booking';
import { http } from '@/lib/Utils';
import { onMounted, ref } from 'vue';
import dayjs from 'dayjs';

const bookings = ref<Booking[]>([]);

async function getBookings() {
  try {
    const res = await bookingRequest.getMyBookings();
    bookings.value = res;
  } catch (err) {
    console.error(err);
  }
}

onMounted(() => {
  getBookings();
});

</script>

<template>
  <div class="flex flex-col items-stretch justify-between h-full">
    <div class="flex-1 px-4 pt-4">
      <Card class="h-full bg-muted">
        <CardHeader>
          <CardTitle>
            我的预约
          </CardTitle>
        </CardHeader>
        <CardContent class="px-2">
          <div
            class="flex flex-col min-w-0 min-h-10 gap-y-2 max-w-full flex-nowrap gap-x-2 overflow-x-hidden overflow-y-auto [&>.bookings-enter-active,.bookings-leave-active]:transition-all [&>.bookings-enter-from,.bookings-leave-to]:opacity-0 [&>.bookings-enter-from,.bookings-leave-to]:transform-[translateX(30px)]">
            <TransitionGroup name="bookings">
              <div v-for="b in bookings" :key="b.id">
                <Card class=" py-2" @click="">
                  <CardHeader>
                    <div class="flex flex-row gap-x-2 items-center">
                      <div class="text-lg">
                        {{ b.seat?.room?.name }}
                      </div>
                      <div class="text-lg">
                        —
                      </div>
                      <div>
                        {{ (b.seat?.row ?? 0) * (b.seat?.room?.cols ?? 0) + (b.seat?.col ?? 0) }}
                      </div>
                      <div class="flex flex-row items-center justify-center ml-auto">
                        <div>{{ b.state }}</div>
                        <Trash2 class="text-red-500"></Trash2>
                      </div>
                    </div>
                    <CardDescription>
                      <div>
                        {{ dayjs(b.startTime).local().format("YYYY/MM/DD") }} {{
                          dayjs(b.startTime).local().format("HH:mm:ss") }} - {{
                          dayjs(b.endTime).local().format("HH:mm:ss") }}
                      </div>
                    </CardDescription>
                  </CardHeader>
                </Card>
              </div>
            </TransitionGroup>
          </div>
        </CardContent>
      </Card>
    </div>
    <!-- <BottomTag /> -->
  </div>
</template>
