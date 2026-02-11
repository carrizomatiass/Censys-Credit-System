using CreditSystem.Application.Steps;
using CreditSystem.Domain.Entities;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditSystem.UnitTests.Steps
{
    public class RiskEvaluationStepTests
    {
        private readonly RiskEvaluationStep _step = new();

        [Fact]
        public async Task Execute_ShouldSucceed_WhenAmountWithinLimit()
        {
            var application = CreditApplication.Create("John Doe", "CUST-001", 500000);

            var result = await _step.ExecuteAsync(application, CancellationToken.None);

            result.Success.Should().BeTrue();

        }

        [Fact]
        public async Task Execute_ShouldFail_WhenAmountExceedsMaximum()
        {
            var application = CreditApplication.Create("John Doe", "CUST-001", 2_000_000);

            var result = await _step.ExecuteAsync(application, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("maximum risk threshold");
        }
    }
}
