using System.Diagnostics.Contracts;
using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Structs.Api;

namespace StudyRoomSystem.Server.Helpers;

public static class QueryableExtension
{
    extension<T>(IQueryable<T> queryable)
    {
        [Pure]
        public IQueryable<T> Page(int page = 1, int pageSize = 20)
        {
            Guard.Against.NegativeOrZero(page);
            Guard.Against.OutOfRange(pageSize, nameof(pageSize), 1, 100, "页大小必须在1到1000");

            return queryable.Skip((page - 1) * pageSize).Take(pageSize);
        }

        public async Task<ApiPageResult<T>> ToApiPageResult(
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
    }
}