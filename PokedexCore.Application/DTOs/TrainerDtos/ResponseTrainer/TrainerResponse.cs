using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.DTOs.TrainerDtos.ResponseTrainer
{
    public class TrainerResponse
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public DateTime RegistrationDate { get; set; }

        public string Rank { get; set; }

        public int PokemonCount { get; set; }

    }
}
