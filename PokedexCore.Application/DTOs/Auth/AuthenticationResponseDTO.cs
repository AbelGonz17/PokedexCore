using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.DTOs.Auth
{
    public class AuthenticationResponseDTO
    {
        public required string Token { get; set; }
        public DateTime FechaExpiracion { get; set; }
    }
}
