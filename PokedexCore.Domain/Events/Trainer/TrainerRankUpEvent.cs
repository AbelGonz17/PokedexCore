using PokedexCore.Domain.Enums;
using PokedexCore.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Domain.Events.Trainer
{
    public class TrainerRankUpEvent:IDomainEvent
    {
        public DateTime OccurredOn { get; private set; }
        public Guid EventId { get; private set; }
        public int TrainerId { get; private set; }
        public TrainerRank OldRank { get; private set; }
        public TrainerRank NewRank { get; private set; }

        public TrainerRankUpEvent(int trainerId, TrainerRank oldRank, TrainerRank nerRank)
        {
            OccurredOn = DateTime.UtcNow;
            EventId = Guid.NewGuid();
            TrainerId = trainerId;
            OldRank = oldRank;
            NewRank = nerRank;
        }
    }
}
