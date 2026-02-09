using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditSystem.Domain.Enums
{
    public enum ApplicationStatus
    {
        Created = 0,
        InProgress = 1,
        EligibilityFailed = 2,
        RiskRejected = 3,
        Approved = 4,
        Rejected = 5,
        Faulted = 6,
        RetryPending = 7
    }
}
