package com.zyx.studyroomsystem.pojo;

import lombok.Data;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Future;
import jakarta.validation.constraints.Size;
import jakarta.validation.constraints.Pattern;

import java.time.OffsetDateTime;
import java.util.UUID;

@Data
public class Booking {
    @NotNull(message = "预订ID不能为空")
    private UUID id;

    @NotNull(message = "用户ID不能为空")
    private UUID userId;

    @NotNull(message = "座位ID不能为空")
    private UUID seatId;

    @NotNull(message = "创建时间不能为空")
    private OffsetDateTime createTime;

    @NotNull(message = "开始时间不能为空")
    @Future(message = "开始时间必须是未来时间")
    private OffsetDateTime startTime;

    @NotNull(message = "结束时间不能为空")
    @Future(message = "结束时间必须是未来时间")
    private OffsetDateTime endTime;

    private OffsetDateTime checkInTime;

    private OffsetDateTime checkOutTime;

    @NotNull(message = "状态不能为空")
    @Pattern(regexp = "^(PENDING|CONFIRMED|CANCELLED|CHECKED_IN|CHECKED_OUT)$",
            message = "状态只能是 PENDING、CONFIRMED、CANCELLED、CHECKED_IN 或 CHECKED_OUT")
    @Size(max = 32, message = "状态长度不能超过 32")
    private String state;
}
