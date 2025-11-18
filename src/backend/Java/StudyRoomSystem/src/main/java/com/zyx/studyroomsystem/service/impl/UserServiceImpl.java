package com.zyx.studyroomsystem.service.impl;

import com.zyx.studyroomsystem.mapper.UserMapper;
import com.zyx.studyroomsystem.pojo.User;
import com.zyx.studyroomsystem.service.UserService;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.UUID;

@Service
public class UserServiceImpl implements UserService {

    private final UserMapper userMapper;
    private final PasswordEncoder passwordEncoder; // 注入配置类提供的 Bean

    // 构造注入
    public UserServiceImpl(UserMapper userMapper, PasswordEncoder passwordEncoder) {
        this.userMapper = userMapper;
        this.passwordEncoder = passwordEncoder;
    }

    @Override
    public User getUserById(UUID id) {
        return userMapper.selectUserById(id);
    }

    @Override
    public User getUserByUserName(String username) {
        return userMapper.selectByUserName(username);
    }

    @Override
    public List<User> getAllUsers() {
        return userMapper.selectAllUsers();
    }

    @Override
    public void addUser(User user) {
        userMapper.insertUser(user);
    }

    @Override
    public void updateUser(User user) {
        userMapper.updateUser(user);
    }

    @Override
    public void deleteUser(UUID id) {
        userMapper.deleteUser(id);
    }

    @Override
    public boolean validateLogin(String userName, String rawPassword) {
        User user = userMapper.selectByUserName(userName);
        if (user == null) {
            return false;
        }
        // 用 encoder.matches() 校验原始密码和数据库里的哈希
        return passwordEncoder.matches(rawPassword, user.getPassword());
    }
}
