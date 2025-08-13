using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.DTOs.TrainerDtos.RequestTrainer
{
    public class ReleasePokemonRequest
    {
        public string Name { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
