namespace StudyRoomSystem.Server.Helpers;

/// <summary>
/// 使用 BCrypt 算法进行密码的加密和校验
/// </summary>
public static class PasswordHelper
{
    /// <summary>
    /// 传入明文密码输出加密后的密码
    /// </summary>
    /// <param name="password"></param>
    /// <returns></returns>
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <summary>
    /// 比对明文密码和加密后的密码
    /// </summary>
    /// <param name="password"></param>
    /// <param name="hashPassword"></param>
    /// <returns></returns>
    public static bool CheckPassword(string password,string hashPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashPassword);
    }
}
