using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.DTOs.ActionsDtos
{
    public class EvolvePokemonRequest
    {
        public int PokemonId { get; set; }
        public string NewName { get; set; }
        public string NewType { get; set; }
    }
}
