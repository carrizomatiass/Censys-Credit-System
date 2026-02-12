using CreditSystem.Domain.Enums;
using CreditSystem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditSystem.Application.Commands.RetryCreditApplication
{
    public class RetryCreditApplicationHandler : IRequestHandler<RetryCreditApplicationCommand, bool>
    {
        private readonly ICreditApplicationRepository _repository;

        public RetryCreditApplicationHandler(ICreditApplicationRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(RetryCreditApplicationCommand request, CancellationToken cancellationToken)
        {
            var application = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (application is null)
                return false;

            //solo se puede reintentar si esta en un estado de fallo
            var retryableStatuses = new[]
            {
            ApplicationStatus.Faulted,
            ApplicationStatus.EligibilityFailed,
            ApplicationStatus.RiskRejected,
            ApplicationStatus.Rejected
        };

            if (!retryableStatuses.Contains(application.Status))
                return false;

            application.UpdateStatus(ApplicationStatus.RetryPending);
            await _repository.UpdateAsync(application, cancellationToken);

            return true;
        }
    }
}
