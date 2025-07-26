using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.DTOs.PokemonDtos.RequestPokemon
{
    public class GetPokemonsRequest
    {
        public int? TrainerId { get; set; }

        public string? MainType { get; set; }

        public string? Region { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int recordsPerPage { get; set; } = 20;
    }
}
