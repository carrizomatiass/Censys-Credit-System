using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditSystem.Application.Commands.CreateCreditApplication
{
    public record CreateCreditApplicationCommand(string CustomerName,string CustomerId,decimal RequestedAmount) : IRequest<Guid>;
}
