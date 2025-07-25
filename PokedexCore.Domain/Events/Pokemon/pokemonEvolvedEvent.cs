using PokedexCore.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Domain.Events.Pokemon
{
    public class pokemonEvolvedEvent:IDomainEvent
    {
        public DateTime OccurredOn { get; private set; }
        public Guid EventId { get; private set; }
        public int PokemonId { get; private set; }
        public string NewName { get; private set; }

        public pokemonEvolvedEvent(int pokemonId, string newName)
        {
            OccurredOn = DateTime.UtcNow;
            EventId = Guid.NewGuid();
            PokemonId = pokemonId;
            NewName = newName;
        }            
    }
}
