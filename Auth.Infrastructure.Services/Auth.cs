using Auth.Application.Infrastructures;
using Auth.Domain.Models;
using Auth.Domain.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Infrastructure.Services
{
    public class Auth : IAuth
    {
        private readonly IAuth _authRepository;
        private readonly IEventBus _bus;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IPasswordHasher _passwordHasher;

        public Auth(IAuth authRepository, IEventBus bus, IJwtTokenGenerator jwtTokenGenerator, IPasswordHasher passwordHasher)
        {
            _authRepository = authRepository;
            _bus = bus;
            _jwtTokenGenerator = jwtTokenGenerator;
            _passwordHasher = passwordHasher;
        }
        public Task AddUserAsync(User newUser)
        {
            throw new NotImplementedException();
        }

        public Task<User> GetUserByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            var user = _authRepository.GetUserByEmailAsync(request.email);

            if (user == null)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            bool isPasswordValid = _passwordHasher.VerifyPassword(user.Result.password, request.password);

            if (!isPasswordValid)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid password."
                };
            }

            string token = _jwtTokenGenerator.GenerateToken(user.Result);

            var createLoginCommand = new CreateLoginCommand(request.email, request.email);
            _bus.SendCommand(createLoginCommand);

            return new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                userName = user.Result.userName,
                email = user.Result.email
            };

        }

        public async Task<bool> RegisterUser(RegisterRequest request)
        {
            var existingUser = await _authRepository.GetUserByEmailAsync(request.email);
            if (existingUser != null)
            {
                return false; // User already exists
            }

            var hashedPassword = _passwordHasher.HashPassword(request.password);

            // Create user entity
            var newUser = new User
            {
                userName = request.userName,
                email = request.email,
                password = hashedPassword
            };

            var createRegisterCommand = new CreateRegisterCommand(query.userName, query.email, query.password);
            await _bus.SendCommand(createRegisterCommand);
            // Save user
            await _authRepository.AddUserAsync(newUser);

            return true;
        }
    }
}
