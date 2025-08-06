using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.DTOs.PokemonDtos.ResponsePokemon
{
    public class PokemonTypeApiResponse
    {
        public List<PokemonSlot> Pokemon { get; set; }
    }

    public class PokemonSlot
    {
        public NamedApiResource Pokemon { get; set; }
    }

    public class NamedApiResource
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}
