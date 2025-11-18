package com.zyx.studyroomsystem.web;

import com.zyx.studyroomsystem.exception.AuthenticationFailedException;
import com.zyx.studyroomsystem.exception.InvalidRequestException;
import com.zyx.studyroomsystem.exception.ResourceConflictException;
import com.zyx.studyroomsystem.exception.ResourceNotFoundException;
import com.zyx.studyroomsystem.exception.UserAlreadyExistsException;
import org.springframework.http.HttpStatus;
import org.springframework.security.access.AccessDeniedException;
import org.springframework.web.bind.MethodArgumentNotValidException;
import org.springframework.validation.BindException;
import org.springframework.validation.FieldError;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.ResponseStatus;
import org.springframework.web.bind.annotation.RestControllerAdvice;

import javax.security.sasl.AuthenticationException;
import java.io.FileNotFoundException;

/**
 * 全局异常处理器
 * 统一拦截并返回标准化的 ApiResponse
 *
 * 设计思路：
 * - 每类异常对应一个 HTTP 状态码
 * - 同时返回业务码（方便前端区分错误类型）
 * - 保证前后端交互的错误响应格式一致
 */
@RestControllerAdvice
public class GlobalExceptionHandler {

    /**
     * 400 - 参数错误（业务逻辑上的非法参数）
     * 例如：分页参数为负数、非法枚举值等
     */
    @ResponseStatus(HttpStatus.BAD_REQUEST)
    @ExceptionHandler({IllegalArgumentException.class, InvalidRequestException.class})
    public ApiResponse<?> handleBadRequest(Exception e) {
        // 业务码 1000 表示通用参数错误
        return ApiResponse.fail(1000, "请求不合法: " + e.getMessage());
    }

    /**
     * 400 - 参数校验失败（@Valid 注解触发）
     * 例如：密码长度不足、邮箱格式不正确等
     */
    @ResponseStatus(HttpStatus.BAD_REQUEST)
    @ExceptionHandler({MethodArgumentNotValidException.class, BindException.class})
    public ApiResponse<?> handleValidationException(Exception e) {
        StringBuilder sb = new StringBuilder("参数校验失败: ");
        if (e instanceof MethodArgumentNotValidException ex) {
            for (FieldError error : ex.getBindingResult().getFieldErrors()) {
                sb.append(error.getField()).append(" ").append(error.getDefaultMessage()).append("; ");
            }
        } else if (e instanceof BindException ex) {
            for (FieldError error : ex.getBindingResult().getFieldErrors()) {
                sb.append(error.getField()).append(" ").append(error.getDefaultMessage()).append("; ");
            }
        }
        // 业务码 1001 表示校验错误
        return ApiResponse.fail(1001, sb.toString());
    }

    /**
     * 401 - 未认证（用户未登录或认证失败）
     */
    @ResponseStatus(HttpStatus.UNAUTHORIZED)
    @ExceptionHandler({AuthenticationException.class, AuthenticationFailedException.class})
    public ApiResponse<?> handleAuth(Exception e) {
        // 业务码 2000 表示认证失败
        return ApiResponse.fail(2000, "认证失败: " + e.getMessage());
    }

    /**
     * 403 - 权限不足（用户已登录但无权限访问资源）
     */
    @ResponseStatus(HttpStatus.FORBIDDEN)
    @ExceptionHandler(AccessDeniedException.class)
    public ApiResponse<?> handleForbidden(AccessDeniedException e) {
        // 业务码 3000 表示权限不足
        return ApiResponse.fail(3000, "权限不足");
    }

    /**
     * 404 - 资源不存在
     * 例如：查找的用户/文件不存在
     */
    @ResponseStatus(HttpStatus.NOT_FOUND)
    @ExceptionHandler({FileNotFoundException.class, ResourceNotFoundException.class})
    public ApiResponse<?> handleNotFound(Exception e) {
        // 业务码 4000 表示资源不存在
        return ApiResponse.fail(4000, "资源不存在: " + e.getMessage());
    }

    /**
     * 409 - 资源冲突
     * 例如：用户名已存在、数据唯一性冲突
     */
    @ResponseStatus(HttpStatus.CONFLICT)
    @ExceptionHandler({ResourceConflictException.class, UserAlreadyExistsException.class})
    public ApiResponse<?> handleConflict(Exception e) {
        // 业务码 5000 表示资源冲突
        return ApiResponse.fail(5000, "资源冲突: " + e.getMessage());
    }

    /**
     * 500 - 服务器内部错误
     * 捕获所有未处理的异常，避免堆栈信息泄露给前端
     */
    @ResponseStatus(HttpStatus.INTERNAL_SERVER_ERROR)
    @ExceptionHandler(Exception.class)
    public ApiResponse<?> handleServerError(Exception e) {
        // 业务码 9000 表示未知错误
        return ApiResponse.fail(9000, "服务器内部错误: " + e.getMessage());
    }
}
