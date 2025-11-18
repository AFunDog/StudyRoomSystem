package com.zyx.studyroomsystem.exception;

import java.io.Serial;

/**
 * ResourceConflictException 表示资源冲突异常。

 * 使用场景：
 * - 当用户注册时，用户名或邮箱已存在
 * - 当尝试创建已存在的资源（如文件、记录）时

 * 语义：
 * - 对应 HTTP 状态码 409 (Conflict)
 * - 在全局异常处理器中捕获后，可以返回统一的 ApiResponse
 */
public class ResourceConflictException extends RuntimeException {

    /**
     * 序列化版本号，用于保证序列化/反序列化时的兼容性。
     * 使用 @Serial 注解明确标记这是序列化相关的字段。
     */
    @Serial
    private static final long serialVersionUID = 1L;

    /**
     * 无参构造函数。
     * 用于在不需要额外信息时抛出异常。
     */
    public ResourceConflictException() {
        super();
    }

    /**
     * 带消息的构造函数。
     * @param message 异常描述信息，例如 "用户已存在"
     */
    public ResourceConflictException(String message) {
        super(message);
    }

    /**
     * 带消息和原因的构造函数。
     * @param message 异常描述信息
     * @param cause   异常的根本原因（可用于异常链）
     */
    public ResourceConflictException(String message, Throwable cause) {
        super(message, cause);
    }

    /**
     * 仅带原因的构造函数。
     * @param cause 异常的根本原因
     */
    public ResourceConflictException(Throwable cause) {
        super(cause);
    }
}
