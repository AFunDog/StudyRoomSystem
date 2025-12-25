<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import dayjs from 'dayjs'
import utc from 'dayjs/plugin/utc'
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
import type { Room } from '@/lib/types/Room'

dayjs.extend(utc)

const props = defineProps<{
  modelValue?: { start: string; end: string } | null
  room?: Room | null
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: { start: string; end: string } | null): void
}>()

// 日历 + 时间
const date = ref<any | null>(null)
const startHour = ref<string | null>(null)
const startMin = ref<string | null>(null)
const endHour = ref<string | null>(null)
const endMin = ref<string | null>(null)

/** 解析 "HH:mm:ss" */
function parseTimeStr(t: string | undefined | null) {
  if (!t) return { hour: 0, minute: 0 }
  const [hStr, mStr] = t.split(':')
  const h = Number(hStr) || 0
  const m = Number(mStr) || 0
  return { hour: h, minute: m }
}

/**
 * 基于房间开放时间，向内取整到 30min，得到最早/最晚可选时间。
 * 例如 06:50~18:20 会变成 07:00~18:00。
 * 如果配置异常，就返回 null（此时不限时间，退回 0–23 点）。
 */
const minMaxTime = computed<{
  minHour: number
  minMinute: number
  maxHour: number
  maxMinute: number
} | null>(() => {
  if (!props.room?.openTime || !props.room?.closeTime) {
    return null
  }

  const open = parseTimeStr(props.room.openTime)
  const close = parseTimeStr(props.room.closeTime)

  let startMinutes = open.hour * 60 + open.minute
  let endMinutes = close.hour * 60 + close.minute

  const startRounded = Math.ceil(startMinutes / 30) * 30
  const endRounded = Math.floor(endMinutes / 30) * 30

  if (endRounded <= startRounded) {
    return null
  }

  return {
    minHour: Math.floor(startRounded / 60),
    minMinute: startRounded % 60,
    maxHour: Math.floor(endRounded / 60),
    maxMinute: endRounded % 60,
  }
})

/** 统一的 0–23 小时列表，用作兜底 */
const allHours = Array.from({ length: 24 }, (_, i) => i)
const minuteBaseOptions = [0, 30]

// 开始/结束时间：小时选项，按开放时间裁剪；如果没配置就 0–23
const startHourOptions = computed(() => {
  if (!minMaxTime.value) return allHours
  const res: number[] = []
  for (let h = minMaxTime.value.minHour; h <= minMaxTime.value.maxHour; h++) {
    res.push(h)
  }
  return res
})

const endHourOptions = computed(() => {
  if (!minMaxTime.value) return allHours
  const res: number[] = []
  for (let h = minMaxTime.value.minHour; h <= minMaxTime.value.maxHour; h++) {
    res.push(h)
  }
  return res
})

// 开始分钟选项：如果在起始小时，只能选 >= minMinute 的 0/30
const startMinuteOptions = computed(() => {
  if (!minMaxTime.value) return minuteBaseOptions
  if (startHour.value == null) return minuteBaseOptions
  const h = Number(startHour.value)
  const { minHour, minMinute } = minMaxTime.value

  if (h > minHour) return minuteBaseOptions
  if (h < minHour) return [] // 理论上不会出现

  return minuteBaseOptions.filter((m) => m >= minMinute)
})

// 结束分钟选项：如果在结束小时，只能选 <= maxMinute 的 0/30
const endMinuteOptions = computed(() => {
  if (!minMaxTime.value) return minuteBaseOptions
  if (endHour.value == null) return minuteBaseOptions
  const h = Number(endHour.value)
  const { maxHour, maxMinute } = minMaxTime.value

  if (h < maxHour) return minuteBaseOptions
  if (h > maxHour) return [] // 理论上不会出现

  return minuteBaseOptions.filter((m) => m <= maxMinute)
})

// 小时变化时自动修正分钟
watch(
  [startHour, () => startMinuteOptions.value],
  () => {
    if (startMin.value == null) return
    const mins = startMinuteOptions.value
    if (!mins.includes(Number(startMin.value))) {
      startMin.value = mins.length ? String(mins[0]) : null
    }
  }
)

watch(
  [endHour, () => endMinuteOptions.value],
  () => {
    if (endMin.value == null) return
    const mins = endMinuteOptions.value
    if (!mins.includes(Number(endMin.value))) {
      endMin.value = mins.length ? String(mins[0]) : null
    }
  }
)

/** 生成开放区间，用于最终校验 */
function getBoundaryRange(base: dayjs.Dayjs) {
  if (!minMaxTime.value) return null
  const { minHour, minMinute, maxHour, maxMinute } = minMaxTime.value

  const start = base
    .hour(minHour)
    .minute(minMinute)
    .second(0)
    .millisecond(0)

  const end = base
    .hour(maxHour)
    .minute(maxMinute)
    .second(0)
    .millisecond(0)

  return { start, end }
}

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

  // 按开放时间做一层校验（只在房间配置了开放时间时生效）
  const boundary = getBoundaryRange(base)
  if (boundary) {
    if (start.isBefore(boundary.start) || end.isAfter(boundary.end)) {
      toast.error(
        `该房间开放时间约束为 ${boundary.start.format(
          'HH:mm'
        )} ~ ${boundary.end.format('HH:mm')}，请在开放时间内预约`
      )
      return
    }
  }

  // 后端要求 UTC：统一转为 UTC ISO 字符串
  const startIso = dayjs(start).utc().toISOString()
  const endIso = dayjs(end).utc().toISOString()

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
      <!-- 日期 -->
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

        <!-- 展示后端配置的开放时间 -->
        <div class="text-xs text-muted-foreground mt-1">
          开放时间：
          {{ props.room?.openTime ?? '--:--' }}
          ~
          {{ props.room?.closeTime ?? '--:--' }}
        </div>
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
                  v-for="v in startHourOptions"
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
                <SelectItem
                  v-for="m in startMinuteOptions"
                  :key="m"
                  :value="String(m)"
                >
                  {{ m.toString().padStart(2, '0') }}
                </SelectItem>
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
                  v-for="v in endHourOptions"
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
                <SelectItem
                  v-for="m in endMinuteOptions"
                  :key="m"
                  :value="String(m)"
                >
                  {{ m.toString().padStart(2, '0') }}
                </SelectItem>
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
