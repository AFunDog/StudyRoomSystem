import type { Booking } from "./Booking";
import type { User } from "./User";

export type ViolationState = "Violation";
export type ViolationType = "超时" | "强制取消" | "管理员";

export interface Violation {
  id: string;
  bookingId: string;
  userId: string;
  createTime: string;
  state: ViolationState;
  type: ViolationType;
  content: string;
  user?: User | null;
  booking?: Booking | null;
}

export interface ViolationPage {
  total: number;
  page: number;
  pageSize: number;
  items: Violation[];
}

export function localizeViolationType(type: ViolationType) {
  switch (type) {
    case "超时":
      return "超时";
    case "强制取消":
      return "强制取消";
    case "管理员":
      return "管理员处理";
  }
}
