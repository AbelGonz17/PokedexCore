using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.DTOs.Pagination
{
    public class PaginationDTO
    {
        public int Page { get; set; }
        private int recordsForPage = 10;
        private readonly int RecordMaximumQuantityPerPage = 20;
        
        public int RecordsForPage
        {
            get
            {
                return recordsForPage;
            }
            set
            {
                recordsForPage = (value > RecordMaximumQuantityPerPage) ? RecordMaximumQuantityPerPage : value;
            }
        }

    }
}
