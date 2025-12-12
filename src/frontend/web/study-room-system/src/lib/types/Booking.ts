import type { Seat } from "./Seat";
import type { User } from "./User";

type BookingState = "Booked" | "CheckIn" | "Checkout" | "Canceled";
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
    switch (state) {
        case "Booked":
            return "已预约";
        case "CheckIn":
            return "已签到";
        case "Checkout":
            return "已签退";
        case "Canceled":
            return "已取消";
    }
}