
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
    const status = err.response?.status;
    const requestUrl = err.config?.url;

    if (status === 401) {
      // 如果是登录接口返回的401，说明是账号密码错误，不提示“登录过期”
      if (requestUrl?.includes("/auth/login")) {
        // 交给登录页的catch去处理，不在拦截器里提示
        return Promise.reject(err);
      }

      // 其他接口返回401，说明登录过期
      toast.error("登录已过期，请重新登录");

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