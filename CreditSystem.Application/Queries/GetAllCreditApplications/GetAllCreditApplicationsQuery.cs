using CreditSystem.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditSystem.Application.Queries.GetAllCreditApplications
{
    public record GetAllCreditApplicationsQuery : IRequest<IReadOnlyList<CreditApplicationDto>>;
}
