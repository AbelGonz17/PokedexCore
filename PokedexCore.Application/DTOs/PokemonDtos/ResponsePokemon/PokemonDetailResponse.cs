using PokedexCore.Application.DTOs.TrainerDtos.ResponseTrainer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.DTOs.PokemonDtos.ResponsePokemon
{
    public class PokemonDetailResponse
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string MainType { get; set; }

        public string Region { get; set; }

        public DateTime CaptureDate { get; set; }

        public int Level { get; set; }

        public bool IsShiny { get; set; }

        public string Status { get; set; }

        public TrainerBasicResponse Trainer { get; set; }
    }
}
