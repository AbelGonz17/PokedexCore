using MediatR;
using PokedexCore.Domain.Entities;
using PokedexCore.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Domain.Events.Pokemon
{
    public class ShinyPokemonFoundEvent:INotification
    {
       
        public int PokemonId { get; private set; }
        public string Name { get; private set; }

        public ShinyPokemonFoundEvent(int pokemonId, string name)
        { 
            PokemonId = pokemonId;
            Name = name;
        }
    }
}
