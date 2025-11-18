package com.zyx.studyroomsystem.service;

import com.zyx.studyroomsystem.pojo.Room;

import java.util.List;
import java.util.UUID;

public interface RoomService {

    /**
     * 根据房间ID查询房间
     */
    Room getRoomById(UUID id);

    /**
     * 查询所有房间
     */
    List<Room> getAllRooms();

    /**
     * 新增房间
     */
    void addRoom(Room room);

    /**
     * 更新房间信息
     */
    void updateRoom(Room room);

    /**
     * 删除房间
     */
    boolean deleteRoom(UUID id);

    /**
     * 根据房间名称判断房间是否存在
     */
    boolean existsByName(String name);
}
