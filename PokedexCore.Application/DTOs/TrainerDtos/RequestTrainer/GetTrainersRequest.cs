using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.DTOs.TrainerDtos.RequestTrainer
{
    public class GetTrainersRequest
    {
        public string? UserName { get; set; }

        public string? Rank { get; set; }

        public int Page {  get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
