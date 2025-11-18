package com.zyx.studyroomsystem.web;

/**
 * 通用响应结构，用于统一接口返回格式。
 * 字段说明：
 * - code：状态码（0 表示成功，-1 表示失败，其他值可扩展）
 * - message：提示信息（如 "OK" 或错误原因）
 * - data：实际返回的数据内容（泛型 T，可以是对象、列表或简单值）
 * - timestamp：时间戳（毫秒），用于标记响应生成的时间
 */
public record ApiResponse<T>(int code, String message, T data, long timestamp) {
    public static <T> ApiResponse<T> ok(T data) {
        return new ApiResponse<>(0, "ok", data, System.currentTimeMillis());
    }
    //修改错误的处理代码，使其能够处理更为细致的业务逻辑
    public static ApiResponse<?> fail(int code, String msg) {
        return new ApiResponse<>(code, msg, null, System.currentTimeMillis());
    }
}
