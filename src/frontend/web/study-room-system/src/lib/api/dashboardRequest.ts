import { http } from "../utils";
import type { AxiosResponse } from "axios";

export const dashboardRequest = {
  // 预约总数
  getBookingCount: async () => {
    const res: AxiosResponse<any> = await http.get("/booking/list");
    return res.data.total ?? res.data.length ?? 0;
  },

  // 投诉数量
  getComplaintCount: async () => {
    const res: AxiosResponse<any> = await http.get("/complaint/list");
    return res.data.total ?? res.data.length ?? 0;
  },

  // 违规数量
  getViolationCount: async () => {
    const res: AxiosResponse<any> = await http.get("/violation/list");
    return res.data.total ?? res.data.length ?? 0;
  },

  // 房间使用率 = 已启用座位数 / 总座位数
  getRoomUsage: async () => {
    const res: AxiosResponse<any> = await http.get("/room");
    const rooms = res.data;

    let totalSeats = 0;
    let usedSeats = 0;

    for (const room of rooms) {
      totalSeats += room.rows * room.cols;
      usedSeats += room.seats?.length || 0;
    }

    return totalSeats > 0 ? usedSeats / totalSeats : 0;
  },

  // 最近 7 天预约趋势
  getBookingTrend: async () => {
  const res: AxiosResponse<any> = await http.get("/booking/list");
  const bookings = res.data.items ?? res.data;

  const trend: number[] = [0, 0, 0, 0, 0, 0, 0];

  const list = bookings ?? [];

  list.forEach((b: any) => {
  if (!b || !b.startTime) return;

  const day = new Date(b.startTime).getDay(); // 0~6
  const index: number = day === 0 ? 6 : day - 1; // ⭐ 强制告诉 TS 这是 number

  trend[index]!!++; // ✔ TS 不再报错
});
  return trend;
}
};
