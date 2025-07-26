using MediatR;
using PokedexCore.Domain.Events.Pokemon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.Events.Handlers
{
    public class PokemonCanBattleEventHanlder:INotificationHandler<PokemonCanBattleEvent>
    {
        public Task Handle(PokemonCanBattleEvent notification , CancellationToken cancellationToken)
        {
            Console.WriteLine($"The Pokémon {notification.Name} (ID: {notification.PokemonId}) can enter battle (Level {notification.Level})");

            return Task.CompletedTask;
        }
    }
}
