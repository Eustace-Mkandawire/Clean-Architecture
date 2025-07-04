using Auth.Domain.Constants;
using Auth.Domain.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Auth.Infrastructure.Cache
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheService> _logger;
        private readonly IConfiguration _config;

        private const string EmailListCacheKey = AppConstant.EmailListCacheKey;

        public CacheService(IDistributedCache cache, IConfiguration config, ILogger<CacheService> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger;
            _config = config;
        }

        public async void AddEmailToListAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Attempted to add null or empty email to cache list");
                return;
            }

            try
            {
                var cachedList = await _cache.GetStringAsync(EmailListCacheKey);
                List<string> emailList = string.IsNullOrEmpty(cachedList)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(cachedList) ?? new List<string>();

                if (!emailList.Contains(email, StringComparer.OrdinalIgnoreCase))
                {
                    emailList.Add(email);
                    var updatedValue = JsonSerializer.Serialize(emailList);

                    var options = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                    };

                    await _cache.SetStringAsync(EmailListCacheKey, updatedValue, options);
                    _logger.LogInformation("Added email to list and updated cache: {Email}", email);
                }
                else
                {
                    _logger.LogDebug("Email already exists in cache: {Email}", email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding email to cache list");
                throw;
            }
        }

        public async Task<List<string>> GetCachedEmailListAsync()
        {
            try
            {
                var cachedList = await _cache.GetStringAsync(EmailListCacheKey);
                if (string.IsNullOrEmpty(cachedList))
                {
                    _logger.LogDebug("No cached email list found under key: {Key}", EmailListCacheKey);
                    return new List<string>();
                }

                var emailList = JsonSerializer.Deserialize<List<string>>(cachedList) ?? new List<string>();
                _logger.LogInformation("Successfully retrieved {Count} cached emails", emailList.Count);
                return emailList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving cached email list");
                throw;
            }
        }
    }
}
