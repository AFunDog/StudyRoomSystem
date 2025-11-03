
import type { AxiosInstance } from "axios"
import axios from "axios"
import type { ClassValue } from "clsx"
import { clsx } from "clsx"
import { twMerge } from "tailwind-merge"
import { getCurrentInstance } from "vue"
import { HubConnectionBuilder } from "@microsoft/signalr";


const baseUrl = import.meta.env.DEV ? 'http://localhost:5106' : '';
function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

const http: AxiosInstance = axios.create({
  timeout: 5000,
  headers: { "Content-Type": "application/json" },
  baseURL: baseUrl,
});

// 添加请求拦截器
http.interceptors.request.use(config => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

function getHubConnection() {
  const hubConnection = new HubConnectionBuilder()
    .withUrl(`${baseUrl}/hub/data`)
    .withAutomaticReconnect()
    .build();
  return hubConnection;
}

export { http, cn, getHubConnection };