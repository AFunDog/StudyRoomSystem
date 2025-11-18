package com.zyx.studyroomsystem.controller;

import com.zyx.studyroomsystem.exception.AuthenticationFailedException;
import com.zyx.studyroomsystem.exception.UserAlreadyExistsException;
import com.zyx.studyroomsystem.pojo.User;
import com.zyx.studyroomsystem.security.JwtUtil;
import com.zyx.studyroomsystem.security.SecurityUser;
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
    private final JwtUtil jwtUtil;

    public AuthController(UserService userService, PasswordEncoder passwordEncoder, JwtUtil jwtUtil) {
        this.userService = userService;
        this.passwordEncoder = passwordEncoder;
        this.jwtUtil = jwtUtil;
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
        if (user == null) {
            throw new AuthenticationFailedException("用户不存在: " + userName);
        }
        if (!passwordEncoder.matches(password, user.getPassword())) {
            throw new AuthenticationFailedException("密码错误");
        }

        // 使用 JwtUtil 生成 JWT
        String jwtToken = jwtUtil.generateToken(new SecurityUser(user));

        return ApiResponse.ok(Map.of(
                "token", jwtToken,
                "userId", user.getId(),
                "role", user.getRole()
        ));
    }

    @GetMapping("/check")
    public ApiResponse<?> check() {
        return ApiResponse.ok(Map.of("authenticated", true));
    }
}
