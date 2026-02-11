using CreditSystem.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditSystem.Application.Events
{
    public record StepCompletedEvent(Guid CreditApplicationId,ProcessStepType StepType) : INotification;
}
