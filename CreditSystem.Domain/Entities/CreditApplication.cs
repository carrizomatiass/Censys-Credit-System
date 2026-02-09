using CreditSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditSystem.Domain.Entities
{
    public class CreditApplication
    {
        public Guid Id { get; private set; }
        public string CustomerName { get; private set; } = string.Empty;
        public string CustomerId { get; private set; } = string.Empty;
        public decimal RequestedAmount { get; private set; }
        public ApplicationStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        private readonly List<ProcessStep> _steps = new();
        public IReadOnlyCollection<ProcessStep> Steps => _steps.AsReadOnly();

        private CreditApplication() { }

        public static CreditApplication Create(string customerName, string customerId, decimal requestedAmount)
        {
            return new CreditApplication
            {
                Id = Guid.NewGuid(),
                CustomerName = customerName,
                CustomerId = customerId,
                RequestedAmount = requestedAmount,
                Status = ApplicationStatus.Created,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void UpdateStatus(ApplicationStatus newStatus)
        {
            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddStep(ProcessStep step)
        {
            _steps.Add(step);
        }

        public ProcessStep? GetCurrentStep()
        {
            return _steps
                .OrderByDescending(s => s.StartedAt)
                .FirstOrDefault();
        }

        public ProcessStep? GetStepByType(ProcessStepType stepType)
        {
            return _steps.FirstOrDefault(s => s.StepType == stepType);
        }

    }
}
