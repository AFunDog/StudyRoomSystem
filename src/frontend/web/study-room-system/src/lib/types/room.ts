import type { Seat } from "./seat";

export interface Room {
  id: string;
  name: string;
  openTime : string;
  closeTime : string;
  rows: number;
  cols: number;
  seats: Seat[] | null;
}