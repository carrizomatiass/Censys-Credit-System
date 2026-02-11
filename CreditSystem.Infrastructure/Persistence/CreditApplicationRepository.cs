using CreditSystem.Domain.Entities;
using CreditSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditSystem.Infrastructure.Persistence
{
    public class CreditApplicationRepository : ICreditApplicationRepository
    {
        private readonly AppDbContext _context;

        public CreditApplicationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CreditApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.CreditApplications
                .Include(a => a.Steps)
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }

        public async Task AddAsync(CreditApplication application, CancellationToken cancellationToken = default)
        {
            await _context.CreditApplications.AddAsync(application, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(CreditApplication application, CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task AddStepAsync(ProcessStep step, CancellationToken cancellationToken = default)
        {
            //guardo directamente el step, ya que el CreditApplication se encuentra trackeado por el contexto
            await _context.ProcessSteps.AddAsync(step, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
