using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.DTOs.PokemonDtos.ResponsePokemon
{
    public class PokemonSummaryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MainType { get; set; }
        public int Level { get; set; }
        public string Region { get; set; }
        public string SpriteUrl { get; set; }
    }
}
