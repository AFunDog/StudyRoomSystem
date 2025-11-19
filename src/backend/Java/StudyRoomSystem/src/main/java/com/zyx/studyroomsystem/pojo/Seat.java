package com.zyx.studyroomsystem.pojo;

import com.zyx.studyroomsystem.web.UlidToUuidConverter;
import jakarta.persistence.Entity;
import jakarta.persistence.Id;
import jakarta.persistence.PrePersist;
import jakarta.validation.constraints.Min;
import jakarta.validation.constraints.NotNull;
import lombok.Data;

import java.util.UUID;

@Data
@Entity
public class Seat {
    @Id
    private UUID id;

    @PrePersist
    public void prePersist() {
        if (id == null) {
            this.id = UlidToUuidConverter.generateUuidFromUlid(); // 调用工具类生成 UUID
        }
    }

    @NotNull(message = "房间ID不能为空")
    private UUID roomId;

    @NotNull(message = "行号不能为空")
    @Min(value = 1, message = "行号必须大于等于 1")
    private Integer row;

    @NotNull(message = "列号不能为空")
    @Min(value = 1, message = "列号必须大于等于 1")
    private Integer col;
}
