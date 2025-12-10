import { AxiosError } from "axios";
import { http } from "../utils";

class AuthRequest {
    public async login(request: { username: string, password: string }) {
        const res = await http.post("/auth/login", request);
        return res.data;
    }
    public async logout() {
        try {
            const res = await http.post("/auth/logout");
            return res.status === 200;
        } catch (err) {
            console.error(err);
            return false;
        }
    }
    public async check() {
        try {
            const res = await http.get("/auth/check");
            return res.status == 200;
        }
        catch (err) {
            console.error(err);
        }
    }
    public async register(request: {
        userName: string;
        password: string;
        displayName?: string | null;
        campusId: string;
        phone: string;
        email?: string;
    }) {
        try {
            const res = await http.post("/user/register", request);

            // 拦截成功状态码
            if ([200, 201, 202, 204].includes(res.status)) {
                return res.data; // 返回用户对象
            }

            // 非成功状态码，抛出错误
            throw new Error(`Unexpected status code: ${res.status}`);
        } catch (err: unknown) {
            console.error("注册接口调用失败：", err);

            const error = err as AxiosError<{ status?: number; title?: string }>;

            // 拦截 409 Conflict
            if (error.response?.status === 409) {
                return {
                    status: error.response.data?.status,
                    title: error.response.data?.title,
            };
        }

            // 其他错误直接抛出
            throw err;
        }
    }
    public async registerAdmin(request: {
        userName: string, password: string, displayName?: string | null, campusId: string, phone: string, email?: string
    }) {
        try {
            const res = await http.post("/user/register", request);

            // 拦截成功状态码
            if ([200, 201, 202, 204].includes(res.status)) {
                return res.data; // 返回用户对象
            }

            // 非成功状态码，抛出错误
            throw new Error(`Unexpected status code: ${res.status}`);
        } catch (err: unknown) {
            console.error("注册接口调用失败：", err);

            const error = err as AxiosError<{ status?: number; title?: string }>;

            // 拦截 409 Conflict
            if (error.response?.status === 409) {
                return {
                    status: error.response.data?.status,
                    title: error.response.data?.title,
            };
        }

            // 其他错误直接抛出
            throw err;
        }
    }
}

const authRequest = new AuthRequest();

export { authRequest };