package com.zyx.studyroomsystem.controller;

import com.zyx.studyroomsystem.exception.ResourceConflictException;
import com.zyx.studyroomsystem.exception.ResourceNotFoundException;
import com.zyx.studyroomsystem.pojo.User;
import com.zyx.studyroomsystem.service.UserService;
import com.zyx.studyroomsystem.web.ApiResponse;
import com.zyx.studyroomsystem.web.UlidToUuidConverter;
import jakarta.validation.Valid;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.Map;
import java.util.UUID;

@RestController
@RequestMapping("/api/v1/user")
public class UserController {
    private final UserService userService;
    public UserController(UserService userService){ this.userService = userService; }

    /** 根据 ID 获取用户 */
    @GetMapping("/{id}")
    public ApiResponse<User> get(@PathVariable UUID id){
        User user = userService.getUserById(id);
        if (user == null) {
            throw new ResourceNotFoundException("用户不存在: " + id);
        }
        return ApiResponse.ok(user);
    }

    /** 获取所有用户 */
    @GetMapping
    public ApiResponse<List<User>> list(){
        List<User> users = userService.getAllUsers();
        if (users == null || users.isEmpty()) {
            throw new ResourceNotFoundException("没有用户数据");
        }
        return ApiResponse.ok(users);
    }

    /** 创建用户 */
    @PostMapping
    public ApiResponse<?> create(@Valid @RequestBody User user){
        if (userService.getUserByUserName(user.getUserName()) != null) {
            throw new ResourceConflictException("用户名已存在: " + user.getUserName());
        }
        // 手动生成 ULID → UUID
        user.setId(UlidToUuidConverter.generateUuidFromUlid());
        user.setCreateTime(java.time.OffsetDateTime.now());
        userService.addUser(user);
        return ApiResponse.ok(Map.of("id", user.getId()));
    }

    /** 更新用户 */
    @PutMapping("/{id}")
    public ApiResponse<?> update(@PathVariable UUID id, @Valid @RequestBody User user){
        User existing = userService.getUserById(id);
        if (existing == null) {
            throw new ResourceNotFoundException("用户不存在: " + id);
        }
        user.setId(id); // 保持 ID 一致
        userService.updateUser(user);
        return ApiResponse.ok(Map.of("updated", true));
    }

    /** 删除用户 */
    @DeleteMapping("/{id}")
    public ApiResponse<?> delete(@PathVariable UUID id){
        User existing = userService.getUserById(id);
        if (existing == null) {
            throw new ResourceNotFoundException("用户不存在: " + id);
        }
        userService.deleteUser(id);
        return ApiResponse.ok(Map.of("deleted", true));
    }
}
