import { http } from "../utils";
import type { Booking } from "../types/Booking";
import { AxiosError } from "axios";

class BookingRequest {
    private handleError(err: unknown, defaultMsg: string) {
        console.error(err);
        if (err instanceof AxiosError) {
            const data = err.response?.data as any;
            if (data && typeof data.title === "string") {
                return { message: data.title };
            }
            if (data && typeof data.message === "string") {
                return { message: data.message };
            }
        }
        return { message: defaultMsg };
    }

    /** 获取我的预约 */
    public async getMyBookings(): Promise<Booking[]> {
        try {
            const res = await http.get("/booking/my");
            return res.data as Booking[];
        } catch (err) {
            this.handleError(err, "获取我的预约失败");
            return [];
        }
    }

    /** 管理员获取所有预约（分页） */
    public async getAllBookings(page = 1, pageSize = 20) {
        try {
            const res = await http.get("/booking/all", { params: { page, pageSize } });
            return res.data;
        } catch (err) {
            this.handleError(err, "获取所有预约失败");
            return { total: 0, page, pageSize, items: [] };
        }
    }

    /** 获取指定预约 */
    public async getBookingById(id: string): Promise<Booking | null> {
        try {
            const res = await http.get(`/booking/${id}`);
            return res.data as Booking;
        } catch (err) {
            this.handleError(err, "获取预约详情失败");
            return null;
        }
    }

    /** 创建预约 */
    public async createBooking(request: { seatId: string; startTime: string; endTime: string }): Promise<Booking | null> {
        try {
            const res = await http.post("/booking", request);
            return res.data as Booking;
        } catch (err) {
            this.handleError(err, "创建预约失败");
            return null;
        }
    }

    /** 修改预约 */
    public async updateBooking(request: { id: string; startTime: string; endTime: string }): Promise<Booking | null> {
        try {
            const res = await http.put("/booking", request);
            return res.data as Booking;
        } catch (err) {
            this.handleError(err, "修改预约失败");
            return null;
        }
    }

    /** 取消预约 */
    public async cancelBooking(id: string, isForce = false): Promise<{ message: string }> {
        try {
            await http.delete(`/booking/${id}`, { params: { isForce } });
            return { message: "预约已取消" };
        } catch (err) {
            return this.handleError(err, "取消预约失败");
        }
    }

    /** 签到 */
    public async checkIn(request: { id: string }): Promise<Booking> {
        const res = await http.post("/booking/check-in", request);
        return res.data as Booking;
    }

    /** 签退 */
    public async checkOut(request: { id: string }): Promise<Booking> {
        const res = await http.post("/booking/check-out", request);
        return res.data as Booking;
    }
}

const bookingRequest = new BookingRequest();
export { bookingRequest };
