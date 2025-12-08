<script setup lang="ts">
import AdminMainHeader from './AdminMainHeader.vue';
import { ref, onMounted } from 'vue';

// 引入拆分后的模块
import UsersView from './UsersView.vue';
import RoomsView from './RoomsView.vue';
import BookingsView from './BookingsView.vue';
import ComplaintsView from './ComplaintsView.vue';
import ViolationsView from './ViolationsView.vue';
import FilesView from './FilesView.vue';

import { Users, Building2, CalendarCheck, FileText, AlertTriangle, ClipboardList, Home } from 'lucide-vue-next';


const currentMenu = ref('dashboard');

// 左侧菜单配置：每个菜单项对应一个功能模块
const menus = [
  { key: 'dashboard', label: '首页概览', icon: Home },
  { key: 'users', label: '用户管理', icon: Users },
  { key: 'rooms', label: '房间管理', icon: Building2 },
  { key: 'bookings', label: '预约管理', icon: CalendarCheck },
  { key: 'complaints', label: '投诉处理', icon: ClipboardList },
  { key: 'violations', label: '违规记录', icon: AlertTriangle },
  { key: 'files', label: '文件管理', icon: FileText },
];
</script>

<template>
  <div class="flex flex-col h-screen bg-background">
    <!-- 顶部栏 -->
    <AdminMainHeader />

    <div class="flex flex-1">
      <!-- 左侧菜单 -->                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        
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
      <main class="flex-1 p-6 overflow-y-auto bg-card rounded-lg shadow-inner">
        
        <!-- 首页概览：展示统计数据 -->
        <div v-if="currentMenu === 'dashboard'" class="space-y-4">
          <h2 class="text-2xl font-bold mb-4">系统概览</h2>
          <div class="grid grid-cols-3 gap-4">
            <!-- TODO: 调用 /api/v1/booking/my 获取预约数量 -->
            <div class="bg-accent text-accent-foreground p-4 rounded-lg shadow">
              <h3 class="text-lg font-semibold">预约总数</h3>
              <p class="text-2xl font-bold">128</p>
            </div>
            <!-- TODO: 调用 /api/v1/room 获取房间信息并计算使用率 -->
            <div class="bg-accent text-accent-foreground p-4 rounded-lg shadow">
              <h3 class="text-lg font-semibold">房间使用率</h3>
              <p class="text-2xl font-bold">76%</p>
            </div>
            <!-- TODO: 调用 /api/v1/complaint 获取投诉数量 -->
            <div class="bg-accent text-accent-foreground p-4 rounded-lg shadow">
              <h3 class="text-lg font-semibold">投诉数量</h3>
              <p class="text-2xl font-bold">12</p>
            </div>
          </div>
        </div>

        <!-- 其他模块入口 -->
        <UsersView v-else-if="currentMenu === 'users'" />
        <RoomsView v-else-if="currentMenu === 'rooms'" />
        <BookingsView v-else-if="currentMenu === 'bookings'" />
        <!-- <ComplaintsView v-else-if="currentMenu === 'complaints'" />
        <ViolationsView v-else-if="currentMenu === 'violations'" />
        <FilesView v-else-if="currentMenu === 'files'" /> -->


        <!-- 投诉处理 -->
        <!-- <div v-else-if="currentMenu === 'complaints'">
          <h2 class="text-xl font-bold mb-4">投诉处理</h2>
          <p class="text-muted-foreground">
            - GET /api/v1/complaint 获取所有投诉<br>
            - POST /api/v1/complaint 创建投诉<br>
            - PUT /api/v1/complaint 修改投诉<br>
            - GET /api/v1/complaint/{id} 获取指定投诉
          </p>
        </div> -->

        <!-- 违规记录 -->
        <!-- <div v-else-if="currentMenu === 'violations'">
          <h2 class="text-xl font-bold mb-4">违规记录</h2>
          <p class="text-muted-foreground">
            - GET /api/v1/violation 查看所有违规记录<br>
            - POST /api/v1/violation 创建违规记录<br>
            - PUT /api/v1/violation 修改违规记录<br>
            - GET /api/v1/violation/{id} 查看指定违规记录<br>
            - DELETE /api/v1/violation/{id} 删除违规记录
          </p>
        </div> -->

        <!-- 文件管理 -->
        <!-- <div v-else-if="currentMenu === 'files'">
          <h2 class="text-xl font-bold mb-4">文件管理</h2>
          <p class="text-muted-foreground">
            - POST /api/v1/file 上传文件<br>
            - GET /api/v1/file/{file} 获取文件<br>
            - DELETE /api/v1/file/{file} 删除文件
          </p>
        </div> -->
      </main>
    </div>
  </div>
</template>
