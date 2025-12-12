import { AxiosError } from "axios";
import { http } from "../utils";

class AuthRequest {
    public async login(request: { userName: string, password: string }) {
        const res = await http.post("/auth/login", request);
        console.log(res);
        return res;
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
}

const authRequest = new AuthRequest();

export { authRequest };