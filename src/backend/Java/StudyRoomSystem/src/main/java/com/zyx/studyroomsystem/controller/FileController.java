package com.zyx.studyroomsystem.controller;

import com.zyx.studyroomsystem.exception.InvalidRequestException;
import com.zyx.studyroomsystem.exception.ResourceConflictException;
import com.zyx.studyroomsystem.exception.ResourceNotFoundException;
import com.zyx.studyroomsystem.web.ApiResponse;
import jakarta.validation.constraints.NotBlank;
import org.springframework.core.io.Resource;
import org.springframework.core.io.UrlResource;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.multipart.MultipartFile;

import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.StandardCopyOption;
import java.util.List;
import java.util.Map;
import java.util.UUID;

@RestController
@RequestMapping("/api/v1/file")
public class FileController {

    private static final Path UPLOAD_DIR = Path.of("uploads");

    /** 文件上传接口 */
    @PostMapping
    public ApiResponse<?> upload(@RequestParam("file") MultipartFile file) throws Exception {
        if (file.isEmpty()) {
            throw new ResourceConflictException("上传文件为空");
        }
        Files.createDirectories(UPLOAD_DIR);

        String originalName = file.getOriginalFilename();
        //校验空文件
        if (originalName == null || originalName.isBlank()) {
            throw new InvalidRequestException("上传文件缺少文件名");
        }
        //文件大小限制
        if (file.getSize() > 50 * 1024 * 1024) { // 50MB
            throw new ResourceConflictException("文件过大，超过限制");
        }
        //文件类型限制
        String ext = originalName.substring(originalName.lastIndexOf('.') + 1).toLowerCase();
        if (!List.of("jpg", "png", "pdf").contains(ext)) {
            throw new ResourceConflictException("不支持的文件类型: " + ext);
        }
        // 清理文件名，避免路径穿越
        originalName = Path.of(originalName).getFileName().toString();
        Path target = UPLOAD_DIR.resolve(UUID.randomUUID() + "_" + originalName);

        // 如果文件已存在则覆盖
        Files.copy(file.getInputStream(), target, StandardCopyOption.REPLACE_EXISTING);

        //返回文件名称、大小、类型，方便前端展示
        return ApiResponse.ok(Map.of("fileName", target.getFileName().toString(),
                "size", file.getSize(),
                "type", ext
        ));
    }

    /** 文件下载接口 */
    @GetMapping("/{file}")
    public ResponseEntity<Resource> download(@PathVariable @NotBlank String file) throws Exception {
        Path path = UPLOAD_DIR.resolve(file).normalize();

        // 安全校验：防止路径穿越攻击
        if (!path.startsWith(UPLOAD_DIR)) {
            throw new ResourceConflictException("非法文件路径");
        }

        if (!Files.exists(path)) {
            throw new ResourceNotFoundException("文件不存在: " + file);
        }

        //根据文件扩展名动态设置更合适的 MIME 类型（比如图片返回 image/png）
        Resource res = new UrlResource(path.toUri());
        String contentType = Files.probeContentType(path);
        if (contentType == null) {
            //截取文件后缀
            String fileName = path.getFileName().toString();
            int dotIndex = fileName.lastIndexOf('.');
            String ext = (dotIndex != -1) ? fileName.substring(dotIndex + 1).toLowerCase() : "";

            contentType = switch (ext) {
                case "jpg", "jpeg" -> MediaType.IMAGE_JPEG_VALUE;
                case "png" -> MediaType.IMAGE_PNG_VALUE;
                case "pdf" -> MediaType.APPLICATION_PDF_VALUE;
                default -> MediaType.APPLICATION_OCTET_STREAM_VALUE;
            };
        }

        return ResponseEntity.ok()
                .contentType(MediaType.parseMediaType(contentType))
                .header("Content-Disposition", "attachment; filename=\"" + path.getFileName() + "\"")
                .body(res);

    }

    /** 删除文件接口 */
    @DeleteMapping("/{file}")
    public ApiResponse<?> delete(@PathVariable @NotBlank String file) throws Exception {
        Path path = UPLOAD_DIR.resolve(file).normalize();

        if (!path.startsWith(UPLOAD_DIR)) {
            throw new ResourceConflictException("非法文件路径");
        }

        if (!Files.exists(path)) {
            throw new ResourceNotFoundException("文件不存在: " + file);
        }

        Files.delete(path);
        return ApiResponse.ok(Map.of("deleted", true));
    }
}
