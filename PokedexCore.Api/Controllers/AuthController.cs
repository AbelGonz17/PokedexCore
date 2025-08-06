using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PokedexCore.Application.DTOs.Auth;
using PokedexCore.Application.Interfaces;
using System.Security.Cryptography.Pkcs;

namespace PokedexCore.Api.Controllers
{
    [ApiController]
    [Route("api/Auth")]
    public class AuthController:ControllerBase
    {
        private readonly IAuthServices authSercices;

        public AuthController(IAuthServices authSercices)
        {
            this.authSercices = authSercices;
        }

        [HttpPost("Registration")]
        public async Task<IActionResult> Registration(CredentialsUserDTO credentialsUserDTO)
        {
            var result = await authSercices.RegistrarAsync(credentialsUserDTO);

            if (result.Success)
            {
                return Ok(result); 
            }
            else
            {
                return BadRequest(result); 
            }
        }

        [HttpPost("Login")]        
        public async Task<IActionResult> Login (CredentialsLoginDTO credentialsLoginDTO)
        {
            var result = await authSercices.LoginAsync(credentialsLoginDTO);
            
            if(result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
    }
}
