<script setup lang="ts">
import { Card, CardContent, CardHeader, CardDescription } from '@/components/ui/card';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog"
import { Button } from '@/components/ui/button';
import type { Booking } from '@/lib/types/Booking';
import { http } from '@/lib/Utils';
import dayjs from 'dayjs';
import { Trash2 } from 'lucide-vue-next';
import { onMounted, onUnmounted, ref } from 'vue';
import { getHubConnection } from '@/lib/api/HubConnection';
import { bookingRequest } from '@/lib/api/BookingRequest';

// const hubConnection = getHubConnection();
const bookings = ref<Booking[]>([]);
const isDialogOpen = ref(false);
const selectBooking = ref<Booking | null>(null);

// async function getMyBookings() {
//   try {
//     var res = await http.get('/api/v1/booking/my').then(res => res.data);
//     bookings.value = res;
//     console.log(res);
//   }
//   catch (err) {
//     console.log(err);
//   }
// }

// async function cancelBooking(id: string) {
//   try {
//     var res = await http.delete(`/api/v1/booking/${id}`).then(res => res.data);
//     getMyBookings();
//     console.log(res);
//   }
//   catch (err) {
//     console.log(err);
//   }
// }

onMounted(async () => {
  bookings.value = await bookingRequest.getMyBookings();
  getHubConnection().on('bookings-my-update', (data : Booking[]) => {
    console.log('bookings-my-update',data);
    bookings.value = data.filter(x => x.state == "Booking" || x.state == "CheckIn");
  });
})

onUnmounted(() => {
  getHubConnection().off('bookings-my-update');
});


</script>
<template>
  <div class="w-full min-w-0">
    <Dialog v-model:open="isDialogOpen">
      <DialogContent>
        <DialogHeader>
          <DialogTitle>
            我的预约
          </DialogTitle>
          <div class="flex flex-col gap-y-2">
            <div class="flex flex-row gap-x-2 items-center">
              <div class="text-lg">
                {{ selectBooking?.seat?.room?.name }}
              </div>
              <div class="text-lg">
                —
              </div>
              <div>
                {{ (selectBooking?.seat?.row ?? 0) * (selectBooking?.seat?.room?.cols ?? 0) + (selectBooking?.seat?.col
                  ?? 0) }}
              </div>

            </div>
            <div class="text-left text-muted-foreground">
              预约时间: {{ dayjs(selectBooking?.startTime).local().format("YYYY/MM/DD") }} {{
                dayjs(selectBooking?.startTime).local().format("HH:mm:ss") }} - {{
                dayjs(selectBooking?.endTime).local().format("HH:mm:ss") }}
            </div>
          </div>
        </DialogHeader>
        <DialogFooter>
          <div class="flex flex-row items-center justify-center">
            <Button variant="destructive" @click="if (selectBooking != null) {
              const res = bookingRequest.cancelBooking(selectBooking.id, true);
              console.log(res);
              isDialogOpen = false;
            }">
              取消预约
            </Button>
          </div>
        </DialogFooter>
      </DialogContent>
    </Dialog>
    <div class="bg-accent h-full w-full rounded-xl overflow-hidden p-2">
      <div
        class="flex flex-col px-4 min-w-0 min-h-5 max-h-20 gap-y-2 max-w-full flex-nowrap gap-x-2 overflow-x-hidden overflow-y-auto [&>.bookings-enter-active,.bookings-leave-active]:transition-all [&>.bookings-enter-from,.bookings-leave-to]:opacity-0 [&>.bookings-enter-from,.bookings-leave-to]:transform-[translateX(30px)]">
        <TransitionGroup name="bookings">
          <div v-for="b in bookings" :key="b.id">
            <Card class=" py-2" @click="isDialogOpen = true; selectBooking = b">
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
    </div>
  </div>
</template>