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
    user : User | null;
    seat : Seat | null;
}