using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditSystem.Domain.Enums
{
    public enum ProcessStepType
    {
        EligibilityValidation = 1,
        RiskEvaluation = 2,
        CreditConditionsCalculation = 3,
        FinalDecision = 4
    }
}
