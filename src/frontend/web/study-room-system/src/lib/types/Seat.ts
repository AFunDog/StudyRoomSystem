import type { Room } from "./Room";
import type { Booking } from "./Booking";

export interface Seat {
    id : string;
    row : number;
    col : number;
    room : Room | null;
    bookings?: Booking[];
}

export interface SeatState {
    id: string | null;
    row: number;
    col: number;
    open: boolean;     // 是否创建
    booked: boolean;   // 是否被预约
}
