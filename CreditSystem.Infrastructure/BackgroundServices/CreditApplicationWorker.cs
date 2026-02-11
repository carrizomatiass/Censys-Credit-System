using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CreditSystem.Application.Orchestrator;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace CreditSystem.Infrastructure.BackgroundServices
{
    public class CreditApplicationWorker : BackgroundService
    {
        private readonly Channel<Guid> _channel;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CreditApplicationWorker> _logger;

        public CreditApplicationWorker(
            Channel<Guid> channel,
            IServiceScopeFactory scopeFactory,
            ILogger<CreditApplicationWorker> logger)
        {
            _channel = channel;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CreditApplicationWorker started.");

            await foreach (var applicationId in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    _logger.LogInformation("Processing application {ApplicationId}.", applicationId);

                    using var scope = _scopeFactory.CreateScope();
                    var orchestrator = scope.ServiceProvider.GetRequiredService<CreditApplicationOrchestrator>();

                    await orchestrator.ProcessApplicationAsync(applicationId, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing application {ApplicationId}.", applicationId);
                }
            }
        }
    }
}
