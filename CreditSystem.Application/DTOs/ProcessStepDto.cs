using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditSystem.Application.DTOs
{
    public class ProcessStepDto
    {
        public Guid Id { get; set; }
        public string StepType { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public bool HasFailed { get; set; }
        public string? ErrorMessage { get; set; }
        public int RetryCount { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
