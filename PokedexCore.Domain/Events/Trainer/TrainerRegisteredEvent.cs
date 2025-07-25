using PokedexCore.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Domain.Events.Trainer
{
    public class TrainerRegisteredEvent: IDomainEvent
    {
        public DateTime OccurredOn { get; private set; }
        public Guid EventId { get; private set; }
        public int TrainerId { get; private set; }
        public string UserName { get; private set; }
        public string Email { get; private set; }

        public TrainerRegisteredEvent(int trainerId, string userName, string email)
        {
            OccurredOn = DateTime.UtcNow;
            EventId = Guid.NewGuid();
            UserName = userName;
            Email = email;
            TrainerId = trainerId;      
        }

    }
}
