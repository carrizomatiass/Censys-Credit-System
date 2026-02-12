using CreditSystem.Application.Interfaces;
using CreditSystem.Application.Orchestrator;
using CreditSystem.Domain.Entities;
using CreditSystem.Domain.Enums;
using CreditSystem.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditSystem.UnitTests.Orchestrator
{
    public class CreditApplicationOrchestratorTests
    {
        private readonly ICreditApplicationRepository _repository;
        private readonly IMediator _mediator;
        private readonly ILogger<CreditApplicationOrchestrator> _logger;


        public CreditApplicationOrchestratorTests()
        {
            _repository = Substitute.For<ICreditApplicationRepository>();
            _mediator = Substitute.For<IMediator>();
            _logger = Substitute.For<ILogger<CreditApplicationOrchestrator>>();
        }

        private CreditApplicationOrchestrator CreateOrchestrator(IEnumerable<IProcessStepExecutor> steps)
        {
            return new CreditApplicationOrchestrator(_repository, steps, _mediator, _logger);
        }

        [Fact]
        public async Task ProcessApplication_ShouldApprove_WhenAllStepsSucceed()
        {
            // Arrange
            var application = CreditApplication.Create("John Doe", "CUST-001", 50000);
            _repository.GetByIdAsync(application.Id, Arg.Any<CancellationToken>())
                .Returns(application);

            var steps = CreateSuccessfulSteps();
            var orchestrator = CreateOrchestrator(steps);

            // Act
            await orchestrator.ProcessApplicationAsync(application.Id);

            // Assert
            application.Status.Should().Be(ApplicationStatus.Approved);
            await _repository.Received(4).AddStepAsync(Arg.Any<ProcessStep>(), Arg.Any<CancellationToken>());
            await _repository.Received().UpdateAsync(application, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ProcessApplication_ShouldSetEligibilityFailed_WhenEligibilityFails()
        {
            // Arrange
            var application = CreditApplication.Create("John Doe", "CUST-001", 50000);
            _repository.GetByIdAsync(application.Id, Arg.Any<CancellationToken>())
                .Returns(application);

            var eligibilityStep = Substitute.For<IProcessStepExecutor>();
            eligibilityStep.StepType.Returns(ProcessStepType.EligibilityValidation);
            eligibilityStep.ExecuteAsync(application, Arg.Any<CancellationToken>())
                .Returns(new StepResult(false, "Not eligible"));

            var orchestrator = CreateOrchestrator(new[] { eligibilityStep });

            // Act
            await orchestrator.ProcessApplicationAsync(application.Id);

            // Assert
            application.Status.Should().Be(ApplicationStatus.EligibilityFailed);
        }

        [Fact]
        public async Task ProcessApplication_ShouldSetRiskRejected_WhenRiskEvaluationFails()
        {
            // Arrange
            var application = CreditApplication.Create("John Doe", "CUST-001", 50000);
            _repository.GetByIdAsync(application.Id, Arg.Any<CancellationToken>())
                .Returns(application);

            var eligibilityStep = Substitute.For<IProcessStepExecutor>();
            eligibilityStep.StepType.Returns(ProcessStepType.EligibilityValidation);
            eligibilityStep.ExecuteAsync(application, Arg.Any<CancellationToken>())
                .Returns(new StepResult(true));

            var riskStep = Substitute.For<IProcessStepExecutor>();
            riskStep.StepType.Returns(ProcessStepType.RiskEvaluation);
            riskStep.ExecuteAsync(application, Arg.Any<CancellationToken>())
                .Returns(new StepResult(false, "High risk"));

            var orchestrator = CreateOrchestrator(new[] { eligibilityStep, riskStep });

            // Act
            await orchestrator.ProcessApplicationAsync(application.Id);

            // Assert
            application.Status.Should().Be(ApplicationStatus.RiskRejected);
        }

        [Fact]
        public async Task ProcessApplication_ShouldSetFaulted_WhenStepThrowsException()
        {
            // Arrange
            var application = CreditApplication.Create("John Doe", "CUST-001", 50000);
            _repository.GetByIdAsync(application.Id, Arg.Any<CancellationToken>())
                .Returns(application);

            var failingStep = Substitute.For<IProcessStepExecutor>();
            failingStep.StepType.Returns(ProcessStepType.EligibilityValidation);
            failingStep.ExecuteAsync(application, Arg.Any<CancellationToken>())
                .ThrowsAsync(new Exception("Connection timeout"));

            var orchestrator = CreateOrchestrator(new[] { failingStep });

            // Act
            await orchestrator.ProcessApplicationAsync(application.Id);

            // Assert
            application.Status.Should().Be(ApplicationStatus.Faulted);
        }



        [Fact]
        public async Task ProcessApplication_ShouldDoNothing_WhenApplicationNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repository.GetByIdAsync(id, Arg.Any<CancellationToken>())
                .Returns((CreditApplication?)null);

            var orchestrator = CreateOrchestrator(Array.Empty<IProcessStepExecutor>());

            // Act
            await orchestrator.ProcessApplicationAsync(id);

            // Assert
            await _repository.DidNotReceive().UpdateAsync(Arg.Any<CreditApplication>(), Arg.Any<CancellationToken>());
        }

        private IEnumerable<IProcessStepExecutor> CreateSuccessfulSteps()
        {
            var stepTypes = new[]
            {
            ProcessStepType.EligibilityValidation,
            ProcessStepType.RiskEvaluation,
            ProcessStepType.CreditConditionsCalculation,
            ProcessStepType.FinalDecision
        };

            return stepTypes.Select(type =>
            {
                var step = Substitute.For<IProcessStepExecutor>();
                step.StepType.Returns(type);
                step.ExecuteAsync(Arg.Any<CreditApplication>(), Arg.Any<CancellationToken>())
                    .Returns(new StepResult(true));
                return step;
            });
        }

        [Fact]
        public async Task ProcessApplication_ShouldReprocess_WhenRetryPending()
        {
            // Arrange
            var application = CreditApplication.Create("John Doe", "CUST-001", 50000);
            application.UpdateStatus(ApplicationStatus.RetryPending);

            _repository.GetByIdAsync(application.Id, Arg.Any<CancellationToken>())
                .Returns(application);

            var steps = CreateSuccessfulSteps();
            var orchestrator = CreateOrchestrator(steps);

            // Act
            await orchestrator.ProcessApplicationAsync(application.Id);

            // Assert
            application.Status.Should().Be(ApplicationStatus.Approved);
            await _repository.Received(4).AddStepAsync(Arg.Any<ProcessStep>(), Arg.Any<CancellationToken>());
        }
    }
}
