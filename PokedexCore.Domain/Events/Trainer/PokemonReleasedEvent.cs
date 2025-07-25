using PokedexCore.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Domain.Events
{
    public class PokemonReleasedEvent:IDomainEvent
    {
        public DateTime OccurredOn { get; private set; }
        public Guid EventId { get; private set; }
        public int TrainerID { get; private set; }
        public int PokemonId {  get; private set; }

        public PokemonReleasedEvent(int trainerId, int pokemonId)
        {
            TrainerID = trainerId;
            PokemonId = pokemonId;
            OccurredOn = DateTime.UtcNow;
            EventId = Guid.NewGuid();
           
        }
    }
}
