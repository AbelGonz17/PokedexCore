using PokedexCore.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Domain.Events.Trainer
{
    public class PokemonCaughtByTrainer:IDomainEvent
    {
        public DateTime OccurredOn { get;  private set; }
        public Guid EventId {  get;  private set; }
        public int TrainerId { get; private set; } 
        public int PokemonId { get; private set; }

        public PokemonCaughtByTrainer(int trainerId, int pokemonId)
        {
            TrainerId = trainerId;
            PokemonId = pokemonId;
            OccurredOn = DateTime.UtcNow;
            EventId = Guid.NewGuid();
        }
    }
}
