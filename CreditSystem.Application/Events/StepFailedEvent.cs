using CreditSystem.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditSystem.Application.Events
{
    public record StepFailedEvent (Guid CreditApplicationId, ProcessStepType StepId, string ErrorMessage) : INotification;
}
