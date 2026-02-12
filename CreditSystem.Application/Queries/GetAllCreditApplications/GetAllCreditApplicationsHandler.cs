using CreditSystem.Application.DTOs;
using CreditSystem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditSystem.Application.Queries.GetAllCreditApplications
{
    public class GetAllCreditApplicationsHandler : IRequestHandler<GetAllCreditApplicationsQuery, IReadOnlyList<CreditApplicationDto>>
    {
        private readonly ICreditApplicationRepository _repository;

        public GetAllCreditApplicationsHandler(ICreditApplicationRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<CreditApplicationDto>> Handle(GetAllCreditApplicationsQuery request, CancellationToken cancellationToken)
        {
            var applications = await _repository.GetAllAsync(cancellationToken);

            return applications.Select(app => new CreditApplicationDto
            {
                Id = app.Id,
                CustomerName = app.CustomerName,
                CustomerId = app.CustomerId,
                RequestedAmount = app.RequestedAmount,
                Status = app.Status.ToString(),
                CreatedAt = app.CreatedAt,
                UpdatedAt = app.UpdatedAt,
                Steps = app.Steps.Select(s => new ProcessStepDto
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
            }).ToList();
        }
    }
}
