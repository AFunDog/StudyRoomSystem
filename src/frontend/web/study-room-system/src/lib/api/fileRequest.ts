import { http } from "@/lib/utils";

class FileRequest {
  async upload(file: File): Promise<string> {
    const formData = new FormData();
    formData.append("file", file);
    const res: any = await http.post("/file", formData, {
      headers: { "Content-Type": "multipart/form-data" },
    });
    // 后端返回 { url: "/api/v1/file/{id.ext}" }
    return res.data?.url || res.url || "";
  }
}

export const fileRequest = new FileRequest();
