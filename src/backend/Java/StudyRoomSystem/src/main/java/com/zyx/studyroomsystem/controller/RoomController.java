package com.zyx.studyroomsystem.controller;

import com.zyx.studyroomsystem.exception.ResourceConflictException;
import com.zyx.studyroomsystem.exception.ResourceNotFoundException;
import com.zyx.studyroomsystem.pojo.Room;
import com.zyx.studyroomsystem.service.RoomService;
import com.zyx.studyroomsystem.web.ApiResponse;
import jakarta.validation.Valid;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.Map;
import java.util.UUID;

@RestController
@RequestMapping("/api/v1/room")
public class RoomController {
    private final RoomService roomService;

    public RoomController(RoomService roomService) {
        this.roomService = roomService;
    }

    /** 获取所有房间 */
    @GetMapping
    public ApiResponse<List<Room>> list() {
        return ApiResponse.ok(roomService.getAllRooms());
    }

    /** 创建房间 */
    @PostMapping
    public ApiResponse<?> create(@Valid @RequestBody Room room) {
        // 检查房间是否已存在（假设通过名称判断）
        if (roomService.existsByName(room.getName())) {
            throw new ResourceConflictException("房间已存在: " + room.getName());
        }
//        room.setId(UUID.randomUUID()); id通过ulid设置
        roomService.addRoom(room);
        return ApiResponse.ok(Map.of("id", room.getId()));
    }

    /** 根据 ID 获取房间 */
    @GetMapping("/{id}")
    public ApiResponse<Room> get(@PathVariable UUID id) {
        Room room = roomService.getRoomById(id);
        if (room == null) {
            throw new ResourceNotFoundException("房间不存在: " + id);
        }
        return ApiResponse.ok(room);
    }

    /** 删除房间 */
    @DeleteMapping("/{id}")
    public ApiResponse<?> delete(@PathVariable UUID id) {
        boolean deleted = roomService.deleteRoom(id);
        if (!deleted) {
            throw new ResourceNotFoundException("房间不存在: " + id);
        }
        return ApiResponse.ok(Map.of("deleted", true));
    }
}
