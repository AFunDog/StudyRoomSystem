package com.zyx.studyroomsystem.mapper;

import com.zyx.studyroomsystem.pojo.Seat;
import org.apache.ibatis.annotations.*;

import java.util.List;
import java.util.UUID;

@Mapper
public interface SeatMapper {

    @Select("SELECT id, room_id, row, col FROM seats WHERE id = #{id}")
    Seat selectSeatById(UUID id);

    @Select("SELECT id, room_id, row, col FROM seats WHERE room_id = #{roomId}")
    List<Seat> selectSeatsByRoomId(UUID roomId);

    @Insert("INSERT INTO seats(id, room_id, row, col) VALUES(#{id,jdbcType=OTHER}, #{roomId}, #{row}, #{col})")
    void insertSeat(Seat seat);

    @Update("UPDATE seats SET room_id = #{roomId}, row = #{row}, col = #{col} WHERE id=#{id,jdbcType=OTHER}")
    void updateSeat(Seat seat);

    @Delete("DELETE FROM seats WHERE id=#{id,jdbcType=OTHER}")
    void deleteSeat(UUID id);
}
