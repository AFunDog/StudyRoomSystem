import { createRouter, createWebHistory } from 'vue-router'

// 导入页面组件
import LoginView from '@/views/login/LoginView.vue'
import MainView from '@/views/main/layout/MainView.vue'
import UserHomeView from '@/views/main/pages/home/UserHomeView.vue'
import RegisterView from '@/views/register/RegisterView.vue'
import AdminLoginView from '@/views/admin/login/AdminLoginView.vue'
import MyUserView from '@/views/main/pages/usercenter/UserCenterView.vue'
import SeatBookingView from '@/views/main/pages/seatbooking/SeatBookingView.vue'
import MyBookingsView from '@/views/main/pages/mybookings/MyBookingsView.vue'
import MyViolationsView from '@/views/main/pages/myviolations/MyViolationsView.vue'
import MyComplaintsView from '@/views/main/pages/mycomplaints/MyComplaintsView.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      component: MainView, // 首页框架静态引入
      redirect: '/userhome',
      children: [
        { path: 'userhome', component: UserHomeView },
        { path: 'seatbooking', component: SeatBookingView },
        { path: 'mybookings', component: MyBookingsView },
        { path: 'mycomplaints', component: MyComplaintsView },
        { path: 'myviolations', component: MyViolationsView },
        { path: 'usercenter', component: MyUserView },
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
