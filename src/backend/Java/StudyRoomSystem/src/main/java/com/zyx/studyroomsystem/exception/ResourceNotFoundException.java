package com.zyx.studyroomsystem.exception;

import java.io.Serial;

/**
 * ResourceNotFoundException 表示资源未找到异常。

 * 使用场景：
 * - 当请求的资源（如房间、座位、预订、用户等）不存在时抛出

 * 语义：
 * - 对应 HTTP 状态码 404 (Not Found)
 * - 在全局异常处理器中捕获后，可以返回统一的 ApiResponse
 */
public class ResourceNotFoundException extends RuntimeException {

    @Serial
    private static final long serialVersionUID = 1L;

    /**
     * 无参构造函数。
     */
    public ResourceNotFoundException() {
        super("资源未找到");
    }

    /**
     * 带消息的构造函数。
     * @param message 异常描述信息，例如 "房间不存在: 1234"
     */
    public ResourceNotFoundException(String message) {
        super(message);
    }

    /**
     * 带消息和原因的构造函数。
     * @param message 异常描述信息
     * @param cause   异常的根本原因
     */
    public ResourceNotFoundException(String message, Throwable cause) {
        super(message, cause);
    }

    /**
     * 仅带原因的构造函数。
     * @param cause 异常的根本原因
     */
    public ResourceNotFoundException(Throwable cause) {
        super(cause);
    }
}
