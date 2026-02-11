using CreditSystem.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditSystem.Application.Queries.GetCreditApplication
{
    public record GetCreditApplicationQuery(Guid Id) : IRequest<CreditApplicationDto?>;
}
