import { createRouter, createWebHistory } from 'vue-router'

// 导入页面组件
import LoginView from '@/views/login/LoginView.vue'
import MainView from '@/views/main/layout/MainView.vue'
import RegisterView from '@/views/register/RegisterView.vue'
import AdminLoginView from '@/views/admin/login/AdminLoginView.vue'
import CalendarView from '@/views/main/pages/calendar/CalendarView.vue'
import SettingView from '@/views/setting/SettingView.vue'
import MyUserView from '@/views/main/pages/usercenter/UserCenterView.vue'
import MainHomeView from '@/views/main/pages/mainhome/MainHomeView.vue'
import SeatBookingView from '@/views/main/pages/seatbooking/SeatBookingView.vue'
import MyBookingsView from '@/views/main/pages/mybookings/MyBookingsView.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      component: MainView, // 首页框架静态引入
      name: 'main',
      children: [
        { path: '/seatbooking', component: SeatBookingView },
        { path: '/mybookings', component: MyBookingsView },
        { path: '/usercenter', component: MyUserView },

        // 弃用
        { path: '/calendar', component: CalendarView },
        // { path: '/setting', component: SettingView },
      ]
    },
    // 静态引入
    { path: '/login', component: LoginView },
    { path: '/register', component: RegisterView },
    { path: '/admin/login', component: AdminLoginView },

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
