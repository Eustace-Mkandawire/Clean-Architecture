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
    public sealed class RegisterConsumer : IConsumer<RegisterCreatedContract>
    {
        private readonly ICacheService _cacheRepository;
        private readonly ILogger<RegisterConsumer> _logger;

        public RegisterConsumer(ICacheService cacheRepository, ILogger<RegisterConsumer> logger)
        {
            _cacheRepository = cacheRepository;
            _logger = logger;
        }

        public Task Consume(ConsumeContext<RegisterCreatedContract> context)
        {
            _cacheRepository.AddEmailToListAsync(context.Message.Email);
            _logger.LogInformation($"Consumer : {typeof(RegisterConsumer)}\n  Message : {context.Message.Email} ");
            return Task.CompletedTask;
        }
    }
}
