using Microsoft.EntityFrameworkCore;
using PokedexCore.Application.DTOs.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.Services
{
    public static class PaginationExtension
    {
        public static async Task<PaginatedResponse<T>> PaginateAsync<T>(
            this IQueryable<T> query,
            int page,
            int recordsPerPage)
        {
            var totalRecords = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * recordsPerPage)
                .Take(recordsPerPage)
                .ToListAsync();

            return new PaginatedResponse<T>
            {
                Items = items,
                TotalRecords = totalRecords,
                Page = page,
                RecordsPerPage = recordsPerPage
            };
        }
    }
}
