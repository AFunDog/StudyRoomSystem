<script setup lang="ts">
import { inject, ref, watch } from 'vue';
import { SELECT_ROOM } from './define';
import { cn, http } from '@/lib/utils';
import { Armchair, Dot } from 'lucide-vue-next';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
  DialogClose
} from "@/components/ui/dialog"
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage
} from '@/components/ui/form'
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover"
import { CalendarIcon } from 'lucide-vue-next';
import { Calendar } from '@/components/ui/calendar';

import Button from '@/components/ui/button/Button.vue';
import type { Seat } from '@/lib/types/seat';
import { computed, type RefSymbol } from '@vue/reactivity';
import z from 'zod';
import { toTypedSchema } from '@vee-validate/zod';
import type { GenericObject } from 'vee-validate';
import type { DateValue } from 'reka-ui';
import { parseDate, CalendarDate } from '@internationalized/date';
import dayjs from 'dayjs';
import ViewBox from '@/components/ui/view-box/ViewBox.vue';

const isSelectDialogOpen = ref(false);
const selectRoom = inject(SELECT_ROOM)!;
const seats = ref<Seat[]>([]);
const selectSeat = ref<Seat | null>(null);

const seatSchema = z.object({
  date: z.preprocess(val => typeof val === 'string' ? parseDate(val) : val, z.custom<CalendarDate>(v => v !== null && v !== undefined, {
    message: '请选择有效的日期'
  })),
  startHour: z.number(),
  startMin: z.number(),
  // endDate: z.preprocess(val => typeof val === 'string' ? parseDate(val) : val, z.custom<DateValue>(v => v !== null)),
  endHour: z.number(),
  endMin: z.number(),
})

const seatFormSchema = toTypedSchema(seatSchema);

watch(selectRoom, (room) => {
  if (!room) return;

  seats.value = Array<Seat>(room.rows * room.cols);
  room.seats?.forEach(seat => {
    seats.value[seat.row * (room.cols ?? 0) + seat.col] = seat;
  })
})

function isSeatValid(i: number) {
  return selectRoom?.value?.seats?.find(s => s.row * (selectRoom?.value?.cols ?? 0) + s.col === i);
}
function onSeatClick(seat: Seat) {
  isSelectDialogOpen.value = true;
  selectSeat.value = seat;
}
async function onSubmit(values: GenericObject) {
  try {
    if (values.date instanceof CalendarDate)
      console.log({
        seatId: selectSeat.value?.id,
        startTime: dayjs(values.date.toString()).add(values.startHour, 'h').add(values.startMin, 'm').toISOString(),
        endTime: dayjs(values.date.toString()).add(values.endHour, 'h').add(values.endMin, 'm').toISOString(),
      });
    const res = await http.post('/api/v1/booking', {
      seatId: selectSeat.value?.id,
      startTime: dayjs(values.date.toString()).add(values.startHour, 'h').add(values.startMin, 'm').toISOString(),
      endTime: dayjs(values.date.toString()).add(values.endHour, 'h').add(values.endMin, 'm').toISOString(),
    })
    console.log(res);
    isSelectDialogOpen.value = false;
  }
  catch (error) {
    console.log(error);
  }
}

// const value = ref<DateValue>();

</script>
<template>

  <div class="bg-accent p-4 rounded-xl h-full w-full grid grid-rows-[auto_1fr] ">
    <div class="flex items-center justify-center gap-x-2">
      <div>
        <div class="rounded-full w-3 h-3 bg-green-400"></div>
      </div>
      <div>
        {{ selectRoom?.name }}
      </div>
    </div>
    <div>
      <ViewBox>
        <!-- <div class="flex flex-col items-center justify-center h-full w-full"> -->

        <div v-if="selectRoom != null" :class="cn('grid border-gray-500 border-4 rounded-md')"
          v-show="selectRoom != null" :style="{
            'grid-template-columns': `repeat(${selectRoom?.cols},1fr)`
          }">
          <div v-for="(s, i) in seats" :key="i" :class="cn([isSeatValid(i) ? '' : 'invisible pointer-events-none'])">
            <Armchair class="size-12 active:text-primary transition-colors ease-in-out" @click="onSeatClick(s)" />
            <!-- <div class="justify-self-center">{{ i + 1 }}</div> -->
          </div>

        </div>
        <!-- </div> -->
      </ViewBox>

      <Dialog v-model:open="isSelectDialogOpen">
        <DialogContent>
          <DialogHeader>
            <DialogTitle>预约座位</DialogTitle>
            <DialogDescription>
              选择预约的时间范围
            </DialogDescription>
          </DialogHeader>
          <div>
            <Form id="bookseat-form" class="flex flex-col gap-y-4" :validation-schema="seatFormSchema"
              @submit="onSubmit">
              <FormField v-slot="{ componentField }" name="date">
                <FormItem>
                  <FormLabel>预约日期</FormLabel>
                  <FormDescription>
                    选择预约日期
                  </FormDescription>
                  <FormControl>
                    <Popover>
                      <PopoverTrigger as-child>
                        <Button variant="outline" :class="cn(
                          'w-[280px] justify-start text-left font-normal'
                        )">
                          <CalendarIcon class="mr-2 h-4 w-4" />
                          {{ componentField.modelValue ?? "选择预约日期" }}
                        </Button>
                      </PopoverTrigger>
                      <PopoverContent class="w-auto p-0">
                        <Calendar v-bind="componentField" initial-focus />
                      </PopoverContent>
                    </Popover>
                  </FormControl>
                  <FormMessage />
                </FormItem>
              </FormField>
              <!-- <div class="flex flex-row gap-x-4"> -->
              <div class="flex flex-row gap-x-2 items-end">
                <FormField v-slot="{ componentField }" name="startHour">
                  <FormItem>
                    <FormLabel>开始时间</FormLabel>
                    <FormControl>
                      <Select v-bind="componentField">
                        <SelectTrigger>
                          <SelectValue placeholder="时" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectGroup>
                            <SelectLabel>时</SelectLabel>
                            <SelectItem v-for="v in Array(24).fill(0).map((_, i) => i + 1)" :value="v">{{ v }}
                            </SelectItem>
                          </SelectGroup>
                        </SelectContent>
                      </Select>
                    </FormControl>
                  </FormItem>
                </FormField>
                <FormField v-slot="{ componentField }" name="startMin">
                  <FormItem>
                    <FormControl>
                      <Select v-bind="componentField">
                        <SelectTrigger>
                          <SelectValue placeholder="钟" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectGroup>
                            <SelectLabel>钟</SelectLabel>
                            <SelectItem v-for="v in Array(12).fill(0).map((_, i) => i * 5)" :value="v">{{
                              v.toString().padStart(2, '0') }}</SelectItem>
                          </SelectGroup>
                        </SelectContent>
                      </Select>
                    </FormControl>
                  </FormItem>
                </FormField>
              </div>
              <!-- </div> -->
              <div class="flex flex-row gap-x-2 items-end">


                <FormField v-slot="{ componentField }" name="endHour">
                  <FormItem>
                    <FormLabel>结束时间</FormLabel>
                    <FormControl>
                      <Select v-bind="componentField">
                        <SelectTrigger>
                          <SelectValue placeholder="时" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectGroup>
                            <SelectLabel>时</SelectLabel>
                            <SelectItem v-for="v in Array(24).fill(0).map((_, i) => i + 1)" :value="v">{{ v }}
                            </SelectItem>
                          </SelectGroup>
                        </SelectContent>
                      </Select>
                    </FormControl>
                  </FormItem>
                </FormField>
                <FormField v-slot="{ componentField }" name="endMin">
                  <FormItem>
                    <FormControl>
                      <Select v-bind="componentField">
                        <SelectTrigger>
                          <SelectValue placeholder="钟" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectGroup>
                            <SelectLabel>钟</SelectLabel>
                            <SelectItem v-for="v in Array(12).fill(0).map((_, i) => i * 5)" :value="v">{{
                              v.toString().padStart(2, '0') }}</SelectItem>
                          </SelectGroup>
                        </SelectContent>
                      </Select>
                    </FormControl>
                  </FormItem>
                </FormField>
              </div>
            </Form>
          </div>
          <DialogFooter>
            <div class="flex flex-row items-center justify-center gap-x-2">
              <DialogClose>取消</DialogClose>
              <Button variant="default" type="submit" form="bookseat-form">确认</Button>
            </div>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>

  </div>
</template>