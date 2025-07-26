using MediatR;
using PokedexCore.Domain.Events.Pokemon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.Events.Handlers
{
    public class PokemonLevelUpEventHandler: INotificationHandler<PokemonLevelUpEvent>
    {
        public Task Handle(PokemonLevelUpEvent notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"The Pokémon with ID {notification.PokemonId} has been leveled up to {notification.NewLevel}");

            return Task.CompletedTask;
        }

    }
}
