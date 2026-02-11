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
    public class CreditConditionsCalculationStep : IProcessStepExecutor
    {
        public ProcessStepType StepType => ProcessStepType.CreditConditionsCalculation;

        public async Task<StepResult> ExecuteAsync(CreditApplication application, CancellationToken cancellationToken)
        {
            await Task.Delay(600, cancellationToken);

            //Simula condiciones 
            return new StepResult(true);
        }
    }
}
