using MediatR;
using PokedexCore.Domain.Events.Pokemon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.Events.Handlers
{
    public class PokemonEvolvedEventHandler:INotificationHandler<PokemonEvolvedEvent>
    {
        public Task Handle(PokemonEvolvedEvent notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"The Pokémon with ID {notification.PokemonId} has evolved to {notification.NewName} ({notification.NewType})!");

            return Task.CompletedTask;
        }
    }
}
