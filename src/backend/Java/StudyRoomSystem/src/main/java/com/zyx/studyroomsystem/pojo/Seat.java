package com.zyx.studyroomsystem.pojo;

import lombok.Data;
import jakarta.validation.constraints.Min;
import jakarta.validation.constraints.NotNull;

import java.util.UUID;

@Data
public class Seat {
    @NotNull(message = "座位ID不能为空")
    private UUID id;

    @NotNull(message = "房间ID不能为空")
    private UUID roomId;

    @NotNull(message = "行号不能为空")
    @Min(value = 1, message = "行号必须大于等于 1")
    private Integer row;

    @NotNull(message = "列号不能为空")
    @Min(value = 1, message = "列号必须大于等于 1")
    private Integer col;
}
