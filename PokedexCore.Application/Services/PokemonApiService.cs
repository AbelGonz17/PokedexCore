using PokedexCore.Application.DTOs;
using PokedexCore.Application.Interfaces.ExternalServices;
using PokedexCore.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PokedexCore.Application.Services
{
    public class PokemonApiService : IPokemonApiService
    {
        private const string BASE_URL = "https://pokeapi.co/api/v2/pokemon/";
        private readonly HttpClient httpClient;

        public PokemonApiService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<bool> PokemonExistAsync(string pokemonName)
        {
            try
            {
                var response = await httpClient.GetAsync($"{BASE_URL}{pokemonName.ToLower()}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<PokemonApiData> GetPokemonDataAsync(string pokemonName)
        {
            var response = await httpClient.GetAsync($"{BASE_URL}{pokemonName.ToLower()}");

            if (!response.IsSuccessStatusCode)
                throw new DomainException($"Pokemon '{pokemonName}' not found in PokeApi");

            var json = await response.Content.ReadAsStringAsync();

            var pokemonApiResponse = JsonSerializer.Deserialize<PokeApiResponse>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            return new PokemonApiData
            {
                Name = pokemonApiResponse.Name,
                Types = pokemonApiResponse.Types.Select(t => t.Type.Name).ToList(),
                SpriteUrl = pokemonApiResponse.Sprites.FrontDefault,
                Height = pokemonApiResponse.Height,
                Weight = pokemonApiResponse.Weight,
                Id = pokemonApiResponse.Id
            };
        }
    }
}
