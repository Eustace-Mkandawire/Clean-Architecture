using Auth.Domain.Contracts;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Auth.Domain.Models;
using Auth.Domain.Repositories;
using Auth.Infrastructure.Helpers;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IBusControl _bus;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger, IJwtTokenGenerator jwtTokenGenerator, IPasswordHasher passwordHasher, IBusControl bus)
        {
            _userRepository = userRepository;
            _logger = logger;
            _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email);

            if (user == null)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            bool isPasswordValid = _passwordHasher.VerifyPassword(user.Password, request.Password);

            if (!isPasswordValid)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid password."
                };
            }

            string token = _jwtTokenGenerator.GenerateToken(user);

            var loginResponse = new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                UserName = user.UserName,
                Email = user.Email
            };

            try
            {
                _logger.LogInformation("Publishing LoginCreatedEvent for email: {Email}", user.Email);

                await _bus.Publish(new LoginCreatedContract
                {
                    Email = user.Email
                });

                _logger.LogInformation("Successfully published LoginCreatedEvent for email: {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish LoginCreatedEvent for email: {Email}", user.Email);
                throw;
            }
            return loginResponse;
        }

        public async Task<bool> Register(RegisterRequest request)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);

            if (existingUser != null)
                return false;

            var hashedPassword = _passwordHasher.HashPassword(request.Password);

            var newUser = new User
            {
                UserName = request.UserName,
                Email = request.Email,
                Password = hashedPassword
            };

            try
            {
                _logger.LogInformation("Publishing RegisterCreatedEvent for email: {Email}", request.Email);

                await _bus.Publish(new RegisterCreatedContract { Email = request.Email });


                _logger.LogInformation("Successfully published RegisterCreatedEvent for email: {Email}", request.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish RegisterCreatedEvent for email: {Email}", request.Email);
                throw;
            }

            await _userRepository.AddUserAsync(newUser);

            _logger.LogInformation("New user added successfully");
            return true;
        }
    }
}
