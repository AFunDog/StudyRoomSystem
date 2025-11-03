import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import { useColorMode } from '@vueuse/core'
import { createRouter, createWebHistory } from 'vue-router'
import LoginView from './views/login/LoginView.vue'
import MainView from './views/main/MainView.vue'
import { getHubConnection, http } from './lib/utils'
import utc from 'dayjs/plugin/utc'
import dayjs from 'dayjs'
import RegisterView from './views/register/RegisterView.vue'

dayjs.extend(utc)

// 颜色模式
const color = useColorMode()
console.log(color.value)

// 路由
const router = createRouter({
    history: createWebHistory(), routes: [
        { path: '/', component: MainView },
        { path: '/login', component: LoginView },
        { path: '/register', component: RegisterView },
    ]
})

// router.beforeEach((to,from,next) =>{
//     const token = localStorage.getItem('token');

// });


getHubConnection().start().catch(err => {
    console.log('Error while starting Hub connection: ' + err);
}).then(() => {
    console.log('Hub connection started');
})


// 加载配置，并挂载
const app = createApp(App).use(router).mount('#app')