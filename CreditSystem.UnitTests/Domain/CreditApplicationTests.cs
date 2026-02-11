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
    public class CreditApplicationTests
    {
        [Fact]
        public void Create_ShouldSetStatusToCreated()
        {
            var application = CreditApplication.Create("John Doe", "CUST-001", 50000);

            application.Status.Should().Be(ApplicationStatus.Created);
            application.CustomerName.Should().Be("John Doe");
            application.CustomerId.Should().Be("CUST-001");
            application.RequestedAmount.Should().Be(50000);
            application.Id.Should().NotBeEmpty();
            application.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void UpdateStatus_ShouldChangeStatusAndSetUpdatedAt()
        {
            var application = CreditApplication.Create("John Doe", "CUST-001", 50000);

            application.UpdateStatus(ApplicationStatus.InProgress);

            application.Status.Should().Be(ApplicationStatus.InProgress);
            application.UpdatedAt.Should().NotBeNull();
        }

        [Fact]
        public void AddStep_ShouldAddToStepsCollection()
        {
            var application = CreditApplication.Create("John Doe", "CUST-001", 50000);
            var step = ProcessStep.Create(application.Id, ProcessStepType.EligibilityValidation);

            application.AddStep(step);

            application.Steps.Should().HaveCount(1);
            application.Steps.First().StepType.Should().Be(ProcessStepType.EligibilityValidation);
        }

        [Fact]
        public void GetCurrentStep_ShouldReturnMostRecentStep()
        {
            var application = CreditApplication.Create("John Doe", "CUST-001", 50000);
            var step1 = ProcessStep.Create(application.Id, ProcessStepType.EligibilityValidation);
            var step2 = ProcessStep.Create(application.Id, ProcessStepType.RiskEvaluation);

            application.AddStep(step1);
            application.AddStep(step2);

            var current = application.GetCurrentStep();
            current.Should().NotBeNull();
            current!.StepType.Should().Be(ProcessStepType.RiskEvaluation);
        }

        [Fact]
        public void GetStepByType_ShouldReturnCorrectStep()
        {
            var application = CreditApplication.Create("John Doe", "CUST-001", 50000);
            var step = ProcessStep.Create(application.Id, ProcessStepType.RiskEvaluation);
            application.AddStep(step);

            var found = application.GetStepByType(ProcessStepType.RiskEvaluation);

            found.Should().NotBeNull();
            found!.Id.Should().Be(step.Id);
        }

        [Fact]
        public void GetStepByType_ShouldReturnNull_WhenStepNotExists()
        {
            var application = CreditApplication.Create("John Doe", "CUST-001", 50000);

            var found = application.GetStepByType(ProcessStepType.FinalDecision);

            found.Should().BeNull();
        }
    }
}
