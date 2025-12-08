import { createRouter, createWebHistory } from 'vue-router'

// 导入页面组件
import LoginView from '@/views/login/LoginView.vue'
import MainView from '@/views/main/MainView.vue'
import RegisterView from '@/views/register/RegisterView.vue'
import AdminLoginView from '@/views/admin/login/AdminLoginView.vue'
import CalendarView from '@/views/main/calendar/CalendarView.vue'
import SettingView from '@/views/setting/SettingView.vue'
import MyUserView from '@/views/user/MyUserView.vue'
import MainHomeView from '@/views/main/home/MainHomeView.vue'


// 图标
import { Calendar, House } from 'lucide-vue-next'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      component: MainView, // 首页框架静态引入
      name: 'main',
      children: [
        { path: '/calendar', component: CalendarView, meta: { icon: Calendar, index: 0 } },
        { path: '/', component: MainHomeView, meta: { icon: House, index: 1 } },
        // { path: '/setting', component: SettingView },
      ]
    },
    // 静态引入
    { path: '/login', component: LoginView },
    { path: '/register', component: RegisterView },
    { path: '/admin/login', component: AdminLoginView },
    // { path: '/user', component: MyUserView },

    // 动态加载隐私政策和用户协议页面
    { path: '/privacy-policy', component: () => import('@/views/pages/PrivacyPolicy.vue') },
    { path: '/user-agreement', component: () => import('@/views/pages/UserAgreement.vue') },

    //动态加载管理员界面
    {
      path: '/admin',
      component: () => import('@/views/admin/main/AdminMainView.vue'),
      children: [
        { path: 'dashboard', component: () => import('@/views/admin/main/DashboardView.vue') },
        { path: 'users', component: () => import('@/views/admin/main/UsersView.vue') },
        { path: 'rooms', component: () => import('@/views/admin/main/RoomsView.vue') },
        { path: 'bookings', component: () => import('@/views/admin/main/BookingsView.vue') },
        { path: 'complaints', component: () => import('@/views/admin/main/ComplaintsView.vue') },
        { path: 'violations', component: () => import('@/views/admin/main/ViolationsView.vue') },
        { path: 'files', component: () => import('@/views/admin/main/FilesView.vue') },
      ]
    }
  ]
})

export default router
