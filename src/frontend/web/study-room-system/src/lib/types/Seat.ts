import type { Room } from "./Room";

export interface Seat {
    id : string;
    row : number;
    col : number;

    room : Room | null;
}

export interface SeatState {
    id: string | null;     // 可能还没保存
    row: number;
    col: number;
    open: boolean;         // 是否启用
}