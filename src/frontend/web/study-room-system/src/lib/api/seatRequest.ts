import { http } from "../utils";

export const seatRequest = {
  // 获取指定座位信息
  getSeat: (id: string) => http.get(`/seat/${id}`),

  // 创建座位
  createSeat: (data: { roomId: string; row: number; col: number }) =>
    http.post("/seat", data),

  // 修改座位信息
  updateSeat: (data: { id: string; roomId?: string; row?: number; col?: number }) =>
    http.put("/seat", data),
};
