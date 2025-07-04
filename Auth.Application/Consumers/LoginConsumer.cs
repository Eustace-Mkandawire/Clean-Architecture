using Auth.Domain.Contracts;
using Auth.Domain.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Application.Commands
{
    public sealed class LoginConsumer : IConsumer<LoginCreatedContract>
    {
        private readonly ICacheService _cacheRepository;
        private readonly ILogger<LoginConsumer> _logger;
        public LoginConsumer(ICacheService cacheRepository, ILogger<LoginConsumer> logger)
        {
            _cacheRepository = cacheRepository ?? throw new ArgumentNullException(nameof(cacheRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task Consume(ConsumeContext<LoginCreatedContract> context)
        {
            _cacheRepository.AddEmailToListAsync(context.Message.Email);
            _logger.LogInformation($"Consumer: {typeof(LoginConsumer)}\n Message: {context.Message.Email}");
            return Task.CompletedTask;
        }
    }
}
