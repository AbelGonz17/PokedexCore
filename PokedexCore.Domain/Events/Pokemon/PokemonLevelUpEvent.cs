using PokedexCore.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Domain.Events.Pokemon
{
    public class PokemonLevelUpEvent: IDomainEvent
    {
        public DateTime OccurredOn { get; private set; }
        public Guid EventId { get; private set; }
        public int PokemonId { get; private set; }
        public int NewLevel {  get; private set; }

        public PokemonLevelUpEvent(int pokemonId , int level)
        {
            OccurredOn = DateTime.UtcNow;
            EventId = Guid.NewGuid();
            PokemonId = pokemonId;
            NewLevel = level;      
        }
    }
}
