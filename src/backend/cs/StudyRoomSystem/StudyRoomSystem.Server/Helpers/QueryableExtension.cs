using System.Diagnostics.Contracts;
using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Structs.Api;

namespace StudyRoomSystem.Server.Helpers;

public static class QueryableExtension
{
    [Pure]
    public static IQueryable<T> Page<T>(this IQueryable<T> queryable, int page = 1, int pageSize = 20)
    {
        Guard.Against.NegativeOrZero(page);
        Guard.Against.OutOfRange(pageSize, nameof(pageSize), 1, 100, "页大小必须在1到1000");

        return queryable.Skip((page - 1) * pageSize).Take(pageSize);
    }

    public static async Task<ApiPageResult<T>> ToApiPageResult<T>(
        this IQueryable<T> queryable,
        int page = 1,
        int pageSize = 20)
    {
        return new ApiPageResult<T>()
        {
            Total = await queryable.CountAsync(),
            Page = page,
            PageSize = pageSize,
            Items = await queryable.Page(page, pageSize).ToListAsync()
        };
    }

    public static ApiPageResult<T> ToApiPageResult<T>(this IEnumerable<T> enumerable, int page = 1, int pageSize = 20)
    {
        var list = enumerable.ToList();
        return new ApiPageResult<T>()
        {
            Total = list.Count,
            Page = page,
            PageSize = pageSize,
            Items = list.Skip((page - 1) * pageSize).Take(pageSize).ToList()
        };
    }
}