package com.zyx.studyroomsystem.controller;

import com.zyx.studyroomsystem.exception.InvalidRequestException;
import com.zyx.studyroomsystem.exception.ResourceNotFoundException;
import com.zyx.studyroomsystem.pojo.Booking;
import com.zyx.studyroomsystem.service.BookingService;
import com.zyx.studyroomsystem.web.ApiResponse;
import jakarta.validation.Valid;
import jakarta.validation.constraints.NotNull;
import org.springframework.web.bind.annotation.*;

import java.time.OffsetDateTime;
import java.util.List;
import java.util.Map;
import java.util.UUID;

@RestController
@RequestMapping("/api/v1/booking")
public class BookingController {

    private final BookingService bookingService;

    public BookingController(BookingService bookingService) {
        this.bookingService = bookingService;
    }

    /** 获取单个预约 */
    @GetMapping("/{id}")
    public ApiResponse<Booking> get(@PathVariable @NotNull UUID id) {
        Booking booking = bookingService.getBookingById(id);
        if (booking == null) {
            throw new ResourceNotFoundException("预约未找到: `" + id);
        }
        return ApiResponse.ok(booking);
    }

    /** 获取用户的预约列表 */
    @GetMapping("/my")
    public ApiResponse<List<Booking>> my(@RequestParam @NotNull UUID userId) {
        return ApiResponse.ok(bookingService.getBookingsByUserId(userId));
    }

    /** 创建预约 */
    @PostMapping
    public ApiResponse<?> create(@Valid @RequestBody Booking booking) {
        // 校验时间冲突逻辑放在了 service 层
//        booking.setId(UUID.randomUUID()); id通过ulid设置
        booking.setCreateTime(OffsetDateTime.now());
        bookingService.addBooking(booking);
        return ApiResponse.ok(Map.of("id", booking.getId()));
    }

    /** 更新预约 */
    @PutMapping
    public ApiResponse<?> update(@Valid @RequestBody Booking booking) {
        bookingService.updateBooking(booking);
        return ApiResponse.ok(Map.of("updated", true));
    }

    /** 删除预约 */
    @DeleteMapping("/{id}")
    public ApiResponse<?> delete(@PathVariable @NotNull UUID id) {
        bookingService.deleteBooking(id);
        return ApiResponse.ok(Map.of("deleted", true));
    }

    /** 签到 */
    @PostMapping("/check-in")
    public ApiResponse<?> checkIn(@RequestBody Map<String, String> body) {
        String bookingId = body.get("bookingId");
        if (bookingId == null) {
            throw new InvalidRequestException("缺少预约ID");
        }
        UUID id = UUID.fromString(bookingId);
        Booking b = bookingService.getBookingById(id);
        if (b == null) {
            throw new ResourceNotFoundException("预约未找到: " + id);
        }
        OffsetDateTime now = OffsetDateTime.now();
        if (now.isBefore(b.getStartTime()) || now.isAfter(b.getEndTime())) {
            throw new InvalidRequestException("当前时间不在预约时间范围内");
        }
        b.setCheckInTime(now);
        b.setState("CHECKED_IN");
        bookingService.updateBooking(b);
        return ApiResponse.ok(Map.of("checkedIn", true));
    }

    /** 签退 */
    @PostMapping("/check-out")
    public ApiResponse<?> checkOut(@RequestBody Map<String, String> body) {
        String bookingId = body.get("bookingId");
        if (bookingId == null) {
            throw new InvalidRequestException("缺少预约ID");
        }
        UUID id = UUID.fromString(bookingId);
        Booking b = bookingService.getBookingById(id);
        if (b == null) {
            throw new ResourceNotFoundException("预约未找到: " + id);
        }
        b.setCheckOutTime(OffsetDateTime.now());
        b.setState("CHECKED_OUT");
        bookingService.updateBooking(b);
        return ApiResponse.ok(Map.of("checkedOut", true));
    }
}
