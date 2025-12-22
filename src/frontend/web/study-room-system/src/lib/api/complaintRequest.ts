import { http } from "../utils";
import type { ComplaintPage } from "../types/Complaint";
import type { Complaint } from "../types/Complaint";

class ComplaintRequest {
  // 获取我的投诉（分页）
  public async getMyComplaints(params?: { page?: number; pageSize?: number }) {
    const p = params ?? {};
    const res = await http.get("/complaint/my", {
      params: {
        page: p.page ?? 1,
        pageSize: p.pageSize ?? 20,
      },
    });
    return res.data as ComplaintPage;
  }

  // 创建投诉
  public async createComplaint(data: { seatId: string; type: string; content: string; targetTime?: string | null }) {
    const res = await http.post("/complaint", {
      seatId: data.seatId,
      type: data.type,
      content: data.content,
      targetTime: data.targetTime ?? null,
    });
    return res.data as Complaint;
  }

  // 修改投诉（仅普通用户使用）
  public async editComplaint(data: { id: string; type?: string; content?: string; targetTime?: string | null }) {
    const res = await http.put("/complaint", data);
    return res.data as Complaint;
  }
}

export const complaintRequest = new ComplaintRequest();
