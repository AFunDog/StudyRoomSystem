<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { toast } from "vue-sonner";
import dayjs from "dayjs";
import type { Violation } from "@/lib/types/Violation";
import { violationRequest } from "@/lib/api/violationRequest";
import ViolationFilters from "./components/ViolationFilters.vue";
import ViolationList from "./components/ViolationList.vue";
import { Button } from "@/components/ui/button";

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

    if (res.items.length < pageSize) {
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
    <div class="flex items-center justify-between">
      <div class="text-lg font-semibold">
        我的违规
      </div>
    </div>

    <ViolationFilters
      :type-filter="typeFilter"
      :loading="loading"
      @update:type-filter="typeFilter = $event as TypeFilter"
      @refresh="() => loadViolations(true)"
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
