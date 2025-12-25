<script setup lang="ts">
import AdminMainHeader from './AdminMainHeader.vue';
import { ref } from 'vue';

// 引入拆分后的模块
import UsersView from './UsersView.vue';
import RoomsView from './RoomsView.vue';
import BookingsView from './BookingsView.vue';
import ComplaintsView from './ComplaintsView.vue';
import ViolationsView from './ViolationsView.vue';
import FilesView from './FilesView.vue';

import { Users, Building2, CalendarCheck, FileText, AlertTriangle, ClipboardList, Home } from 'lucide-vue-next';

// 图表
import { Line, Doughnut } from "vue-chartjs";
import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  ArcElement,
  LineElement,
  PointElement,
  CategoryScale,
  LinearScale
} from "chart.js";

ChartJS.register(
  Title,
  Tooltip,
  Legend,
  ArcElement,
  LineElement,
  PointElement,
  CategoryScale,
  LinearScale
);

const currentMenu = ref('dashboard');

// 左侧菜单配置
const menus = [
  { key: 'dashboard', label: '首页概览', icon: Home },
  { key: 'users', label: '用户管理', icon: Users },
  { key: 'rooms', label: '房间管理', icon: Building2 },
  { key: 'bookings', label: '预约管理', icon: CalendarCheck },
  { key: 'complaints', label: '投诉处理', icon: ClipboardList },
  { key: 'violations', label: '违规记录', icon: AlertTriangle },
  { key: 'files', label: '文件管理', icon: FileText },
];

// 模拟数据
const bookingCount = ref(128);
const roomUsage = ref(0.16);
const complaintCount = ref(12);

// 折线图数据 - 改为橙色主题 (#f97316 是你要的橙色)
const lineData = {
  labels: ["周一", "周二", "周三", "周四", "周五", "周六", "周日"],
  datasets: [
    {
      label: "预约数量",
      data: [12, 19, 15, 22, 30, 28, 35],
      borderColor: "#f97316", // 主橙色
      backgroundColor: "rgba(249, 115, 22, 0.1)", // 淡橙色背景
      tension: 0.4,
      fill: true,
      pointBackgroundColor: "#f97316",
      pointBorderColor: "#fff",
      pointHoverRadius: 6
    }
  ]
};

// 环形图数据 - 橙色渐变系
const donutData = {
  labels: ["房间使用率", "投诉占比", "违规占比"],
  datasets: [
    {
      data: [76, 12, 5],
      backgroundColor: [
        "#f97316", // 主橙色
        "#fb923c", // 中橙色
        "#fdba74"  // 浅橙色
      ],
      borderColor: "#fff",
      borderWidth: 2,
      hoverOffset: 10
    }
  ]
};
</script>

<template>
  <div class="flex flex-col h-screen bg-background text-foreground">
    <!-- 顶部栏 - 恢复原灰色风格，移除 primary 类 -->
    <AdminMainHeader />

    <div class="flex flex-1 overflow-hidden">
      <!-- 左侧菜单 - 恢复原灰色风格 -->
      <aside class="w-64 bg-muted p-4 border-r shadow-md">
        <div class="text-lg font-bold mb-6 text-primary">管理控制台</div>
        <ul class="space-y-2">
          <li v-for="menu in menus" :key="menu.key">
            <button
              class="flex items-center gap-x-2 w-full text-left px-3 py-2 rounded-md transition-colors duration-200"
              :class="currentMenu === menu.key 
                ? 'bg-primary text-white shadow-sm' 
                : 'hover:bg-accent hover:text-primary'"
              @click="currentMenu = menu.key"
            >
              <component :is="menu.icon" class="size-4" />
              {{ menu.label }}
            </button>
          </li>
        </ul>
      </aside>

      <!-- 主内容区 -->
      <main class="flex-1 p-6 overflow-y-auto bg-background">
        <!-- 首页概览 -->
        <div v-if="currentMenu === 'dashboard'" class="space-y-8">
          <div class="flex items-center justify-between">
            <h2 class="text-2xl font-bold">系统概览</h2>
            <span class="text-sm text-muted-foreground">更新于: {{ new Date().toLocaleString('zh-CN') }}</span>
          </div>

          <!-- 顶部统计卡片 - 橙色文字 -->
          <div class="grid grid-cols-3 gap-6">
            <div class="bg-card p-6 rounded-xl shadow-sm border border-gray-200 hover:shadow-md transition-shadow duration-300">
              <h3 class="text-lg text-muted-foreground font-medium mb-3">预约总数</h3>
              <p class="text-4xl font-bold text-[#f97316]">{{ bookingCount }}</p>
              <span class="text-sm text-green-500 mt-2 inline-block">↑ 12% 较上周</span>
            </div>

            <div class="bg-card p-6 rounded-xl shadow-sm border border-gray-200 hover:shadow-md transition-shadow duration-300">
              <h3 class="text-lg text-muted-foreground font-medium mb-3">房间使用率</h3>
              <p class="text-4xl font-bold text-[#f97316]">{{ (roomUsage * 100).toFixed(0) }}%</p>
              <span class="text-sm text-green-500 mt-2 inline-block">↑ 5% 较上周</span>
            </div>

            <div class="bg-card p-6 rounded-xl shadow-sm border border-gray-200 hover:shadow-md transition-shadow duration-300">
              <h3 class="text-lg text-muted-foreground font-medium mb-3">投诉数量</h3>
              <p class="text-4xl font-bold text-[#f97316]">{{ complaintCount }}</p>
              <span class="text-sm text-red-500 mt-2 inline-block">↓ 8% 较上周</span>
            </div>
          </div>

          <!-- 图表区域 -->
          <div class="grid grid-cols-2 gap-6">
            <!-- 折线图 -->
            <div class="bg-card p-6 rounded-xl shadow-sm border border-gray-200 hover:shadow-md transition-shadow duration-300">
              <h3 class="text-lg font-medium mb-4">最近 7 天预约趋势</h3>
              <div class="h-64">
                <Line 
                  :data="lineData" 
                  :options="{
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: { legend: { display: false } },
                    scales: { 
                      y: { beginAtZero: true, grid: { color: 'rgba(249, 115, 22, 0.05)' } }, 
                      x: { grid: { display: false } } 
                    }
                  }"
                />
              </div>
            </div>

            <!-- 环形图 -->
            <div class="bg-card p-6 rounded-xl shadow-sm border border-gray-200 hover:shadow-md transition-shadow duration-300 flex flex-col items-center">
              <h3 class="text-lg font-medium mb-4">资源占比</h3>
              <div class="w-64 h-64">
                <Doughnut 
                  :data="donutData" 
                  :options="{
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                      legend: { position: 'bottom', labels: { padding: 20, font: { size: 12 } } }
                    }
                  }"
                />
              </div>
            </div>
          </div>
        </div>

        <!-- 其他模块 - 灰色边框容器 -->
        <div class="bg-card rounded-xl shadow-sm p-6 border border-gray-200" v-else>
          <UsersView v-if="currentMenu === 'users'" />
          <RoomsView v-else-if="currentMenu === 'rooms'" />
          <BookingsView v-else-if="currentMenu === 'bookings'" />
          <ComplaintsView v-else-if="currentMenu === 'complaints'" />
          <ViolationsView v-else-if="currentMenu === 'violations'" /> 
          <FilesView v-else-if="currentMenu === 'files'" /> 
        </div>
      </main>
    </div>
  </div>
</template>

<style scoped>
/* 自定义滚动条 - 适配橙色主题 */
::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}
::-webkit-scrollbar-thumb {
  background-color: rgba(249, 115, 22, 0.4);
  border-radius: 3px;
}
::-webkit-scrollbar-track {
  background-color: rgba(249, 115, 22, 0.05);
}
</style>