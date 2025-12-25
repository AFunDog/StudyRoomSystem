// src/api/fileRequest.ts
import { http } from "../utils";

export const fileRequest = {
  // 上传文件（multipart/form-data）
  uploadFile: (file: File) => {
    const formData = new FormData();
    formData.append("file", file);
    return http.post("/file", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
        "x-api-version": "1.0"
      }
    });
  },

  // 获取文件（返回的是文件流）
  getFile: (id: string) => http.get(`/file/${id}`, { responseType: "blob" }),

  // 删除文件
  deleteFile: (id: string) => http.delete(`/file/${id}`),

  // 获取文件列表（分页）
  getFileList: (page = 1, pageSize = 20) =>
    http.get("/file/list", {
      params: { page, pageSize }
    })
};
