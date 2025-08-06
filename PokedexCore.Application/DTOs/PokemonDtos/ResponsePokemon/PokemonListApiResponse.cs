using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.DTOs.PokemonDtos.ResponsePokemon
{
    public class PokemonListApiResponse
    {
        public List<PokemonBasicInfo> Results { get; set; }
        public int Count { get; set; }
        public string Next { get; set; }
        public string Previous { get; set; }

    }
}
