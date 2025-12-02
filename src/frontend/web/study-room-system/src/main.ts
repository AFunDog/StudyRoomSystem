import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import { useColorMode } from '@vueuse/core'
import { http } from './lib/utils'
import utc from 'dayjs/plugin/utc'
import dayjs from 'dayjs'
import { restartHubConnection } from './lib/api/hubConnection'

// 导入路由
import router from './router'

dayjs.extend(utc)

// 颜色模式
const color = useColorMode()
console.log(color.value)

// 启动 SignalR / Hub 连接
restartHubConnection()

// 创建并挂载应用
createApp(App).use(router).mount('#app')
