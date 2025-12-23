import type { Seat } from "./Seat";
import type { User } from "./User";

type BookingState = "已预约" | "已签到" | "已签退" | "已取消" | "已超时";
export interface Booking {
    id: string;
    userId: string;
    seatId: string;
    createTime: string;
    startTime: string;
    endTime: string;
    checkInTime: string | null;
    checkOutTime: string | null;
    state : BookingState;
    user : User | null;
    seat : Seat | null;
}

/**
 * 翻译 Booking 状态
 * @param state 
 * @returns 
 */
export function localizeState(state : BookingState){
    return state;
}