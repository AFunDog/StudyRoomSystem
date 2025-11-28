import type { Room } from "./room";

export interface Seat {
    id : string;
    row : number;
    col : number;

    room : Room | null;
}