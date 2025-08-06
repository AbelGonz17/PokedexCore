using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.DTOs.PokemonDtos.ResponsePokemon
{
    public class PokemonTypeResponse
    {
        public List<PokemonTypeSlot> Pokemon { get; set; } = new();
    }
}
