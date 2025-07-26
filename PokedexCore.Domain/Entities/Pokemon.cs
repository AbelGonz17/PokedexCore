using MediatR;
using PokedexCore.Domain.Enums;
using PokedexCore.Domain.Events.Pokemon;
using PokedexCore.Domain.Exceptions;
using PokedexCore.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Domain.Entities
{
    public class Pokemon : IEntitiesBase
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string MainType { get; set; }

        public string Region { get; set; }

        public int TrainerId { get; set; }

        public DateTime CaptureDate { get; set; }

        public bool IsShiny { get; set; }

        public bool IsFainted { get; private set; }

        public int Level { get; set; }

        public PokemonStatus Status { get; set; }

        public Trainer Trainer { get; set; }

        private List<INotification> _domainEvents = new();
        public IReadOnlyList<INotification> DomainEvents => _domainEvents.AsReadOnly();

        private Pokemon() { }

        public Pokemon(string name, string mainType, string region, int trainerId)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            MainType = MainType ?? throw new ArgumentNullException(nameof(mainType));
            Region = region ?? throw new ArgumentNullException(nameof(region));
            TrainerId = trainerId;
            CaptureDate = DateTime.UtcNow;
            Level = 1;
            Status = PokemonStatus.Wild;
        }

        public void levelUP()
        {
            if (Level >= 100)
                throw new DomainException("Pokemon has reached maximun level");

            Level++;
            _domainEvents.Add(new PokemonLevelUpEvent(Id, Level));
        }

        public void Evolve(string newName, string newType)
        {
            if (Level < 16)
                throw new DomainException("El Pokémon debe ser al menos nivel 16 para evolucionar.");

            Name = newName;
            MainType = newType;

            _domainEvents.Add(new PokemonEvolvedEvent(Id, newName, newType));
        }

        public void MarkAsShiny()
        {
            IsShiny = true;
            _domainEvents.Add(new ShinyPokemonFoundEvent(Id, Name));
        }

        public void CanBattle()
        {
            if (Level < 5)
                throw new DomainException("El Pokémon debe tener al menos nivel 5 para entrar en batalla.");

            if (IsFainted)
                throw new DomainException("El Pokémon está debilitado y no puede luchar.");

            if (TrainerId <= 0)
                throw new DomainException("Este Pokémon no está asignado a un entrenador.");

            _domainEvents.Add(new PokemonCanBattleEvent(Id, Name, Level));
        }

        public void ClearDomainEvents() => _domainEvents.Clear();
       
    }
}
