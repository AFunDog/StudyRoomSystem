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
        userName: string, password: string, displayName?: string | null, campusId: string, phone: string, email?: string
    }) {
        try {
            const res = await http.post("/user/register", request);
            return res.data;
        }
        catch (err) {
            console.error(err);
        }
    }
    public async registerAdmin(request: {
        userName: string, password: string, displayName?: string | null, campusId: string, phone: string, email?: string
    }) {
        try {
            const res = await http.post("/user/registerAdmin", request);
            return res.data;
        }
        catch (err) {
            console.error(err);
        }
    }
}

const authRequest = new AuthRequest();

export { authRequest };