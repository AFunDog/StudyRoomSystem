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