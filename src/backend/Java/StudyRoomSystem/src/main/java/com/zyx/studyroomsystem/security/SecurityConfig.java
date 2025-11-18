package com.zyx.studyroomsystem.security;

import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.security.config.annotation.web.builders.HttpSecurity;
import org.springframework.security.config.http.SessionCreationPolicy;
import org.springframework.security.web.SecurityFilterChain;
import org.springframework.security.web.authentication.UsernamePasswordAuthenticationFilter;

@Configuration
public class SecurityConfig {

    private final JwtAuthenticationFilter jwtFilter;

    // 构造函数注入 JwtAuthenticationFilter
    public SecurityConfig(JwtAuthenticationFilter jwtFilter) {
        this.jwtFilter = jwtFilter;
    }

    @Bean
    public SecurityFilterChain filterChain(HttpSecurity http) throws Exception {
        http
                // 如果是前后端分离接口，通常关闭 CSRF
                .csrf(csrf -> csrf.disable())
                // 配置请求权限
                .authorizeHttpRequests(auth -> auth
                        // 放行 auth 模块的三个接口
                        .requestMatchers("/api/v1/auth/register").permitAll()
                        .requestMatchers("/api/v1/auth/login").permitAll()
//                        .requestMatchers("/api/v1/auth/check").permitAll()

                        // 放行 OpenAPI 和 Swagger UI 相关端点
                        .requestMatchers("/v3/api-docs/**").permitAll()
                        .requestMatchers("/swagger-ui/**").permitAll()
                        .requestMatchers("/swagger-ui.html").permitAll()

                        //测试
                        .requestMatchers("/test/**").permitAll()

                        // 其他接口需要认证
                        .anyRequest().authenticated()
                )
//                // 使用 HTTP Basic 登录（可以改成 JWT 或表单登录）
//                .httpBasic(Customizer.withDefaults()); // 新版本写法

                // 设置为无状态，JWT 不依赖 Session
                .sessionManagement(session -> session.sessionCreationPolicy(SessionCreationPolicy.STATELESS));

        // 把 JWT 过滤器加到 UsernamePasswordAuthenticationFilter 之前
        http.addFilterBefore(jwtFilter, UsernamePasswordAuthenticationFilter.class);

        return http.build();
    }
}
