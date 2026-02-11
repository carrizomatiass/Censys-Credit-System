using CreditSystem.Application.Interfaces;
using CreditSystem.Domain.Entities;
using CreditSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditSystem.Application.Steps
{
    public class RiskEvaluationStep : IProcessStepExecutor
    {
        public ProcessStepType StepType => ProcessStepType.RiskEvaluation;

        public async Task<StepResult> ExecuteAsync(CreditApplication application, CancellationToken cancellationToken)
        {
            await Task.Delay(800, cancellationToken);

            //Simula riesgo basada en monto
            if (application.RequestedAmount > 1_000_000)
                return new StepResult(false, "Amount exceeds maximum risk threshold.");

            return new StepResult(true);
        }
    }
}
