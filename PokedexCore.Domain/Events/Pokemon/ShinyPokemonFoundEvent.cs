using PokedexCore.Domain.Entities;
using PokedexCore.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Domain.Events.Pokemon
{
    public class ShinyPokemonFoundEvent:IDomainEvent
    {
        public DateTime OccurredOn { get; private set; }
        public Guid EventId { get; private set; }
        public int PokemonId { get; private set; }
        public string Name { get; private set; }

        public ShinyPokemonFoundEvent(int pokemonId, string name)
        {
            OccurredOn = DateTime.UtcNow;
            EventId = Guid.NewGuid();
            PokemonId = pokemonId;
            Name = name;
        }
    }
}
