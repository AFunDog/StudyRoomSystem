package com.zyx.studyroomsystem.controller;

import com.zyx.studyroomsystem.exception.AuthenticationFailedException;
import com.zyx.studyroomsystem.exception.UserAlreadyExistsException;
import com.zyx.studyroomsystem.pojo.User;
import com.zyx.studyroomsystem.service.UserService;
import com.zyx.studyroomsystem.web.ApiResponse;
import com.zyx.studyroomsystem.web.RegisterDto;
import jakarta.validation.Valid;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.web.bind.annotation.*;

import java.util.Map;
import java.util.UUID;

@RestController
@RequestMapping("/api/v1/auth")
public class AuthController {
    private final UserService userService;
    private final PasswordEncoder passwordEncoder;

    public AuthController(UserService userService, PasswordEncoder passwordEncoder) {
        this.userService = userService;
        this.passwordEncoder = passwordEncoder;
    }

    /**
     * 私有方法：创建用户并保存
     * @param dto  注册信息
     * @param role 用户角色（USER / ADMIN）
     * @return 创建好的 User 对象
     */
    private User createUser(RegisterDto dto, String role) {
        if (userService.getUserByUserName(dto.userName()) != null) {
            throw new UserAlreadyExistsException(role + "已存在: " + dto.userName());
        }
        User u = new User();
        u.setId(UUID.randomUUID());
        u.setCreateTime(java.time.OffsetDateTime.now());
        u.setUserName(dto.userName());
        u.setDisplayName(dto.displayName());
        u.setEmail(dto.email());
        u.setPassword(passwordEncoder.encode(dto.password()));
        u.setRole(role);
        userService.addUser(u);
        return u;
    }

    @PostMapping("/register")
    public ApiResponse<?> register(@Valid @RequestBody RegisterDto dto) {
        User u = createUser(dto, "USER");
        return ApiResponse.ok(Map.of("id", u.getId()));
    }

    @PostMapping("/registerAdmin")
    public ApiResponse<?> registerAdmin(@Valid @RequestBody RegisterDto dto) {
        User u = createUser(dto, "ADMIN");
        return ApiResponse.ok(Map.of("id", u.getId()));
    }

    @PostMapping("/login")
    public ApiResponse<?> login(@RequestBody Map<String,String> body) {
        String userName = body.get("userName");
        String password = body.get("password");
        User user = userService.getUserByUserName(userName);
        if (user == null || !passwordEncoder.matches(password, user.getPassword())) {
            throw new AuthenticationFailedException("登陆失败");
        }
        // 简化：返回会话标识；后续可替换为 JWT
        String token = UUID.randomUUID().toString();
        return ApiResponse.ok(Map.of("token", token, "userId", user.getId(), "role", user.getRole()));
    }

    @GetMapping("/check")
    public ApiResponse<?> check() {
        return ApiResponse.ok(Map.of("authenticated", true));
    }
}
