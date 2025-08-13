using PokedexCore.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Domain.Entities
{
    public class TrainerPokemons:IEntitiesBase
    {
        public int Id {  get; set; }

        public int TrainerId { get; set; }
        public Trainer Trainer { get; set; }

        public int PokemonId { get; set; }
        public Pokemon Pokemon { get; set; }

        public int Quantity { get; set; }

        public bool IsShiny { get; set; }

        public int Level { get; set; } = 1;

        public DateTime LastCaptureDate { get; set; }

        public int Experience { get; set; } = 0;

    }
}
