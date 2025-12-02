package com.zyx.studyroomsystem.controller;

import com.zyx.studyroomsystem.exception.AuthenticationFailedException;
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

    @PostMapping("/register")
    public ApiResponse<?> register(@Valid @RequestBody RegisterDto dto) {
        User u = userService.registerUser(dto, "USER");
        return ApiResponse.ok(Map.of("id", u.getId()));
    }

    @PostMapping("/registerAdmin")
    public ApiResponse<?> registerAdmin(@Valid @RequestBody RegisterDto dto) {
        User u = userService.registerUser(dto, "ADMIN");
        return ApiResponse.ok(Map.of("id", u.getId()));
    }

    @PostMapping("/login")
    public ApiResponse<?> login(@RequestBody Map<String,String> body) {
        String userName = body.get("username");
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
