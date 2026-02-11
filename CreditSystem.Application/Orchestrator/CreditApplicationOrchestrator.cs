using CreditSystem.Application.Events;
using CreditSystem.Application.Interfaces;
using CreditSystem.Domain.Entities;
using CreditSystem.Domain.Enums;
using CreditSystem.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditSystem.Application.Orchestrator
{
    public class CreditApplicationOrchestrator
    {
        private readonly ICreditApplicationRepository _repository;
        private readonly IEnumerable<IProcessStepExecutor> _stepExecutors;
        private readonly IMediator _mediator;
        private readonly ILogger<CreditApplicationOrchestrator> _logger;

        private static readonly ProcessStepType[] StepOrder =
        {
        ProcessStepType.EligibilityValidation,
        ProcessStepType.RiskEvaluation,
        ProcessStepType.CreditConditionsCalculation,
        ProcessStepType.FinalDecision
        };

        public CreditApplicationOrchestrator(
            ICreditApplicationRepository repository,
            IEnumerable<IProcessStepExecutor> stepExecutors,
            IMediator mediator,
            ILogger<CreditApplicationOrchestrator> logger)
        {
            _repository = repository;
            _stepExecutors = stepExecutors;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task ProcessApplicationAsync(Guid applicationId, CancellationToken cancellationToken = default)
        {
            var application = await _repository.GetByIdAsync(applicationId, cancellationToken);
            if (application is null)
            {
                _logger.LogError("Credit application {ApplicationId} not found.", applicationId);
                return;
            }

            application.UpdateStatus(ApplicationStatus.InProgress);
            await _repository.UpdateAsync(application, cancellationToken);

            foreach (var stepType in StepOrder)
            {
                var executor = _stepExecutors.FirstOrDefault(e => e.StepType == stepType);
                if (executor is null)
                {
                    _logger.LogError("No executor found for step {StepType}.", stepType);
                    application.UpdateStatus(ApplicationStatus.Faulted);
                    await _repository.UpdateAsync(application, cancellationToken);
                    return;
                }

                var processStep = ProcessStep.Create(applicationId, stepType);
                application.AddStep(processStep);

                try
                {
                    _logger.LogInformation("Executing step {StepType} for application {ApplicationId}.", stepType, applicationId);

                    var result = await executor.ExecuteAsync(application, cancellationToken);

                    if (result.Success)
                    {
                        processStep.MarkAsCompleted();
                        await _mediator.Publish(new StepCompletedEvent(applicationId, stepType), cancellationToken);
                    }
                    else
                    {
                        processStep.MarkAsFailed(result.ErrorMessage ?? "Unknown error");
                        await _mediator.Publish(new StepFailedEvent(applicationId, stepType, result.ErrorMessage ?? "Unknown error"), cancellationToken);

                        var failureStatus = stepType switch
                        {
                            ProcessStepType.EligibilityValidation => ApplicationStatus.EligibilityFailed,
                            ProcessStepType.RiskEvaluation => ApplicationStatus.RiskRejected,
                            _ => ApplicationStatus.Rejected
                        };

                        application.UpdateStatus(failureStatus);
                        await _repository.UpdateAsync(application, cancellationToken);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Step {StepType} threw an exception for application {ApplicationId}.", stepType, applicationId);
                    processStep.MarkAsFailed(ex.Message);
                    application.UpdateStatus(ApplicationStatus.Faulted);
                    await _repository.UpdateAsync(application, cancellationToken);
                    return;
                }
            }

            application.UpdateStatus(ApplicationStatus.Approved);
            await _repository.UpdateAsync(application, cancellationToken);

            _logger.LogInformation("Application {ApplicationId} approved successfully.", applicationId);
        }
    }
}
