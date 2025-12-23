import type { Seat } from "./Seat";
import type { User } from "./User";

export type ComplaintState = "无" | "已发起" | "已处理" | "已关闭";

export interface Complaint {
  id: string;
  sendUserId: string;
  seatId: string;
  state: ComplaintState;
  type: string;
  sendContent: string;
  targetTime: string | null;
  createTime: string;
  handleTime: string | null;
  handleUserId: string | null;
  handleContent: string | null;
  sendUser?: User | null;
  seat?: Seat | null;
  handleUser?: User | null;
}

export interface ComplaintPage {
  total: number;
  page: number;
  pageSize: number;
  items: Complaint[];
}

export function localizeComplaintState(state: ComplaintState) {
  return state;
}
