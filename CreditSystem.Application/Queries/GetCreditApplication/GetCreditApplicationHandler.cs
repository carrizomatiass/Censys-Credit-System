using CreditSystem.Application.DTOs;
using CreditSystem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditSystem.Application.Queries.GetCreditApplication
{
    public class GetCreditApplicationHandler : IRequestHandler<GetCreditApplicationQuery, CreditApplicationDto?>
    {
        private readonly ICreditApplicationRepository _repository;

        public GetCreditApplicationHandler(ICreditApplicationRepository repository)
        {
            _repository = repository;
        }

        public async Task<CreditApplicationDto?> Handle(GetCreditApplicationQuery request, CancellationToken cancellationToken)
        {
            var application = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (application is null)
                return null;

            return new CreditApplicationDto
            {
                Id = application.Id,
                CustomerName = application.CustomerName,
                CustomerId = application.CustomerId,
                RequestedAmount = application.RequestedAmount,
                Status = application.Status.ToString(),
                CreatedAt = application.CreatedAt,
                UpdatedAt = application.UpdatedAt,
                Steps = application.Steps.Select(s => new ProcessStepDto
                {
                    Id = s.Id,
                    StepType = s.StepType.ToString(),
                    IsCompleted = s.IsCompleted,
                    HasFailed = s.HasFailed,
                    ErrorMessage = s.ErrorMessage,
                    RetryCount = s.RetryCount,
                    StartedAt = s.StartedAt,
                    CompletedAt = s.CompletedAt
                }).ToList()
            };
        }
    }
}
