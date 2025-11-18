package com.zyx.studyroomsystem.mapper;

import com.zyx.studyroomsystem.pojo.User;
import org.apache.ibatis.annotations.*;

import java.util.List;
import java.util.UUID;

@Mapper
public interface UserMapper {

    @Select("SELECT id, create_time, user_name, display_name, password, campus_id, phone, email, role, avatar " +
            "FROM users WHERE id = #{id}")
    User selectUserById(UUID id);

    @Select("SELECT id, create_time, user_name, display_name, password, campus_id, phone, email, role, avatar " +
            "FROM users WHERE user_name = #{userName}")
    User selectByUserName(String userName);

    @Select("SELECT id, create_time, user_name, display_name, password, campus_id, phone, email, role, avatar " +
            "FROM users")
    List<User> selectAllUsers();

    @Insert("INSERT INTO users(id, create_time, user_name, display_name, password, campus_id, phone, email, role, avatar) " +
            "VALUES(#{id}, #{createTime}, #{userName}, #{displayName}, #{password}, #{campusId}, #{phone}, #{email}, #{role}, #{avatar})")
    void insertUser(User user);

    @Update("UPDATE users SET display_name = #{displayName}, phone = #{phone}, email = #{email}, role = #{role}, avatar = #{avatar} " +
            "WHERE id=#{id}")
    void updateUser(User user);

    @Delete("DELETE FROM users WHERE id=#{id}")
    void deleteUser(UUID id);
}
