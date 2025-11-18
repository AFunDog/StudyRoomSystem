package com.zyx.studyroomsystem.service.impl;

import com.zyx.studyroomsystem.mapper.SeatMapper;
import com.zyx.studyroomsystem.pojo.Seat;
import com.zyx.studyroomsystem.service.SeatService;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.UUID;

@Service
public class SeatServiceImpl implements SeatService {

    private final SeatMapper seatMapper;

    // 构造注入
    public SeatServiceImpl(SeatMapper seatMapper) {
        this.seatMapper = seatMapper;
    }

    @Override
    public Seat getSeatById(UUID id) {
        return seatMapper.selectSeatById(id);
    }

    @Override
    public List<Seat> getSeatsByRoomId(UUID roomId) {
        return seatMapper.selectSeatsByRoomId(roomId);
    }

    @Override
    public void addSeat(Seat seat) {
        seatMapper.insertSeat(seat);
    }

    @Override
    public void updateSeat(Seat seat) {
        seatMapper.updateSeat(seat);
    }

    @Override
    public void deleteSeat(UUID id) {
        seatMapper.deleteSeat(id);
    }
}
