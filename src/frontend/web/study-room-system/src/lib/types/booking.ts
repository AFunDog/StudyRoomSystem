import type { Seat } from "./seat";
import type { User } from "./user";


export interface Booking {
    id: string;
    userId: string;
    seatId: string;
    createTime: string;
    startTime: string;
    endTime: string;
    checkInTime: string | null;
    checkOutTime: string | null;
    state : "Booking" | "CheckIn" | "Checkout" | "Canceled";
    user : User | null;
    seat : Seat | null;
}