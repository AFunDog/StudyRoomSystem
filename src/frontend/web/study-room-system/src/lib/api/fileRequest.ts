import { http } from "@/lib/utils";

class FileRequest {
  // 上传文件，返回文件 URL
  async upload(file: File): Promise<string> {
    const formData = new FormData();
    formData.append("file", file);
    const res: any = await http.post("/file", formData, {
      headers: { "Content-Type": "multipart/form-data", "x-api-version": "1.0" },
    });
    return res.data?.url || res.url || "";
  }

  // 获取文件（返回 blob）
  async get(fileId: string): Promise<Blob> {
    const res = await http.get(`/file/${fileId}`, { responseType: "blob" });
    return res.data;
  }

  // 删除文件
  async delete(fileId: string): Promise<void> {
    await http.delete(`/file/${fileId}`);
  }

  // 获取文件列表（分页）
  async list(page = 1, pageSize = 20): Promise<{ items: any[]; total: number }> {
    const res = await http.get("/file/list", {
      params: { page, pageSize },
    });
    return res.data;
  }
}

export const fileRequest = new FileRequest();
