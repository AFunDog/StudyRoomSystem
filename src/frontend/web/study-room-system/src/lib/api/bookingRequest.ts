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
            if(err instanceof AxiosError){
                return err.response?.data as { message: string };
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
}

const bookingRequest = new BookingRequest();

export { bookingRequest };