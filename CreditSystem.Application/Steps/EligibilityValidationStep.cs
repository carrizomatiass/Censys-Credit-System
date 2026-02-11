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
    public class EligibilityValidationStep : IProcessStepExecutor
    {
        public ProcessStepType StepType => ProcessStepType.EligibilityValidation;

        public async Task<StepResult> ExecuteAsync(CreditApplication application, CancellationToken cancellationToken)
        {
            //Validacion Asincronica
            await Task.Delay(500, cancellationToken);

            //Negocio: monto solicitado minimo
            if (application.RequestedAmount < 1000)
                return new StepResult(false, "Requested amount is below the minimum threshold of $1,000.");

            //Negocio: validar que el nombre del cliente no este vacio
            if (string.IsNullOrWhiteSpace(application.CustomerName))
                return new StepResult(false, "Customer name is required.");

            return new StepResult(true);
        }
    }
}
