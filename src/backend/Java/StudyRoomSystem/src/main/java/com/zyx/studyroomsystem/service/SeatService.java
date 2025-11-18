package com.zyx.studyroomsystem.service;

import com.zyx.studyroomsystem.pojo.Seat;

import java.util.List;
import java.util.UUID;

public interface SeatService {

    /**
     * 根据座位ID查询座位
     */
    Seat getSeatById(UUID id);

    /**
     * 根据房间ID查询该房间所有座位
     */
    List<Seat> getSeatsByRoomId(UUID roomId);

    /**
     * 新增座位
     */
    void addSeat(Seat seat);

    /**
     * 更新座位信息
     */
    void updateSeat(Seat seat);

    /**
     * 删除座位
     */
    void deleteSeat(UUID id);
}
