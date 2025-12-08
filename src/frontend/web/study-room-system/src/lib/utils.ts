
import type { AxiosInstance } from "axios"
import axios from "axios"
import type { ClassValue } from "clsx"
import { clsx } from "clsx"
import { twMerge } from "tailwind-merge"
import { toast } from 'vue-sonner';
import { authRequest } from "@/lib/api/authRequest";
import router from "@/router"; // 全局路由实例


function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

const http: AxiosInstance = axios.create({
  timeout: 5000,
  headers: { "Content-Type": "application/json" },
  baseURL: `/api/v1`,
  withCredentials: true, // 让请求自动带上 HttpOnly Cookie
});

// 请求拦截器
http.interceptors.request.use(config => {
  // TODO: 可以在这里添加日志、公共 header、Loading 状态
  return config;
});

// 响应拦截器
http.interceptors.response.use(
  res => res,
  err => {
    if (err.response?.status === 401) {
      toast.error("登录已过期，请重新登录");

      // 从后端响应里取角色信息
      const role = err.response?.data?.role;

      if (role === "Admin") {
        router.push("/admin/login");
      } else {
        router.push("/login");
      }
    }
    return Promise.reject(err);
  }
);

// 封装登出逻辑
async function logout() {
  try {
    const ok = await authRequest.logout(); // 调用统一的接口封装
    if (ok) {
      toast.success("已登出");
    } else {
      toast.error("登出失败", { description: "请检查网络或稍后再试" });
    }
  } catch (e) {
    console.error(e);
    toast.error("登出失败", { description: "请检查网络或稍后再试" });
  }
  // 清理前端状态（非敏感信息）
  // store.user = null;
}

export { http, cn,logout };