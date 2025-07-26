using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Domain.Events.Pokemon
{
    public class PokemonCanBattleEvent:INotification
    {
        public int PokemonId { get; private set; }
        public string Name { get; private set; }
        public int Level { get; private set; }

        public PokemonCanBattleEvent(int pokemonId, string name, int level)
        {
            PokemonId = pokemonId;
            Name = name;
            Level = level;    
        }
    }
}
