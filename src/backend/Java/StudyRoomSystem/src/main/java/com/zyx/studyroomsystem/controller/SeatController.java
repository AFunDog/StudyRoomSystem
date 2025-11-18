package com.zyx.studyroomsystem.controller;

import com.zyx.studyroomsystem.exception.ResourceNotFoundException;
import com.zyx.studyroomsystem.pojo.Seat;
import com.zyx.studyroomsystem.service.SeatService;
import com.zyx.studyroomsystem.web.ApiResponse;
import jakarta.validation.Valid;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.Map;
import java.util.UUID;

@RestController
@RequestMapping("/api/v1/seat")
public class SeatController {
    private final SeatService seatService;

    public SeatController(SeatService seatService) {
        this.seatService = seatService;
    }

    /** 根据房间ID获取所有座位 */
    @GetMapping("/room/{roomId}")
    public ApiResponse<List<Seat>> listByRoom(@PathVariable UUID roomId) {
        List<Seat> seats = seatService.getSeatsByRoomId(roomId);
        if (seats == null || seats.isEmpty()) {
            throw new ResourceNotFoundException("该房间没有座位: " + roomId);
        }
        return ApiResponse.ok(seats);
    }

    /** 创建座位 */
    @PostMapping
    public ApiResponse<?> create(@Valid @RequestBody Seat seat) {
        seat.setId(UUID.randomUUID());
        seatService.addSeat(seat);
        return ApiResponse.ok(Map.of("id", seat.getId()));
    }

    /** 根据座位ID获取座位 */
    @GetMapping("/{id}")
    public ApiResponse<Seat> get(@PathVariable UUID id) {
        Seat seat = seatService.getSeatById(id);
        if (seat == null) {
            throw new ResourceNotFoundException("座位不存在: " + id);
        }
        return ApiResponse.ok(seat);
    }

    /** 更新座位 */
    @PutMapping("/{id}")
    public ApiResponse<?> update(@PathVariable UUID id, @Valid @RequestBody Seat seat) {
        Seat existing = seatService.getSeatById(id);
        if (existing == null) {
            throw new ResourceNotFoundException("座位不存在: " + id);
        }
        seat.setId(id); // 保持 ID 一致
        seatService.updateSeat(seat);
        return ApiResponse.ok(Map.of("updated", true));
    }

    /** 删除座位 */
    @DeleteMapping("/{id}")
    public ApiResponse<?> delete(@PathVariable UUID id) {
        Seat existing = seatService.getSeatById(id);
        if (existing == null) {
            throw new ResourceNotFoundException("座位不存在: " + id);
        }
        seatService.deleteSeat(id);
        return ApiResponse.ok(Map.of("deleted", true));
    }
}
