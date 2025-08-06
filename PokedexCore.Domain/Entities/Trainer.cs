using PokedexCore.Domain.Enums;
using PokedexCore.Domain.Events;
using PokedexCore.Domain.Events.Trainer;
using PokedexCore.Domain.Exceptions;
using PokedexCore.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Domain.Entities
{
    public class Trainer: IEntitiesBase
    {
        public int Id { get;  set; }

        public string UserName { get; private set; }

        [EmailAddress]
        public string Email { get; private set; }

        public DateTime RegistrationDate { get; private set; }

        public int PokemonCount { get; private set; }

        public TrainerRank Rank { get; private set; }

        private List<Pokemon> _pokemons = new();
        public IReadOnlyList<Pokemon> Pokemons => _pokemons.AsReadOnly();

        private List<IDomainEvent> _domainEvents = new();
        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        private Trainer() { }

        public Trainer(string userName , string email)
        {
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));
            Email = email;
            RegistrationDate = DateTime.Now;
            Rank = TrainerRank.Rookie;

            _domainEvents.Add(new TrainerRegisteredEvent(Id, UserName, Email));

        }

        public Trainer(string userName, string email, string userId) : this(userName, email)
        {
            userId = userId;
        }

        public void CatchPokemon(Pokemon pokemon)
        {
            if (PokemonCount >= GetMaxPokemonCapacity())
                throw new DomainException("Trainer has reached maximum Pokemon capacity");

            _pokemons.Add(pokemon);
            PokemonCount++;

            CheckForRankUp();
            _domainEvents.Add(new PokemonCaughtByTrainer(Id, pokemon.Id));
        }

        public void ReleasePokemon(int pokemonId)
        {
            var pokemon = _pokemons.FirstOrDefault(p => p.Id == pokemonId);
            if (pokemon == null)
                throw new DomainException("Pokemon not found");

            _pokemons.Remove(pokemon);
            PokemonCount--;

            _domainEvents.Add(new PokemonReleasedEvent(Id,pokemon.Id));
        }

        private void CheckForRankUp()
        {
            var newRank = CalculateRank();
            if(newRank != Rank)
            {
                var oldRank = Rank;
                Rank = newRank;
                _domainEvents.Add(new TrainerRankUpEvent(Id, oldRank, newRank));
            }
        }

        private TrainerRank CalculateRank()
        {
            return PokemonCount switch
            {
                >= 50 => TrainerRank.Master,
                >= 25 => TrainerRank.Expert,
                >= 10 => TrainerRank.Advanced,
                >= 3 => TrainerRank.Intermediate,
                _ => TrainerRank.Rookie
            };
        }

        private int GetMaxPokemonCapacity()
        {
            return Rank switch
            {
                TrainerRank.Master => 100,
                TrainerRank.Expert => 50,
                TrainerRank.Advanced => 25,
                TrainerRank.Intermediate => 10,
                _ => 6
            };
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
