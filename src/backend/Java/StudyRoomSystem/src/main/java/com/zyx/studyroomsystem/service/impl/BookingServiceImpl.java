package com.zyx.studyroomsystem.service.impl;

import com.zyx.studyroomsystem.exception.ResourceConflictException;
import com.zyx.studyroomsystem.mapper.BookingMapper;
import com.zyx.studyroomsystem.pojo.Booking;
import com.zyx.studyroomsystem.service.BookingService;
import org.springframework.stereotype.Service;

import java.time.Instant;
import java.time.OffsetDateTime;
import java.util.List;
import java.util.UUID;

@Service
public class BookingServiceImpl implements BookingService {

    private final BookingMapper bookingMapper;

    // 构造注入
    public BookingServiceImpl(BookingMapper bookingMapper) {
        this.bookingMapper = bookingMapper;
    }

    @Override
    public Booking getBookingById(UUID id) {
        return bookingMapper.selectBookingById(id);
    }

    @Override
    public List<Booking> getBookingsByUserId(UUID userId) {
        return bookingMapper.selectBookingsByUserId(userId);
    }

    @Override
    public List<Booking> getBookingsBySeatId(UUID seatId) {
        return bookingMapper.selectBookingsBySeatId(seatId);
    }

    @Override
    public void addBooking(Booking booking) {
        // 1. 校验时间合法性
        OffsetDateTime start = booking.getStartTime();
        OffsetDateTime end = booking.getEndTime();

        if (start == null || end == null || !end.isAfter(start)) {
            throw new ResourceConflictException("预约时间不合法：结束时间必须晚于开始时间");
        }

        // 转换成 Instant，方便比较（避免时区问题）
        Instant startInstant = start.toInstant();
        Instant endInstant = end.toInstant();

        // 2. 校验时间冲突：同一座位在同一时间段不能重复预约
        List<Booking> existing = bookingMapper.selectBookingsBySeatId(booking.getSeatId());
        for (Booking b : existing) {
            Instant existingStart = b.getStartTime().toInstant();
            Instant existingEnd = b.getEndTime().toInstant();

            boolean overlap = !(endInstant.isBefore(existingStart) || startInstant.isAfter(existingEnd));
            if (overlap) {
                throw new ResourceConflictException("该座位在所选时间段已被预约");
            }
        }

        // 3. 插入预约
        bookingMapper.insertBooking(booking);
    }

    @Override
    public void updateBooking(Booking booking) {
        bookingMapper.updateBooking(booking);
    }

    @Override
    public void deleteBooking(UUID id) {
        bookingMapper.deleteBooking(id);
    }
}
