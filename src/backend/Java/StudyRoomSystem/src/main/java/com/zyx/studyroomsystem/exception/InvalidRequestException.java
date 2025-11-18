package com.zyx.studyroomsystem.exception;

import java.io.Serial;

/**
 * InvalidRequestException 表示请求参数不合法的异常。

 * 使用场景：
 * - 当上传文件缺少文件名时
 * - 当请求参数为空或格式错误时
 * - 当前端传递的数据不符合业务要求时

 * 语义：
 * - 对应 HTTP 状态码 400 (Bad Request)
 * - 在全局异常处理器中捕获后，可以返回统一的 ApiResponse
 */
public class InvalidRequestException extends RuntimeException {

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
    public InvalidRequestException() {
        super();
    }

    /**
     * 带消息的构造函数。
     * @param message 异常描述信息，例如 "请求参数不合法"
     */
    public InvalidRequestException(String message) {
        super(message);
    }

    /**
     * 带消息和原因的构造函数。
     * @param message 异常描述信息
     * @param cause   异常的根本原因（可用于异常链）
     */
    public InvalidRequestException(String message, Throwable cause) {
        super(message, cause);
    }

    /**
     * 仅带原因的构造函数。
     * @param cause 异常的根本原因
     */
    public InvalidRequestException(Throwable cause) {
        super(cause);
    }
}
