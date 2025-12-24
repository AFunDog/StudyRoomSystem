import { http } from "../utils";
import type { ViolationPage, Violation, ViolationCreateDto, ViolationUpdateDto } from "../types/Violation";

class ViolationRequest {

  /* -------------------------------------------------------
  获取我的违规记录（分页）
  ------------------------------------------------------- */
  public async getMyViolations(params?: { page?: number; pageSize?: number }) {
    const p = params ?? {};
    const res = await http.get("/violation/my", {
      params: {
        page: p.page ?? 1,
        pageSize: p.pageSize ?? 20,
      },
    });
    return res.data as ViolationPage;
  }

  /* -------------------------------------------------------
  管理员：获取所有违规记录（分页）
  ------------------------------------------------------- */
  public async getAllViolations(params?: { page?: number; pageSize?: number }) {
    const p = params ?? {};
    const res = await http.get("/violation/all", {
      params: {
        page: p.page ?? 1,
        pageSize: p.pageSize ?? 20,
      },
    });
    return res.data as ViolationPage;
  }

  /* -------------------------------------------------------
  获取指定违规记录
  ------------------------------------------------------- */
  public async getViolationById(id: string) {
    const res = await http.get(`/violation/${id}`);
    return res.data as Violation;
  }

  /* -------------------------------------------------------
  创建违规记录
  ------------------------------------------------------- */
  public async createViolation(data: ViolationCreateDto) {
    const res = await http.post("/violation", data);
    return res.data as Violation;
  }

  /* -------------------------------------------------------
  修改违规记录
  ------------------------------------------------------- */
  public async updateViolation(data: ViolationUpdateDto) {
    const res = await http.put("/violation", data);
    return res.data as Violation;
  }

  /* -------------------------------------------------------
  删除违规记录
  ------------------------------------------------------- */
  public async deleteViolation(id: string) {
    await http.delete(`/violation/${id}`);
    return true;
  }
}

export const violationRequest = new ViolationRequest();
