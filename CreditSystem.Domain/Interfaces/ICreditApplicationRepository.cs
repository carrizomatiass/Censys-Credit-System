using CreditSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditSystem.Domain.Interfaces
{
    public interface ICreditApplicationRepository
    {
        Task<CreditApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(CreditApplication application, CancellationToken cancellationToken = default);
        Task UpdateAsync(CreditApplication application, CancellationToken cancellationToken = default);
        Task AddStepAsync(ProcessStep step, CancellationToken cancellationToken = default);
    }
}
