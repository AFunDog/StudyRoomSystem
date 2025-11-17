import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import { useColorMode } from '@vueuse/core'
import { createRouter, createWebHistory } from 'vue-router'
import LoginView from './views/login/LoginView.vue'
import MainView from './views/main/MainView.vue'
import { http } from './lib/Utils'
import utc from 'dayjs/plugin/utc'
import dayjs from 'dayjs'
import RegisterView from './views/register/RegisterView.vue'
import { restartHubConnection } from './lib/api/HubConnection'
import AdminLoginView from './views/admin/login/AdminLoginView.vue'
import CalendarView from './views/main/calendar/CalendarView.vue'
import SettingView from './views/setting/SettingView.vue'
import MyUserView from './views/user/MyUserView.vue'
import AdminMainView from './views/admin/main/AdminMainView.vue'
import MainHomeView from './views/main/home/MainHomeView.vue'
import { Calendar, House } from 'lucide-vue-next'

dayjs.extend(utc)

// 颜色模式
const color = useColorMode()
console.log(color.value)

// 路由
const router = createRouter({
    history: createWebHistory(), routes: [
        {
            path: '/', component: MainView, name: 'main', children: [
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

    ]
})

// router.beforeEach((to,from,next) =>{
//     const token = localStorage.getItem('token');

// });

restartHubConnection();


// 加载配置，并挂载
const app = createApp(App).use(router).mount('#app')