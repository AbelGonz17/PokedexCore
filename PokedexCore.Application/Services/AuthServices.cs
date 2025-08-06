using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PokedexCore.Application.DTOs;
using PokedexCore.Application.DTOs.Auth;
using PokedexCore.Application.DTOs.PokemonDtos.ResponsePokemon;
using PokedexCore.Application.Interfaces;
using PokedexCore.Domain.Entities;
using PokedexCore.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.Services
{
    public class AuthServices : IAuthServices
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IConfiguration configuration;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<AuthServices> logger;

        public AuthServices(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
            IConfiguration configuration, IUnitOfWork unitOfWork, ILogger<AuthServices> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task<ApiResponse<AuthenticationResponseDTO>> RegistrarAsync(CredentialsUserDTO dto)
        {
            var TrainerExists = await unitOfWork.Trainer
                .GetByConditionAsync(c => c.Email == dto.Email);

            if (TrainerExists != null)
            {
                return ApiResponse<AuthenticationResponseDTO>.Fail("This user already exists");
            }


            var user = new IdentityUser { UserName = dto.Email, Email = dto.Email };
            var result = await userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return ApiResponse<AuthenticationResponseDTO>.Fail(errors);
            }

            try
            {
                var trainer = new Trainer(dto.Name, dto.Email, user.Id);

                await unitOfWork.Trainer.AddAsync(trainer);
                await unitOfWork.SaveChangesAsync();

                await userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, "Trainer"));

                return await ConstruirTokenAsync(dto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating user. {Mensaje}", ex.InnerException?.Message ?? ex.Message);
                return ApiResponse<AuthenticationResponseDTO>.Fail("Error creating user.");
            }
        }

        public async Task<ApiResponse<AuthenticationResponseDTO>> LoginAsync(CredentialsLoginDTO dto)
        {
            var usuario = await userManager.FindByEmailAsync(dto.Email);
            if (usuario == null)
            {
                return ApiResponse<AuthenticationResponseDTO>.Fail("User not found");
            }

            var resultado = await signInManager.CheckPasswordSignInAsync(usuario, dto.Password, false);
            if (!resultado.Succeeded)
            {
                return ApiResponse<AuthenticationResponseDTO>.Fail("User or Password incorrect");
            }

            var usuarioDTO = new CredentialsUserDTO
            {
                Email = dto.Email,
                Password = dto.Password,
                Name = usuario.UserName
            };

            return await ConstruirTokenAsync(usuarioDTO);
        }

        private async Task<ApiResponse<AuthenticationResponseDTO>> ConstruirTokenAsync(CredentialsUserDTO dto)
        {
            var trainer = await unitOfWork.Trainer
                .GetByConditionAsync(c => c.Email == dto.Email);

            if (trainer == null)
            {
                return ApiResponse<AuthenticationResponseDTO>.Fail("Trainer not found");
            }

            var claims = new List<Claim>
            {
                new Claim("Email", dto.Email),
                new Claim("TrainerId", trainer.Id.ToString())
            };

            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return ApiResponse<AuthenticationResponseDTO>.Fail("User authentication failed");
            }
            var claimsDB = await userManager.GetClaimsAsync(user);
            claims.AddRange(claimsDB);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["llavejwt"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddYears(1);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            return new ApiResponse<AuthenticationResponseDTO>
            {
                Success = true,
                Data = new AuthenticationResponseDTO
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    FechaExpiracion = expiration
                }
            };

        }

    }
}
