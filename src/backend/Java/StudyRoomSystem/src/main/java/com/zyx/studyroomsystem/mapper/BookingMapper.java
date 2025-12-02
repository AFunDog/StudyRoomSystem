package com.zyx.studyroomsystem.mapper;

import com.zyx.studyroomsystem.pojo.Booking;
import org.apache.ibatis.annotations.*;

import java.util.List;
import java.util.UUID;

@Mapper
public interface BookingMapper {

    @Select("SELECT id, user_id, seat_id, create_time, start_time, end_time, check_in_time, check_out_time, state " +
            "FROM bookings WHERE id = #{id}")
    Booking selectBookingById(UUID id);

    @Select("SELECT id, user_id, seat_id, create_time, start_time, end_time, check_in_time, check_out_time, state " +
            "FROM bookings WHERE user_id = #{userId}")
    List<Booking> selectBookingsByUserId(UUID userId);

    @Select("SELECT id, user_id, seat_id, create_time, start_time, end_time, check_in_time, check_out_time, state " +
            "FROM bookings WHERE seat_id = #{seatId}")
    List<Booking> selectBookingsBySeatId(UUID seatId);

    @Insert("INSERT INTO bookings(id, user_id, seat_id, create_time, start_time, end_time, check_in_time, check_out_time, state) " +
            "VALUES(#{id,jdbcType=OTHER}, #{userId}, #{seatId}, #{createTime}, #{startTime}, #{endTime}, #{checkInTime}, #{checkOutTime}, #{state})")
    void insertBooking(Booking booking);

    @Update("UPDATE bookings SET user_id = #{userId}, seat_id = #{seatId}, start_time = #{startTime}, end_time = #{endTime}, " +
            "check_in_time = #{checkInTime}, check_out_time = #{checkOutTime}, state = #{state} WHERE id=#{id,jdbcType=OTHER}")
    void updateBooking(Booking booking);

    @Delete("DELETE FROM bookings WHERE id=#{id,jdbcType=OTHER}")
    void deleteBooking(UUID id);
}
