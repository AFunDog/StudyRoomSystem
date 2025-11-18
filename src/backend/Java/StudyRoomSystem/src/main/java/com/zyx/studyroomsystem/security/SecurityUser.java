package com.zyx.studyroomsystem.security;

import com.zyx.studyroomsystem.pojo.User;
import org.springframework.security.core.GrantedAuthority;
import org.springframework.security.core.authority.SimpleGrantedAuthority;
import org.springframework.security.core.userdetails.UserDetails;

import java.util.Collection;
import java.util.List;
import java.util.UUID;

/**
 * SecurityUser 是一个适配器类，用于将系统中的 User 实体
 * 转换为 Spring Security 能识别的 UserDetails 对象。
 * 这样 Spring Security 就能基于数据库用户进行认证和授权。
 */
public class SecurityUser implements UserDetails {
    private final User user;

    public SecurityUser(User user) {
        this.user = user;
    }

    /**
     * 返回用户的权限集合。
     * Spring Security 要求权限以 "ROLE_xxx" 格式表示。
     * 这里直接根据 User 的 role 字段拼接。
     */
    @Override
    public Collection<? extends GrantedAuthority> getAuthorities() {
        return List.of(new SimpleGrantedAuthority("ROLE_" + user.getRole()));
    }

    /**
     * 返回用户的加密密码。
     * Spring Security 会用它来校验登录时输入的密码。
     */
    @Override
    public String getPassword() {
        return user.getPassword();
    }

    /**
     * 返回用户名（唯一标识）。
     * Spring Security 用它来查找用户。
     */
    @Override
    public String getUsername() {
        return user.getUserName();
    }

    /**
     * 以下四个方法用于描述账号状态。
     * 如果需要支持账号过期/锁定等逻辑，可以改为根据 User 字段判断。
     * 目前全部返回 true，表示账号始终有效。
     */
    @Override
    public boolean isAccountNonExpired() { return true; }

    @Override
    public boolean isAccountNonLocked() { return true; }

    @Override
    public boolean isCredentialsNonExpired() { return true; }

    @Override
    public boolean isEnabled() { return true; }

    /**
     * 返回用户的唯一 ID（UUID）。
     * 这个不是 Spring Security 必须的，但在业务逻辑中可能需要。
     */
    public UUID getId() { return user.getId(); }
}
