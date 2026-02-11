using CreditSystem.Domain.Entities;
using CreditSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditSystem.Application.Interfaces
{
    public interface IProcessStepExecutor
    {
        ProcessStepType StepType { get; }
        Task<StepResult> ExecuteAsync(CreditApplication application, CancellationToken cancellationToken = default);
    }
    public record StepResult(bool Success, string? ErrorMessage = null);

}
