import { http } from "../utils";
import type { ComplaintPage, Complaint } from "../types/Complaint";

class ComplaintRequest {
  // 获取所有投诉（管理员）
  public async getAllComplaints(params?: { page?: number; pageSize?: number }) {
    const p = params ?? {};
    const res = await http.get("/complaint", {
      params: {
        page: p.page ?? 1,
        pageSize: p.pageSize ?? 20,
      },
    });
    return res.data as ComplaintPage;
  }

  // 获取我的投诉（普通用户）
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

  // 修改投诉（普通用户）
  public async editComplaint(data: { id: string; type?: string; content?: string; targetTime?: string | null }) {
    const res = await http.put("/complaint", data);
    return res.data as Complaint;
  }

  // 管理员：处理投诉（添加违规 + 扣信用分）
  public async handleComplaint(data: {
    id: string;
    content: string;
    targetUserId: string;
    score: number;
    violationContent: string;
  }) {
    const res = await http.put("/complaint/handle", data);
    return res.data as Complaint;
  }

  // 管理员：忽略投诉
  public async ignoreComplaint(data: { id: string; content: string }) {
    const res = await http.put("/complaint/close", data);
    return res.data as Complaint;
  }

  // 删除投诉
  public async deleteComplaint(id: string) {
    const res = await http.delete("/complaint", {
      params: { id },
    });
    return res.data;
  }
}

export const complaintRequest = new ComplaintRequest();
