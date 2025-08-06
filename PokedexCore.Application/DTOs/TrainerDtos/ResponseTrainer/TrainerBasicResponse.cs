using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.DTOs.TrainerDtos.ResponseTrainer
{
    public class TrainerBasicResponse
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int PokemonCount { get; set; }
        public string Rank { get; set; }
    }
}
