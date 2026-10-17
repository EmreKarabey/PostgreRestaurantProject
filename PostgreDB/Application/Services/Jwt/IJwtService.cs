using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Jwt
{
    public interface IJwtService
    {
        string GenerateToken(int userId, string username, string role, bool rememberMe);
    }
}
