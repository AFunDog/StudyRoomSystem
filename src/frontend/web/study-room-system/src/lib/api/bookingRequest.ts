import { http } from "../utils";
import type { Booking } from "../types/Booking";
import axios, { AxiosError } from "axios";

class BookingRequest {
    /** 统一错误格式化 */
    private formatError(err: unknown, defaultMsg: string): Error {
        console.error(err);

        if (axios.isAxiosError(err)) {
            const data = err.response?.data as any;

            if (data?.detail && typeof data.detail === "string") {
                return new Error(data.detail);
            }
            if (data?.title && typeof data.title === "string") {
                return new Error(data.title);
            }
            if (data?.message && typeof data.message === "string") {
                return new Error(data.message);
            }
        }

        return new Error(defaultMsg);
    }

    /** 获取我的预约（支持分页） */
    public async getMyBookings(params?: { page?: number; pageSize?: number }) {
        try {
            const res = await http.get("/booking/my", { params });
            return res.data as Booking[];
        } catch (err) {
            throw this.formatError(err, "获取我的预约失败");
        }
    }

    /** 管理员获取所有预约（分页 + 条件筛选） */
    public async getAllBookings(params?: {
        page?: number;
        pageSize?: number;
        roomId?: string;
        startTime?: string;
        endTime?: string;
        state?: "已预约" | "已签到" | "已签退" | "已取消" | "已超时";
    }) {
        try {
            const res = await http.get("/booking/all", { params });
            return res.data;
        } catch (err) {
            throw this.formatError(err, "获取所有预约失败");
        }
    }


    /** 获取指定预约 */
    public async getBookingById(id: string): Promise<Booking> {
        try {
            const res = await http.get(`/booking/${id}`);
            return res.data as Booking;
        } catch (err) {
            throw this.formatError(err, "获取预约详情失败");
        }
    }

    /** 创建预约 */
    public async createBooking(request: { seatId: string; startTime: string; endTime: string }): Promise<Booking> {
        try {
            const res = await http.post("/booking", request);
            return res.data as Booking;
        } catch (err) {
            throw this.formatError(err, "创建预约失败");
        }
    }

    /** 修改预约 */
    public async updateBooking(request: { id: string; startTime: string; endTime: string }): Promise<Booking> {
        try {
            const res = await http.put("/booking", request);
            return res.data as Booking;
        } catch (err) {
            throw this.formatError(err, "修改预约失败");
        }
    }

    /** 取消预约 */
    public async cancelBooking(id: string, isForce = false): Promise<{ message: string }> {
        try {
            const res = await http.delete(`/booking/${id}`, { params: { isForce } });
            return res.data as { message: string };
        } catch (err) {
            throw this.formatError(err, "取消预约失败");
        }
    }

    /** 签到 */
    public async checkIn(request: { id: string }): Promise<Booking> {
        try {
            const res = await http.post("/booking/check-in", request);
            return res.data as Booking;
        } catch (err) {
            throw this.formatError(err, "签到失败");
        }
    }

    /** 签退 */
    public async checkOut(request: { id: string }): Promise<Booking> {
        try {
            const res = await http.post("/booking/check-out", request);
            return res.data as Booking;
        } catch (err) {
            throw this.formatError(err, "签退失败");
        }
    }
}

export const bookingRequest = new BookingRequest();
