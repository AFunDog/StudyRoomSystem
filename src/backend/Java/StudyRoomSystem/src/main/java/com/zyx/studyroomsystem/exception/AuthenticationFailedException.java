package com.zyx.studyroomsystem.exception;

import java.io.Serial;

/**
 * AuthenticationFailedException 表示认证失败异常。

 * 使用场景：
 * - 用户登录时用户名不存在
 * - 用户登录时密码错误

 * 语义：
 * - 对应 HTTP 状态码 401 (Unauthorized)
 * - 在全局异常处理器中捕获后，可以返回统一的 ApiResponse
 */
public class AuthenticationFailedException extends RuntimeException {

    @Serial
    private static final long serialVersionUID = 1L;

    /**
     * 无参构造函数。
     */
    public AuthenticationFailedException() {
        super("认证失败");
    }

    /**
     * 带消息的构造函数。
     * @param message 异常描述信息，例如 "用户名或密码错误"
     */
    public AuthenticationFailedException(String message) {
        super(message);
    }

    /**
     * 带消息和原因的构造函数。
     * @param message 异常描述信息
     * @param cause   异常的根本原因
     */
    public AuthenticationFailedException(String message, Throwable cause) {
        super(message, cause);
    }

    /**
     * 仅带原因的构造函数。
     * @param cause 异常的根本原因
     */
    public AuthenticationFailedException(Throwable cause) {
        super(cause);
    }
}
