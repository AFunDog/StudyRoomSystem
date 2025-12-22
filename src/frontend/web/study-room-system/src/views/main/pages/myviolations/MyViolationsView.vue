<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { toast } from "vue-sonner";
import dayjs from "dayjs";
import type { Violation } from "@/lib/types/Violation";
import { violationRequest } from "@/lib/api/violationRequest";
import ViolationFilters from "./components/ViolationFilters.vue";
import ViolationList from "./components/ViolationList.vue";
import { Button } from "@/components/ui/button";
import { RotateCw } from "lucide-vue-next";

type TypeFilter = "all" | Violation["type"];

const violations = ref<Violation[]>([]);
const loading = ref(false);
const loadingMore = ref(false);
const page = ref(1);
const pageSize = 20;
const hasMore = ref(true);

const typeFilter = ref<TypeFilter>("all");

const filteredViolations = computed(() => {
  return violations.value
    .filter((v) => typeFilter.value === "all" || v.type === typeFilter.value)
    .sort(
      (a, b) =>
        dayjs(b.createTime).valueOf() -
        dayjs(a.createTime).valueOf()
    );
});

async function loadViolations(reset = false) {
  if (reset) {
    page.value = 1;
    hasMore.value = true;
    violations.value = [];
  }
  if (!hasMore.value && !reset) return;

  if (page.value === 1) {
    loading.value = true;
  } else {
    loadingMore.value = true;
  }

  try {
    const res = await violationRequest.getMyViolations({
      page: page.value,
      pageSize,
    });

    if (res.items.length < pageSize || page.value * pageSize >= res.total ) {
      hasMore.value = false;
    }

    if (page.value === 1) {
      violations.value = res.items;
    } else {
      violations.value = [...violations.value, ...res.items];
    }
  } catch (err) {
    console.error("获取违规记录失败", err);
    toast.error("获取违规记录失败，请稍后重试");
  } finally {
    loading.value = false;
    loadingMore.value = false;
  }
}

async function loadMore() {
  if (!hasMore.value) return;
  page.value += 1;
  await loadViolations(false);
}

onMounted(() => {
  loadViolations(true);
});
</script>

<template>
  <div class="flex flex-col h-full w-full px-4 py-4 gap-4 min-h-0">
    <div class="flex items-center justify-between gap-2">
      <div class="text-lg font-semibold">
        我的违规
      </div>

      <div class="flex items-center gap-2">
        <Button
          variant="ghost"
          size="sm"
          class="flex items-center gap-1 md:hidden
                  bg-gray-100 hover:bg-gray-200
                  text-gray-600 border border-gray-200
                  disabled:opacity-60 disabled:cursor-not-allowed"
          :disabled="loading"
          @click="loadViolations(true)"
        >
          <RotateCw class="w-4 h-4" />
          <span class="text-xs">刷新</span>
        </Button>

        <!-- PC端：纯图标按钮 -->
        <Button
          variant="ghost"
          size="icon"
          class="hidden md:inline-flex
                  bg-gray-100 hover:bg-gray-200
                  text-gray-600 border border-gray-200
                  rounded-full
                  disabled:opacity-60 disabled:cursor-not-allowed"
          :disabled="loading"
          @click="loadViolations(true)"
        ><RotateCw class="w-4 h-4" />
        </Button>
      </div>
    </div>

    <ViolationFilters
      :type-filter="typeFilter"
      :loading="loading"
      @update:type-filter="typeFilter = $event as TypeFilter"
    />

    <div class="flex-1 min-h-0 h-full overflow-hidden">
      <ViolationList
        :violations="filteredViolations"
        :loading="loading"
      />
    </div>

    <div v-if="hasMore" class="mt-2 flex justify-center">
      <Button
        variant="outline"
        size="sm"
        :disabled="loadingMore"
        @click="loadMore"
      >
        {{ loadingMore ? "加载中..." : "加载更多" }}
      </Button>
    </div>
    <div
      v-else
      class="mt-1 flex justify-center text-sm text-muted-foreground"
    >
      没有更多了
    </div>
  </div>
</template>
