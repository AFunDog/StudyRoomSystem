package com.zyx.studyroomsystem.pojo;

import jakarta.validation.constraints.*;
import lombok.Data;

import java.time.LocalTime;
import java.util.UUID;

@Data
public class Room {

    private UUID id;

    @NotBlank(message = "房间名称不能为空")
    @Size(max = 64, message = "房间名称不能超过 64 个字符")
    private String name;

    @NotNull(message = "开放时间不能为空")
    private LocalTime openTime;

    @NotNull(message = "关闭时间不能为空")
    private LocalTime closeTime;

    @NotNull(message = "行数不能为空")
    @Min(value = 1, message = "房间行数必须大于等于 1")
    @Max(value = 50, message = "房间行数不能超过 50")
    private Integer rows;

    @NotNull(message = "列数不能为空")
    @Min(value = 1, message = "房间列数必须大于等于 1")
    @Max(value = 50, message = "房间列数不能超过 50")
    private Integer cols;
}
