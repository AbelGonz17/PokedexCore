using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Data.Securtiry
{
    public class CurrentUserHelper : ICurrentUserHelper
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public CurrentUserHelper(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public int GetUserId()
        {
            var userId = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == "TrainerId")?.Value;
            if (userId == null)
            {
                throw new Exception("User not found");
            }
            return int.Parse(userId);
        }
    }
}
