using CreditSystem.Domain.Entities;
using CreditSystem.Domain.Enums;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditSystem.UnitTests.Domain
{
    public class ProcessStepTests
    {
        [Fact]
        public void Create_ShouldInitializeCorrectly()
        {
            var applicationId = Guid.NewGuid();
            var step = ProcessStep.Create(applicationId, ProcessStepType.EligibilityValidation);

            step.Id.Should().NotBeEmpty();
            step.CreditApplicationId.Should().Be(applicationId);
            step.StepType.Should().Be(ProcessStepType.EligibilityValidation);
            step.IsCompleted.Should().BeFalse();
            step.HasFailed.Should().BeFalse();
            step.RetryCount.Should().Be(0);
        }

        [Fact]
        public void MarkAsCompleted_ShouldSetCompletedState()
        {
            var step = ProcessStep.Create(Guid.NewGuid(), ProcessStepType.RiskEvaluation);

            step.MarkAsCompleted();

            step.IsCompleted.Should().BeTrue();
            step.HasFailed.Should().BeFalse();
            step.CompletedAt.Should().NotBeNull();
        }

        [Fact]
        public void IncrementRetry_ShouldResetFailureAndIncrementCount()
        {
            var step = ProcessStep.Create(Guid.NewGuid(), ProcessStepType.RiskEvaluation);
            step.MarkAsFailed("Temporary error");

            step.IncrementRetry();

            step.RetryCount.Should().Be(1);
            step.HasFailed.Should().BeFalse();
            step.ErrorMessage.Should().BeNull();
            step.CompletedAt.Should().BeNull();
        }
    }
}
