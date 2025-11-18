package com.zyx.studyroomsystem.pojo;

import lombok.Data;
import jakarta.validation.constraints.Email;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.Pattern;
import jakarta.validation.constraints.Size;
import jakarta.validation.constraints.NotNull;
import java.time.OffsetDateTime;
import java.util.UUID;

@Data
public class User {
    @NotNull(message = "用户ID不能为空")
    private UUID id;

    @NotNull(message = "创建时间不能为空")
    private OffsetDateTime createTime;

    @NotBlank(message = "用户名不能为空")
    @Size(min = 3, max = 32, message = "用户名长度必须在 3 到 32 之间")
    @Pattern(regexp = "^[a-zA-Z0-9_.-]+$", message = "用户名只能包含字母、数字、下划线、点和短横线")
    private String userName;

    @NotBlank(message = "昵称不能为空")
    @Size(max = 64, message = "昵称长度不能超过 64")
    private String displayName;

    @NotBlank(message = "密码不能为空")
    @Size(min = 8, max = 64, message = "密码长度必须在 8 到 64 之间")
    private String password;

    @Size(max = 32, message = "校园卡号长度不能超过 32")
    private String campusId;

    @Pattern(regexp = "^\\+?[0-9]{7,15}$", message = "手机号格式不正确")
    private String phone;

    @Email(message = "邮箱格式不正确")
    @Size(max = 128, message = "邮箱长度不能超过 128")
    private String email;

    @NotBlank(message = "角色不能为空")
    @Pattern(regexp = "^(USER|ADMIN)$", message = "角色只能是 USER 或 ADMIN")
    private String role;

    @Size(max = 256, message = "头像URL长度不能超过 256")
    private String avatar;
}
