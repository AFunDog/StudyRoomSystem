package com.zyx.studyroomsystem.web;

import jakarta.validation.constraints.Email;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.Pattern;
import jakarta.validation.constraints.Size;

/**
 * 用户注册参数 DTO，带有字段校验注解。
 */
public record RegisterDto(
        @NotBlank(message = "用户名不能为空")
        @Size(min = 3, max = 32, message = "用户名长度必须在 3 到 32 之间")
        @Pattern(regexp = "^[a-zA-Z0-9_.-]+$", message = "用户名只能包含字母、数字、下划线、点和短横线")
        String userName,

        //昵称可以为空
        @Size(max = 64, message = "昵称长度不能超过 64")
        String displayName,

        @NotBlank(message = "密码不能为空")
        @Size(min = 8, max = 64, message = "密码长度必须在 8 到 64 之间")
        String password,

        @Size(max = 32, message = "工号长度不能超过 32")
        String campusId,

        @Pattern(regexp = "^\\+?[0-9]{7,15}$", message = "手机号格式不正确")
        String phone,

        @Email(message = "邮箱格式不正确")
        @Size(max = 128, message = "邮箱长度不能超过 128")
        String email
){}
