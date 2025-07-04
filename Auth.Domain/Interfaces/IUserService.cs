using Auth.Domain.Entities;
using Auth.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Domain.Interfaces
{
    public interface IUserService
    {
        Task<bool> Register(RegisterRequest request);
        Task<LoginResponse> Login(LoginRequest request);
    }
}
