using System.Diagnostics.Contracts;

namespace StudyRoomSystem.Server.Helpers;

public static class QueryableExtension
{
    [Pure]
    public static IQueryable<T> Page<T>(this IQueryable<T> queryable, int page = 1, int pageSize = 20)
    {
        return queryable.Skip((page - 1) * pageSize).Take(pageSize);
    }
}