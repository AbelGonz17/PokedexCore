using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.DTOs.TrainerDtos.RequestTrainer
{
    public class UpdateTrainerRequest
    {
        public int Id {  get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }
    }
}
