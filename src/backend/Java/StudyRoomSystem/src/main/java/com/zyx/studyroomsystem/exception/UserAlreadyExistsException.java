package com.zyx.studyroomsystem.exception;

import java.io.Serial;

/**
 * UserAlreadyExistsException 表示用户已存在异常。

 * 使用场景：
 * - 当用户注册时，用户名或邮箱已存在

 * 语义：
 * - 对应 HTTP 状态码 409 (Conflict)
 * - 在全局异常处理器中捕获后，可以返回统一的 ApiResponse
 */
public class UserAlreadyExistsException extends ResourceConflictException {

    @Serial
    private static final long serialVersionUID = 1L;

    /**
     * 无参构造函数。
     */
    public UserAlreadyExistsException() {
        super("用户已存在");
    }

    /**
     * 带消息的构造函数。
     * @param message 异常描述信息，例如 "用户名已存在"
     */
    public UserAlreadyExistsException(String message) {
        super(message);
    }

    /**
     * 带消息和原因的构造函数。
     * @param message 异常描述信息
     * @param cause   异常的根本原因
     */
    public UserAlreadyExistsException(String message, Throwable cause) {
        super(message, cause);
    }

    /**
     * 仅带原因的构造函数。
     * @param cause 异常的根本原因
     */
    public UserAlreadyExistsException(Throwable cause) {
        super(cause);
    }
}
