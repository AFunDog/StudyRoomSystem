import { http } from "../utils";
import type { Booking } from "../types/Booking";
import { AxiosError } from "axios";

class BookingRequest {
    public async getMyBookings() {
        try {
            const res = await http.get("/booking/my");
            return res.data as Booking[];
        }
        catch (err) {
            console.error(err);
        }
        return [];
    }

    public async cancelBooking(id: string,isForce: boolean) {
        try {
            const res = await http.delete(`/booking/${id}?isForce=${isForce}`);
            return { message : "预约已取消" };
        }
        catch (err) {
            console.error(err);
            // if(err instanceof AxiosError){
            //     return err.response?.data as { message: string };
            // }
            //修改错误信息返回策略,返回错误中的title作为message
            if (err instanceof AxiosError) {
                const data = err.response?.data as any

                if (data && typeof data.title === "string") {
                    return { message: data.title };
                }
                
                if (data && typeof data.message === "string") {
                    return { message: data.message };
                }
            }
        }
        return { message : "取消预约失败"  };
    }
    public async createBooking(request: { seatId: string, startTime: string, endTime: string }) {
        try {
            const res = await http.post("/booking", request);
            return res.data as Booking;
        }
        catch (err) {
            console.error(err);
        }
        return null;
    }
    public async checkIn(request: { id: string }) {
        // 异常交由调用者处理
        const res = await http.post("/booking/check-in", request);
        return res.data as Booking;
    }
    public async checkOut(request: { id: string }) {
        const res = await http.post("/booking/check-out", request);
        return res.data as Booking;
    }
}

const bookingRequest = new BookingRequest();

export { bookingRequest };