import type { Room } from "./Room";

export interface Seat {
    id : string;
    row : number;
    col : number;

    room : Room | null;
}