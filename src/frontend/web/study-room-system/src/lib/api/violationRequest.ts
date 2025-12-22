import { http } from "../utils";
import type { ViolationPage } from "../types/Violation";

class ViolationRequest {

  // 获取我的违规
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
}

export const violationRequest = new ViolationRequest();
