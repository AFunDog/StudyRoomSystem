import { http } from "../utils";

export const roomRequest = {
  // 获取所有房间信息（包含座位）
  getRooms: () => http.get("/room"),

  // 获取指定房间信息（包含座位布局）
  getRoom: (id: string) => http.get(`/room/${id}`),

  // 创建房间
  createRoom: (data: {
    name: string;
    openTime: string;   // 格式 HH:mm:ss
    closeTime: string;  // 格式 HH:mm:ss
    rows: number;
    cols: number;
  }) => http.post("/room", data),

  // 修改房间信息
  updateRoom: (data: {
    id: string;
    name?: string;
    openTime?: string;
    closeTime?: string;
    rows?: number;
    cols?: number;
  }) => http.put("/room", data),

  // 删除房间
  deleteRoom: (id: string) => http.delete(`/room/${id}`),
};
