package com.zyx.studyroomsystem.pojo;

import com.zyx.studyroomsystem.web.UlidToUuidConverter;
import jakarta.persistence.Entity;
import jakarta.persistence.Id;
import jakarta.persistence.PrePersist;
import jakarta.validation.constraints.*;
import lombok.Data;

import java.time.LocalTime;
import java.util.UUID;

@Data
@Entity
public class Room {
    @Id
    private UUID id;

    @PrePersist
    public void prePersist() {
        if (id == null) {
            this.id = UlidToUuidConverter.generateUuidFromUlid(); // 调用工具类生成 UUID
        }
    }

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
