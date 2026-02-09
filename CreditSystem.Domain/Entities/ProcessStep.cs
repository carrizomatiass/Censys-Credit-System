using CreditSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditSystem.Domain.Entities
{
    public class ProcessStep
    {
        public Guid Id { get; private set; }
        public Guid CreditApplicationId { get; private set; }
        public ProcessStepType StepType { get; private set; }
        public bool IsCompleted { get; private set; }
        public bool HasFailed { get; private set; }
        public string? ErrorMessage { get; private set; }
        public int RetryCount { get; private set; }
        public DateTime StartedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }

        private ProcessStep() { }

        public static ProcessStep Create(Guid creditApplicationId, ProcessStepType stepType)
        {
            return new ProcessStep
            {
                Id = Guid.NewGuid(),
                CreditApplicationId = creditApplicationId,
                StepType = stepType,
                IsCompleted = false,
                HasFailed = false,
                RetryCount = 0,
                StartedAt = DateTime.UtcNow
            };
        }

        public void MarkAsCompleted()
        {
            IsCompleted = true;
            HasFailed = false;
            CompletedAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string errorMessage)
        {
            HasFailed = true;
            ErrorMessage = errorMessage;
            CompletedAt = DateTime.UtcNow;
        }

        public void IncrementRetry()
        {
            RetryCount++;
            HasFailed = false;
            ErrorMessage = null;
            CompletedAt = null;
        }
    }
}
