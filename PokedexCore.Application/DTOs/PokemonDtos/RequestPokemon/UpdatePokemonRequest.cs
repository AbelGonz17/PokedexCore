using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.DTOs.PokemonDtos.RequestPokemon
{
    public class UpdatePokemonRequest
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string MainType { get; set; }

        public string Region { get; set; }
    }
}
