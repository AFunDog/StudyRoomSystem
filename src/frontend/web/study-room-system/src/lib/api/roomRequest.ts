import { http } from "../utils";
import { httpV2 } from "../utils";
import type { Room } from "../types/Room";

export interface RoomAvailabilityResponse {
  room: Room;
  seats?: string[] | null;   // 后端返回的可用座位 id 列表
}

export const roomRequest = {

  // V1 api
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

  //v2 api
  //获取房间-指定时间段下的座位信息
  getRoomWithTime: (params: { id: string; start: string; end: string }) =>
    httpV2.get<RoomAvailabilityResponse>(`/room/${params.id}`, {
      params: {
      start: params.start,
      end: params.end,
    },
  }),
};
