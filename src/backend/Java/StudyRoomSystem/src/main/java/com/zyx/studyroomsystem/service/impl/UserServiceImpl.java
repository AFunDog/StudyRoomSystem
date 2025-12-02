package com.zyx.studyroomsystem.service.impl;

import com.zyx.studyroomsystem.exception.UserAlreadyExistsException;
import com.zyx.studyroomsystem.mapper.UserMapper;
import com.zyx.studyroomsystem.pojo.User;
import com.zyx.studyroomsystem.service.UserService;
import com.zyx.studyroomsystem.web.RegisterDto;
import com.zyx.studyroomsystem.web.UlidToUuidConverter;
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

    @Override
    public User registerUser(RegisterDto dto, String role) {
        if (getUserByUserName(dto.userName()) != null) {
            throw new UserAlreadyExistsException(role + "已存在: " + dto.userName());
        }
        User u = new User();
        // 手动生成 ULID → UUID
        u.setId(UlidToUuidConverter.generateUuidFromUlid());
        u.setCreateTime(java.time.OffsetDateTime.now());
        u.setUserName(dto.userName());
        //检验是否含有昵称，若没有则保持和用户名一致
        u.setDisplayName(
                (dto.displayName() == null || dto.displayName().isBlank())
                        ? dto.userName()
                        : dto.displayName()
        );
        u.setCampusId(dto.campusId());
        u.setEmail(dto.email());
        u.setPhone(dto.phone());
        u.setPassword(passwordEncoder.encode(dto.password()));
        u.setRole(role);
        addUser(u); // 调用底层持久化方法
        return u;
    }
}
