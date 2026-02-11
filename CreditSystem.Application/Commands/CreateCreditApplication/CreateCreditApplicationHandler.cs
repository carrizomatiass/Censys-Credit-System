using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CreditSystem.Domain.Entities;
using CreditSystem.Domain.Interfaces;


namespace CreditSystem.Application.Commands.CreateCreditApplication
{
    public class CreateCreditApplicationHandler : IRequestHandler<CreateCreditApplicationCommand, Guid>
    {
        private readonly ICreditApplicationRepository _repository;

        public CreateCreditApplicationHandler(ICreditApplicationRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> Handle(CreateCreditApplicationCommand request, CancellationToken cancellationToken)
        {
            var application = CreditApplication.Create(
                request.CustomerName,
                request.CustomerId,
                request.RequestedAmount
            );

            await _repository.AddAsync(application, cancellationToken);

            return application.Id;
        }
    }
}
