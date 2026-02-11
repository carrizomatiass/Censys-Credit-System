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
    public class EligibilityValidationStepTests
    {
        private readonly EligibilityValidationStep _step = new();

        [Fact]
        public async Task Execute_ShouldSucceed_WhenAmountIsValid()
        {
            var application = CreditApplication.Create("John Doe", "CUST-001", 5000);

            var cancellationToken = new CancellationTokenSource().Token;
            var result = await _step.ExecuteAsync(application, cancellationToken);

            result.Success.Should().BeTrue();
            result.ErrorMessage.Should().BeNull();
        }
    }
}
