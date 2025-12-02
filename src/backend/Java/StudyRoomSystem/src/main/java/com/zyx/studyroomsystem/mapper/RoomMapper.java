package com.zyx.studyroomsystem.mapper;

import com.zyx.studyroomsystem.pojo.Room;
import org.apache.ibatis.annotations.*;

import java.util.List;
import java.util.UUID;

@Mapper
public interface RoomMapper {

    @Select("SELECT * FROM rooms WHERE id = #{id}")
    Room selectRoomById(UUID id);

    /** 新增：根据房间名查询房间 */
    @Select("SELECT * FROM rooms WHERE name = #{name} LIMIT 1")
    Room selectRoomByName(String name);

    @Select("SELECT * FROM rooms")
    List<Room> selectAllRooms();

    @Insert("INSERT INTO rooms(id, name, open_time, close_time, rows, cols) " +
            "VALUES(#{id,jdbcType=OTHER}, #{name}, #{openTime}, #{closeTime}, #{rows}, #{cols})")
    void insertRoom(Room room);

    @Update("UPDATE rooms SET name = #{name}, open_time = #{openTime}, close_time = #{closeTime}, rows = #{rows}, cols = #{cols} WHERE id=#{id,jdbcType=OTHER}")
    void updateRoom(Room room);

    @Delete("DELETE FROM rooms WHERE id=#{id,jdbcType=OTHER}")
    int deleteRoom(UUID id);  // 返回受影响的行数

    /** 新增：判断房间名是否存在 */
    @Select("SELECT COUNT(1) > 0 FROM rooms WHERE name = #{name}")
    boolean existsByName(String name);
}
