package com.zyx.studyroomsystem.service;

import com.zyx.studyroomsystem.pojo.User;
import com.zyx.studyroomsystem.web.RegisterDto;

import java.util.List;
import java.util.UUID;

public interface UserService {

    /**
     * 根据用户ID查询用户
     */
    User getUserById(UUID id);

    /**
     * 根据用户名查询用户（常用于登录）
     */
    User getUserByUserName(String userName);

    /**
     * 查询所有用户
     */
    List<User> getAllUsers();

    /**
     * 新增用户
     */
    void addUser(User user);

    /**
     * 更新用户信息
     */
    void updateUser(User user);

    /**
     * 删除用户
     */
    void deleteUser(UUID id);

    /**
     * 用户登录校验
     */
    boolean validateLogin(String userName, String password);

    /**
     * 用户注册校验
     */
    User registerUser(RegisterDto dto, String role);
}
