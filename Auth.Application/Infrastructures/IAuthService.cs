using Auth.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Application.Infrastructures
{
    public interface IAuthService
    {
        Task<bool> RegisterUser(RegisterRequest request);

        Task<LoginResponse> Login(LoginRequest query);
    }
}
