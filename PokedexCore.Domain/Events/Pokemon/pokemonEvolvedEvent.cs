using MediatR;
using PokedexCore.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Domain.Events.Pokemon
{
    public class PokemonEvolvedEvent:INotification
    {  
        public int PokemonId { get; private set; }
        public string NewName { get; private set; }
        public string NewType { get; private set; }

        public PokemonEvolvedEvent(int pokemonId, string newName,string newType)
        {
            NewType = newType;
            PokemonId = pokemonId;
            NewName = newName;
        }            
    }
}
