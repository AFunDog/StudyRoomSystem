package com.zyx.studyroomsystem.service.impl;

import com.zyx.studyroomsystem.mapper.RoomMapper;
import com.zyx.studyroomsystem.pojo.Room;
import com.zyx.studyroomsystem.service.RoomService;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.UUID;

@Service
public class RoomServiceImpl implements RoomService {

    private final RoomMapper roomMapper;

    // 构造注入
    public RoomServiceImpl(RoomMapper roomMapper) {
        this.roomMapper = roomMapper;
    }

    @Override
    public Room getRoomById(UUID id) {
        return roomMapper.selectRoomById(id);
    }

    @Override
    public List<Room> getAllRooms() {
        return roomMapper.selectAllRooms();
    }

    @Override
    public void addRoom(Room room) {
        roomMapper.insertRoom(room);
    }

    @Override
    public void updateRoom(Room room) {
        roomMapper.updateRoom(room);
    }

    @Override
    public boolean deleteRoom(UUID id) {
        return roomMapper.deleteRoom(id) > 0; // MyBatis 返回受影响行数
    }

    @Override
    public boolean existsByName(String name) {
        return roomMapper.existsByName(name);
    }
}
