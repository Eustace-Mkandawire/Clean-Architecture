using Auth.MVC.DTO;
using Auth.MVC.Interfaces;
using Auth.MVC.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using JsonException = Newtonsoft.Json.JsonException;

namespace Auth.MVC.Service
{
    public class AuthService : IAuthServices
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthService> _logger;

        public AuthService(HttpClient httpClient, ILogger<AuthService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<LoginDTO> LoginAsync(LoginViewModel loginDTO)
        {
            var url = "https://localhost:44341/api/Auth/login";

            try
            {
                var transferContent = new StringContent(
                    JsonConvert.SerializeObject(loginDTO),
                    Encoding.UTF8,
                    "application/json");
                var response = await _httpClient.PostAsync(url, transferContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<LoginDTO>(responseContent);

                    if (result != null)
                    {

                        result.IsSuccess = true;
                        return result;
                    }
                    else
                    {
                        _logger.LogError($"{new JsonException(responseContent)}");
                        throw new JsonException(responseContent);
                    }
                }
                else
                {
                    _logger.LogError($"Login failed for {loginDTO.Email}");
                    return new LoginDTO
                    {
                        IsSuccess = false,
                        Message = "Login failed"
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Network error {ex.Message}");
                return new LoginDTO
                {
                    IsSuccess = false,
                    Message = $"Network error: {ex.Message}"
                };
            }
            catch (JsonException ex)
            {
                _logger.LogError($"Error processing response {ex.Message}");
                return new LoginDTO
                {
                    IsSuccess = false,
                    Message = $"Error processing response: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error : {ex.Message}");
                return new LoginDTO
                {
                    IsSuccess = false,
                    Message = $"Unexpected error: {ex.Message}"
                };
            }
        }

        public async Task<RegisterDTO> RegisterAsync(RegisterViewModel registerDTO)
        {
            var url = "https://localhost:44341/api/Auth/register";

            try
            {
                var transferContent = new StringContent(
                    JsonConvert.SerializeObject(registerDTO),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(url, transferContent);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<RegisterDTO>(responseContent);
                    if (result != null)
                    {
                        result.IsSuccess = true;
                        return result;
                    }
                    else
                    {
                        _logger.LogError($"{new JsonException(responseContent)}");
                        throw new JsonException(responseContent);
                    }

                }
                else
                {
                    _logger.LogError("Failed to register user");
                    return new RegisterDTO
                    {
                        IsSuccess = false,
                        Message = "Register failed"
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Network error : {ex.Message}");
                return new RegisterDTO
                {
                    IsSuccess = false,
                    Message = $"Network error: {ex.Message}"
                };
            }
            catch (JsonException ex)
            {
                _logger.LogError($"Error processing response : {ex.Message}");
                return new RegisterDTO
                {
                    IsSuccess = false,
                    Message = $"Error processing response: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex.Message}");
                return new RegisterDTO
                {
                    IsSuccess = false,
                    Message = $"Unexpected error: {ex.Message}"
                };
            }
        }

        public async Task<EmailListDTO> GetCacheEmail()
        {
            var url = "https://localhost:44341/api/Auth/get-cached-emails";

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch emails");
                    return new EmailListDTO { Error = "Failed to fetch emails." };
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var emails = JsonConvert.DeserializeObject<List<string>>(responseContent);
                _logger.LogInformation("No emails found");

                return new EmailListDTO
                {
                    Emails = emails ?? new List<string>(),
                    Message = emails?.Count > 0 ? null : "No emails found."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCacheEmail");
                return new EmailListDTO { Error = "An error occurred while fetching emails." };
            }
        }
    }

}