using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Domain.Interfaces
{
    public interface IDomainEvent
    {
        DateTime OccurredOn { get; }
        Guid EventId { get;}
    }
}
