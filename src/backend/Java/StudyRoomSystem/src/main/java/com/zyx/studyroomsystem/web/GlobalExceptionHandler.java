package com.zyx.studyroomsystem.web;

import com.zyx.studyroomsystem.exception.AuthenticationFailedException;
import com.zyx.studyroomsystem.exception.InvalidRequestException;
import com.zyx.studyroomsystem.exception.ResourceConflictException;
import com.zyx.studyroomsystem.exception.ResourceNotFoundException;
import com.zyx.studyroomsystem.exception.UserAlreadyExistsException;
import org.springframework.http.HttpStatus;
import org.springframework.security.access.AccessDeniedException;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.ResponseStatus;
import org.springframework.web.bind.annotation.RestControllerAdvice;

import javax.security.sasl.AuthenticationException;
import java.io.FileNotFoundException;

/**
 * 全局异常处理器
 * 统一拦截并返回标准化的 ApiResponse
 */
@RestControllerAdvice
public class GlobalExceptionHandler {

    // 400 - 参数错误
    @ResponseStatus(HttpStatus.BAD_REQUEST)
    @ExceptionHandler({IllegalArgumentException.class, InvalidRequestException.class})
    public ApiResponse<?> handleBadRequest(Exception e) {
        // 业务码 1000 表示通用参数错误
        return ApiResponse.fail(1000, "请求不合法: " + e.getMessage());
    }

    // 401 - 未认证
    @ResponseStatus(HttpStatus.UNAUTHORIZED)
    @ExceptionHandler({AuthenticationException.class, AuthenticationFailedException.class})
    public ApiResponse<?> handleAuth(Exception e) {
        // 业务码 2000 表示认证失败
        return ApiResponse.fail(2000, "认证失败: " + e.getMessage());
    }

    // 403 - 权限不足
    @ResponseStatus(HttpStatus.FORBIDDEN)
    @ExceptionHandler(AccessDeniedException.class)
    public ApiResponse<?> handleForbidden(AccessDeniedException e) {
        // 业务码 3000 表示权限不足
        return ApiResponse.fail(3000, "权限不足");
    }

    // 404 - 资源不存在
    @ResponseStatus(HttpStatus.NOT_FOUND)
    @ExceptionHandler({FileNotFoundException.class, ResourceNotFoundException.class})
    public ApiResponse<?> handleNotFound(Exception e) {
        // 业务码 4000 表示资源不存在
        return ApiResponse.fail(4000, "资源不存在: " + e.getMessage());
    }

    // 409 - 资源冲突
    @ResponseStatus(HttpStatus.CONFLICT)
    @ExceptionHandler({ResourceConflictException.class, UserAlreadyExistsException.class})
    public ApiResponse<?> handleConflict(Exception e) {
        // 业务码 5000 表示资源冲突
        return ApiResponse.fail(5000, "资源冲突: " + e.getMessage());
    }

    // 500 - 服务器内部错误
    @ResponseStatus(HttpStatus.INTERNAL_SERVER_ERROR)
    @ExceptionHandler(Exception.class)
    public ApiResponse<?> handleServerError(Exception e) {
        // 业务码 9000 表示未知错误
        return ApiResponse.fail(9000, "服务器内部错误: " + e.getMessage());
    }
}
