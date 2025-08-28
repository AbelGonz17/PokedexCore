using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.DTOs
{
    public class PokemonApiData
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime CaptureDate { get; set; }

        public List<string> Types { get; set; }

        public string SpriteUrl { get; set; }

        public int Height { get; set; }

        public int Weight { get; set; }

        public string Region { get; set; }

        public int Level { get; set; }
    }
}
