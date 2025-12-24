import type { Booking } from "./Booking";
import type { User } from "./User";

export type ViolationState = "Violation";
export type ViolationType = "超时" | "强制取消" | "管理员";

export interface Violation {
  id: string;
  bookingId: string | null;
  userId: string;
  createTime: string;
  state: ViolationState;
  type: ViolationType;
  content: string;
  user: User;
  booking: Booking | null;
}

export interface ViolationPage {
  total: number;
  page: number;
  pageSize: number;
  items: Violation[];
}

export interface ViolationCreateDto { 
  userId: string; 
  bookingId?: string | null; 
  type: "超时" | "强制取消" | "管理员"; 
  content: string; 
} 

export interface ViolationUpdateDto { 
  id: string; 
  type?: "超时" | "强制取消" | "管理员" | null; 
  content?: string | null; 
}

export function localizeViolationType(type: ViolationType): string {
  switch (type) {
    case "超时":
      return "超时";
    case "强制取消":
      return "强制取消";
    case "管理员":
      return "管理员处理";
    default:
      return type; // 兜底
  }
}
