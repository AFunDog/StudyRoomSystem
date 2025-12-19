import { http } from "../utils";
import type { Booking } from "../types/Booking";
import { AxiosError } from "axios";

// 异常交由调用者处理,此处不要吞并err
class BookingRequest {
    public async getMyBookings(params?: { page?: number; pageSize?: number }) {
        try {
            const res = await http.get("/booking/my", { params });
            return res.data as Booking[];
        } catch (err) {
            console.error(err);
            throw err; 
        }
  }

    public async cancelBooking(id: string,isForce: boolean) {
        const res = await http.delete(`/booking/${id}?isForce=${isForce}`);
        return res.data as { message?: string };
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