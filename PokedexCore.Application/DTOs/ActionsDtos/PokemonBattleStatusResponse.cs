using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.DTOs.ActionsDtos
{
    public class PokemonBattleStatusResponse
    {
        public int PokemonId { get; set; }
        public bool CanBattle {  get; set; }
        public string Reason { get; set; }
    }
}
