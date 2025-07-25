using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.DTOs.ActionsDtos
{
    public class ReleasePokemonRequest
    {
        public int TrainerId { get; set; }
        public int PokemonId { get; set; }
    }
}
