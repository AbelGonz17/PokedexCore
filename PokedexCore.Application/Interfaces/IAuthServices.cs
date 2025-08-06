using PokedexCore.Application.DTOs;
using PokedexCore.Application.DTOs.Auth;

namespace PokedexCore.Application.Interfaces
{
    public interface IAuthServices
    {
        Task<ApiResponse<AuthenticationResponseDTO>> LoginAsync(CredentialsLoginDTO dto);
        Task<ApiResponse<AuthenticationResponseDTO>> RegistrarAsync(CredentialsUserDTO dto);
    }
}