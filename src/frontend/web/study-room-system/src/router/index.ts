import { createRouter, createWebHistory } from 'vue-router'

// 导入页面组件
import LoginView from '@/views/login/LoginView.vue'
import MainView from '@/views/main/MainView.vue'
import RegisterView from '@/views/register/RegisterView.vue'
import AdminLoginView from '@/views/admin/login/AdminLoginView.vue'
import CalendarView from '@/views/main/calendar/CalendarView.vue'
import SettingView from '@/views/setting/SettingView.vue'
import MyUserView from '@/views/user/MyUserView.vue'
import AdminMainView from '@/views/admin/main/AdminMainView.vue'
import MainHomeView from '@/views/main/home/MainHomeView.vue'

// 新增导入隐私政策和用户协议页面
import PrivacyPolicy from '@/views/pages/PrivacyPolicy.vue'
import UserAgreement from '@/views/pages/UserAgreement.vue'

// 图标
import { Calendar, House } from 'lucide-vue-next'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      component: MainView,
      name: 'main',
      children: [
        { path: '/calendar', component: CalendarView, meta: { icon: Calendar, index: 0 } },
        { path: '/', component: MainHomeView, meta: { icon: House, index: 1 } },
        // { path: '/setting', component: SettingView },
      ]
    },
    { path: '/login', component: LoginView },
    { path: '/register', component: RegisterView },
    { path: '/admin', component: AdminMainView },
    { path: '/admin/login', component: AdminLoginView },
    // { path: '/user', component: MyUserView },

    // 新增隐私政策和用户协议路由
    { path: '/privacy-policy', component: PrivacyPolicy },
    { path: '/user-agreement', component: UserAgreement },
  ]
})

export default router
