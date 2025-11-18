package com.zyx.studyroomsystem.security;

import io.jsonwebtoken.Jwts;
import io.jsonwebtoken.SignatureAlgorithm;
import io.jsonwebtoken.security.Keys;
import org.springframework.security.core.userdetails.UserDetails;
import org.springframework.stereotype.Component;

import java.security.Key;
import java.util.Date;

@Component
public class JwtUtil {
    // 秘钥（至少 256bit，建议放到配置文件中）
    private final String secret = "yourSecretKeyyourSecretKeyyourSecretKey";
    // Token 有效期：1小时
    private final long expiration = 3600_000;

    // 获取签名用的 Key 对象（替代旧的 String 写法）
    private Key getSigningKey() {
        return Keys.hmacShaKeyFor(secret.getBytes());
    }

    /**
     * 生成 JWT Token
     * @param userDetails Spring Security 用户对象
     * @return JWT 字符串
     */
    public String generateToken(UserDetails userDetails) {
        return Jwts.builder()
                .setSubject(userDetails.getUsername()) // 设置用户名
                .claim("role", userDetails.getAuthorities().iterator().next().getAuthority()) // 保存角色信息
                .setIssuedAt(new Date()) // 签发时间
                .setExpiration(new Date(System.currentTimeMillis() + expiration)) // 过期时间
                .signWith(getSigningKey(), SignatureAlgorithm.HS256) // 使用 HS256 算法签名
                .compact();
    }

    /**
     * 从 Token 中解析出用户名
     */
    public String extractUsername(String token) {
        return Jwts.parserBuilder()
                .setSigningKey(getSigningKey())
                .build()
                .parseClaimsJws(token)
                .getBody()
                .getSubject();
    }

    /**
     * 校验 Token 是否有效
     */
    public boolean validateToken(String token, UserDetails userDetails) {
        String username = extractUsername(token);
        return username.equals(userDetails.getUsername()) && !isTokenExpired(token);
    }

    /**
     * 判断 Token 是否过期
     */
    private boolean isTokenExpired(String token) {
        Date expiration = Jwts.parserBuilder()
                .setSigningKey(getSigningKey())
                .build()
                .parseClaimsJws(token)
                .getBody()
                .getExpiration();
        return expiration.before(new Date());
    }
}

