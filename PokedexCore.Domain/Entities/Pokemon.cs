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

        public int Level { get; set; }

        public PokemonStatus Status { get; set; }

        public Trainer Trainer { get; set; }

        private List<IDomainEvent> _domainEvents = new();
        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

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

        public void Evolve(string newName, string NewType)
        {
            if (Level < 16)
                throw new DomainException("Pokemon must be at least level 16 to evolve");

            Name = newName;
            MainType = NewType;
            _domainEvents.Add(new pokemonEvolvedEvent(Id, newName));
        }

        public void MarkAsShiny()
        {
            IsShiny = true;
            _domainEvents.Add(new ShinyPokemonFoundEvent(Id, Name));
        }

        public bool CanBattle()
        {
            return Status == PokemonStatus.Active && Level > 5;
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
            
    }
}
