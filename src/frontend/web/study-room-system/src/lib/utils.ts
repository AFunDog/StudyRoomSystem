
import type { AxiosInstance } from "axios"
import axios from "axios"
import type { ClassValue } from "clsx"
import { clsx } from "clsx"
import { twMerge } from "tailwind-merge"
import { getCurrentInstance } from "vue"
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { toast } from 'vue-sonner';


function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

const http: AxiosInstance = axios.create({
  timeout: 5000,
  headers: { "Content-Type": "application/json" },
  baseURL: `/api/v1`,
  withCredentials: true, // 让请求自动带上 HttpOnly Cookie
});

// 添加请求拦截器
http.interceptors.request.use(config => {
  //TODO: 可以在这里添加日志、公共 header、Loading 状态。
  // 响应拦截器：统一处理错误，比如 401 时清理前端状态并跳转登录。
  return config;
});

async function logout() {
  try {
    await http.post('/auth/logout'); // 后端负责清除 HttpOnly Cookie
  } catch (e) {
    console.error(e);
    toast.error('登出失败', { description: '请检查网络或稍后再试' });
  }
  toast.success('已登出');
  // 清理前端状态（非敏感信息）
  // store.user = null;
}

export { http, cn,logout };