using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.DTOs
{
    public class PokeApiResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public List<PokemonTypeSlot> Types { get; set; }
        public PokemonSprites Sprites { get; set; }
    }

    public class PokemonTypeSlot
    {
        public int Slot { get; set; }
        public PokemonType Type { get; set; }
    }

    public class PokemonType
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class PokemonSprites
    {
        public string FrontDefault { get; set; }
        public string FrontShiny { get; set; }
        public string BackDefault { get; set; }
        public string BackShiny { get; set; }
    }
}
