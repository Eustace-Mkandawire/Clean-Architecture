using Auth.Application.Infrastructures;
using Auth.Domain.Interfaces;
using Auth.Domain.Models;
using Auth.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Auth.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _authService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService authService, ICacheService cacheService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _cacheService = cacheService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("BadRequest failed to query user data");
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Sending user data to login ");
            var result = await _authService.Login(loginRequest);

            if (!result.Success)
            {
                _logger.LogError($"Failed to login {result.Message}");
                return Unauthorized(new { message = result.Message });
            }

            _logger.LogError($"{result.Message}");
            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            if (!ModelState.IsValid) {
                _logger.LogError($"BadRequest failed to register user");
                return BadRequest(ModelState); 
            }

            _logger.LogInformation("Sending user data to register");
            var success = await _authService.Register(registerRequest);

            if (!success)
            {
                _logger.LogError($"Register failed: User already exists");
                return Conflict(new { message = "User already exists." });
            }

            _logger.LogInformation("User registered successfully");
            return Ok(new { message = "Registration successful." });
        }

        [HttpGet("get-cached-emails")]
        public async Task<IActionResult> GetCachedEmails()
        {
            try
            {
                var cachedEmails = await _cacheService.GetCachedEmailListAsync();

                if (cachedEmails == null || cachedEmails.Count == 0)
                {
                    _logger.LogInformation("No cached emails found");
                    return NotFound("No cached emails found.");
                }

                return Ok(cachedEmails);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize cached email list.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error processing cached email data.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving cached email list.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }
    }
}
