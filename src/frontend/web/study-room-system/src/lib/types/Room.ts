import type { Seat } from "./Seat";

export interface Room {
  id: string;
  name: string;
  openTime : string;
  closeTime : string;
  rows: number;
  cols: number;
  seats: Seat[] | null;
}

export interface RoomEdit {
  id: string;
  name: string;
  rows: number;
  cols: number;
  openHour: number;
  openMin: number;
  closeHour: number;
  closeMin: number;
}
