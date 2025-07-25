using PokedexCore.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Domain.Events.Trainer
{
    public class PokemonCapturedEvent:IDomainEvent
    {
        public DateTime OccurredOn { get; private set; }
        public Guid EventId { get; private set; }
        public int PokemonId { get; private set; }
        public string PokemonName { get; private set; }
        public int TrainerId { get; private set; }

        public PokemonCapturedEvent(int pokemonId, string pokemonName, int trainerId )
        {
            OccurredOn = DateTime.Now;
            EventId = Guid.NewGuid();
            PokemonId = pokemonId;
            PokemonName = pokemonName;
            TrainerId = TrainerId;    
        }     

    }
}
