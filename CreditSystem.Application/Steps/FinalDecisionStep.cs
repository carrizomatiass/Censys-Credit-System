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
    public class FinalDecisionStep : IProcessStepExecutor
    {
        public ProcessStepType StepType => ProcessStepType.FinalDecision;

        public async Task<StepResult> ExecuteAsync(CreditApplication application, CancellationToken cancellationToken)
        {
            //simula decision final async
            await Task.Delay(400, cancellationToken);

            //decision final basada en que todos los pasos previos fueron concluidos
            return new StepResult(true);
        }
    }
}
