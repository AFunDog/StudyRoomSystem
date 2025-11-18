package com.zyx.studyroomsystem.service;

import com.zyx.studyroomsystem.pojo.Booking;

import java.util.List;
import java.util.UUID;

public interface BookingService {

    /**
     * 根据预约ID查询预约
     */
    Booking getBookingById(UUID id);

    /**
     * 根据用户ID查询该用户的所有预约
     */
    List<Booking> getBookingsByUserId(UUID userId);

    /**
     * 根据座位ID查询该座位的所有预约
     */
    List<Booking> getBookingsBySeatId(UUID seatId);

    /**
     * 新增预约
     */
    void addBooking(Booking booking);

    /**
     * 更新预约信息
     */
    void updateBooking(Booking booking);

    /**
     * 删除预约
     */
    void deleteBooking(UUID id);
}
