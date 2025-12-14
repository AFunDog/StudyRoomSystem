<script setup lang="ts">
import { ref } from 'vue'
import dayjs from 'dayjs'
import { toast } from 'vue-sonner'

import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover'
import { CalendarIcon } from 'lucide-vue-next'
import { Calendar } from '@/components/ui/calendar'
import Button from '@/components/ui/button/Button.vue'
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { cn } from '@/lib/utils'

const props = defineProps<{
  modelValue?: { start: string; end: string } | null
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: { start: string; end: string } | null): void
}>()

// 不跟 Calendar 较真类型，保持简单
const date = ref<any | null>(null)
const startHour = ref<string | null>(null)
const startMin = ref<string | null>(null)
const endHour = ref<string | null>(null)
const endMin = ref<string | null>(null)

function applyTimeRange() {
  if (
    !date.value ||
    startHour.value == null ||
    startMin.value == null ||
    endHour.value == null ||
    endMin.value == null
  ) {
    toast.error('请完整选择预约日期和时间')
    return
  }

  const base = dayjs(date.value.toString())

  const start = base
    .hour(Number(startHour.value))
    .minute(Number(startMin.value))
    .second(0)
    .millisecond(0)

  const end = base
    .hour(Number(endHour.value))
    .minute(Number(endMin.value))
    .second(0)
    .millisecond(0)

  if (!start.isBefore(end)) {
    toast.error('开始时间必须早于结束时间')
    return
  }

  const startIso = start.toDate().toISOString()
  const endIso = end.toDate().toISOString()

  emit('update:modelValue', { start: startIso, end: endIso })
  toast.success('预约时间已设置')
}
</script>

<template>
  <div class="w-full bg-accent rounded-xl p-3 min-w-0">
    <div
      class="flex flex-col gap-3
             md:flex-row md:items-end md:gap-4"
    >
      <!-- 日期选择 -->
      <div class="flex flex-col gap-1">
        <div class="text-sm font-medium">
          预约日期
        </div>
        <Popover>
          <PopoverTrigger as-child>
            <Button
              variant="outline"
              :class="cn(
                'w-full md:w-[220px] justify-start text-left font-normal'
              )"
            >
              <CalendarIcon class="mr-2 h-4 w-4" />
              <span>
                {{ date ? date.toString() : '选择预约日期' }}
              </span>
            </Button>
          </PopoverTrigger>
          <PopoverContent class="w-auto p-0">
            <Calendar v-model="date" initial-focus />
          </PopoverContent>
        </Popover>
      </div>

      <!-- 开始时间 -->
      <div class="flex flex-row gap-2 items-end">
        <div>
          <div class="text-sm font-medium mb-1">
            开始时间
          </div>
          <Select v-model="startHour">
            <SelectTrigger class="w-[80px]">
              <SelectValue placeholder="时" />
            </SelectTrigger>
            <SelectContent>
              <SelectGroup>
                <SelectLabel>时</SelectLabel>
                <SelectItem
                  v-for="v in Array(24).fill(0).map((_, i) => i)"
                  :key="v"
                  :value="String(v)"
                >
                  {{ v.toString().padStart(2, '0') }}
                </SelectItem>
              </SelectGroup>
            </SelectContent>
          </Select>
        </div>

        <div>
          <Select v-model="startMin">
            <SelectTrigger class="w-[80px]">
              <SelectValue placeholder="分" />
            </SelectTrigger>
            <SelectContent>
              <SelectGroup>
                <SelectLabel>分</SelectLabel>
                <SelectItem value="0">00</SelectItem>
                <SelectItem value="30">30</SelectItem>
              </SelectGroup>
            </SelectContent>
          </Select>
        </div>
      </div>

      <!-- 结束时间 -->
      <div class="flex flex-row gap-2 items-end">
        <div>
          <div class="text-sm font-medium mb-1">
            结束时间
          </div>
          <Select v-model="endHour">
            <SelectTrigger class="w-[80px]">
              <SelectValue placeholder="时" />
            </SelectTrigger>
            <SelectContent>
              <SelectGroup>
                <SelectLabel>时</SelectLabel>
                <SelectItem
                  v-for="v in Array(24).fill(0).map((_, i) => i)"
                  :key="v"
                  :value="String(v)"
                >
                  {{ v.toString().padStart(2, '0') }}
                </SelectItem>
              </SelectGroup>
            </SelectContent>
          </Select>
        </div>

        <div>
          <Select v-model="endMin">
            <SelectTrigger class="w-[80px]">
              <SelectValue placeholder="分" />
            </SelectTrigger>
            <SelectContent>
              <SelectGroup>
                <SelectLabel>分</SelectLabel>
                <SelectItem value="0">00</SelectItem>
                <SelectItem value="30">30</SelectItem>
              </SelectGroup>
            </SelectContent>
          </Select>
        </div>
      </div>

      <!-- 应用按钮 -->
      <div class="md:ml-auto">
        <Button class="mt-2 md:mt-4" @click="applyTimeRange">
          应用时间
        </Button>
      </div>
    </div>
  </div>
</template>
