using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.DTOs.Pagination
{
    public class PaginatedResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalRecords { get; set; }
        public int Page { get; set; }
        public int RecordsPerPage { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / RecordsPerPage);
    }
}
