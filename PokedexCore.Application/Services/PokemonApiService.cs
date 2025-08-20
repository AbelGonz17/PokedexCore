using Microsoft.Extensions.Caching.Memory;
using PokedexCore.Application.DTOs;
using PokedexCore.Application.DTOs.PokemonDtos.ResponsePokemon;
using PokedexCore.Application.Interfaces.ExternalServices;
using PokedexCore.Domain.Enums;
using PokedexCore.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
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

        public async Task<List<PokemonListResponse>> GetAllPokemonsAsync(int limit= 10 , int offset = 0)
        {          
            try
            {        
                // Obtener la lista de pokemones
                var listResponse = await httpClient.GetAsync($"https://pokeapi.co/api/v2/pokemon?limit={limit}&offset={offset}");
                if (!listResponse.IsSuccessStatusCode)
                    return new List<PokemonListResponse>();

                var listJson = await listResponse.Content.ReadAsStringAsync();
                var pokemonList = JsonSerializer.Deserialize<PokemonListApiResponse>(listJson, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                var pokemonResponses = new List<PokemonListResponse>();
              
                var tasks = pokemonList.Results.Select(async pokemon =>
                {
                    try
                    {
                        var pokemonData = await GetPokemonDataAsync(pokemon.Name);
                        return new PokemonListResponse
                        {
                            Id = pokemonData.Id,
                            Name = pokemonData.Name,
                            MainType = pokemonData.Types.FirstOrDefault() ?? "unknown",                            
                        };
                    }
                    catch
                    {
                        return null; // En caso de error, retornamos null
                    }
                });

                var results = await Task.WhenAll(tasks);
                return results.Where(r => r != null).ToList();           

            }
            catch
            {
                return new List<PokemonListResponse>();
            }
        }

        public async Task<ApiResponse<PokemonDetailResponse>> GetPokemonByNameAsync(string name)
        {
            var data = await GetPokemonDataAsync(name);
            if (data == null)
            {
                return ApiResponse<PokemonDetailResponse>.Fail("Pokémon not found");
            }

            var response = new PokemonDetailResponse
            {
                Id = data.Id,
                Name = data.Name,
                MainType = data.Types.FirstOrDefault() ?? "unknown",
                Region = "Unknown",
                CaptureDate = DateTime.UtcNow,
                Level = 1,
                IsShiny = false,
                Status = PokemonStatus.Active,
                Trainer = null
            };

            return ApiResponse<PokemonDetailResponse>.Ok(response);
        }

        public async Task<List<PokemonListResponse>> GetPokemonsByTypeAsync(string type, int limit, int offset)
        {
            var response = await httpClient.GetAsync($"https://pokeapi.co/api/v2/type/{type.ToLower()}");
            if (!response.IsSuccessStatusCode)
                return new List<PokemonListResponse>();

            var json = await response.Content.ReadAsStringAsync();
            var typeData = JsonSerializer.Deserialize<PokemonTypeApiResponse>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            // Lista completa de pokémon de ese tipo
            var allPokemons = typeData.Pokemon
                .Select(p => p.Pokemon.Name)
                .ToList();

            // Paginamos manualmente
            var pagedNames = allPokemons.Skip(offset).Take(limit).ToList();

            // Obtener detalles de esos pokémon
            var tasks = pagedNames.Select(async name =>
            {
                var pokemonData = await GetPokemonDataAsync(name);
                return new PokemonListResponse
                {
                    Id = pokemonData.Id,
                    Name = pokemonData.Name,
                    MainType = pokemonData.Types.FirstOrDefault() ?? "unknown",
                    Level = 1
                };
            });

            var results = await Task.WhenAll(tasks);
            return results.Where(r => r != null).ToList();
        }

        public async Task<int> GetPokemonTotalCountAsync()
        {
            try
            {
                var response = await httpClient.GetAsync("https://pokeapi.co/api/v2/pokemon?limit=1");
                if (!response.IsSuccessStatusCode)
                    return 0;

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PokemonListApiResponse>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                return result?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
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

        public async Task<string> GetNextEvolutionAsync(string currentPokemonName)
        {
            if (string.IsNullOrWhiteSpace(currentPokemonName))
                return null;

            JsonElement speciesResponse;

            try
            {
                speciesResponse = await httpClient.GetFromJsonAsync<JsonElement>(
                    $"https://pokeapi.co/api/v2/pokemon-species/{currentPokemonName.ToLower()}");
            }
            catch (HttpRequestException)
            {
                // Pokémon no encontrado
                return null;
            }

            if (!speciesResponse.TryGetProperty("evolution_chain", out var evolutionChainElement) ||
                !evolutionChainElement.TryGetProperty("url", out var urlElement))
            {
                return null;
            }

            string evolutionChainUrl = urlElement.GetString();

            var chainResponse = await httpClient.GetFromJsonAsync<JsonElement>(evolutionChainUrl);
            if (chainResponse.ValueKind == JsonValueKind.Undefined || chainResponse.ValueKind == JsonValueKind.Null)
                return null;

            var chain = chainResponse.GetProperty("chain");

            return FindNextEvolution(chain, currentPokemonName.ToLower());
        }

        public async Task<int> GetTotalEvolutionsAsync(string currentPokemonName)
        {
            JsonElement speciesResponse;

            try
            {
                speciesResponse = await httpClient.GetFromJsonAsync<JsonElement>(
                    $"https://pokeapi.co/api/v2/pokemon-species/{currentPokemonName.ToLower()}");
            }
            catch (HttpRequestException)
            {
                return 0;
            }

            if (!speciesResponse.TryGetProperty("evolution_chain", out var evolutionChainElement) ||
                !evolutionChainElement.TryGetProperty("url", out var urlElement))
            {
                return 0;
            }

            string evolutionChainUrl = urlElement.GetString();

            var chainResponse = await httpClient.GetFromJsonAsync<JsonElement>(evolutionChainUrl);
            if (chainResponse.ValueKind == JsonValueKind.Undefined || chainResponse.ValueKind == JsonValueKind.Null)
                return 0;

            var chain = chainResponse.GetProperty("chain");

            return CountEvolutions(chain);
        }

        private string FindNextEvolution(JsonElement chainNode, string currentName)
        {
            var speciesName = chainNode.GetProperty("species").GetProperty("name").GetString().ToLower();

            if (speciesName == currentName)
            {
                var evolvesTo = chainNode.GetProperty("evolves_to");
                if (evolvesTo.GetArrayLength() > 0)
                    return evolvesTo[0].GetProperty("species").GetProperty("name").GetString();
                return null;
            }

            foreach (var evo in chainNode.GetProperty("evolves_to").EnumerateArray())
            {
                var result = FindNextEvolution(evo, currentName);
                if (result != null)
                    return result;
            }

            return null;
        }

        private int CountEvolutions(JsonElement chainNode)
        {
            int count = 0;
            var evolvesTo = chainNode.GetProperty("evolves_to");

            if (evolvesTo.GetArrayLength() > 0)
            {
                count += evolvesTo.GetArrayLength();
                foreach (var evo in evolvesTo.EnumerateArray())
                {
                    count += CountEvolutions(evo);
                }
            }

            return count;
        }
    }
}

