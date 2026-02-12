using CreditSystem.Api.Contracts;
using CreditSystem.Application.Commands.CreateCreditApplication;
using CreditSystem.Application.Commands.RetryCreditApplication;
using CreditSystem.Application.Queries.GetAllCreditApplications;
using CreditSystem.Application.Queries.GetCreditApplication;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;

namespace CreditSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CreditApplicationsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly Channel<Guid> _channel;

        public CreditApplicationsController(IMediator mediator, Channel<Guid> channel)
        {
            _mediator = mediator;
            _channel = channel;
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateCreditApplicationRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.CustomerName) ||
                string.IsNullOrWhiteSpace(request.CustomerId) ||
                request.RequestedAmount <= 0)
            {
                return BadRequest("Los datos de la solicitud no son validos");
            }

            var command = new CreateCreditApplicationCommand(
                request.CustomerName,
                request.CustomerId,
                request.RequestedAmount
            );

            var applicationId = await _mediator.Send(command, cancellationToken);

            //encola procesamiento en background
            await _channel.Writer.WriteAsync(applicationId, cancellationToken);

            return AcceptedAtAction(
                nameof(GetById),
                new { id = applicationId },
                new { id = applicationId, status = "Created" }
            );
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var query = new GetAllCreditApplicationsQuery();
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetCreditApplicationQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            if (result is null)
                return NotFound(new { message = $"Solicitud de credito {id} no encontrada" });

            return Ok(result);
        }

        [HttpPost("{id:guid}/retry")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Retry(Guid id, CancellationToken cancellationToken)
        {
            var command = new RetryCreditApplicationCommand(id);
            var result = await _mediator.Send(command, cancellationToken);

            if (!result)
            {
                var application = await _mediator.Send(new GetCreditApplicationQuery(id), cancellationToken);

                if (application is null)
                    return NotFound(new { message = $"Solicitud de credito {id} no encontrada" });

                return Conflict(new { message = $"La solicitud no se puede reintentar en estado '{application.Status}'." });
            }

            //re encolar para procesamiento en background
            await _channel.Writer.WriteAsync(id, cancellationToken);

            return AcceptedAtAction(
                nameof(GetById),
                new { id },
                new { id, status = "RetryPending" }
            );
        }
    }
}
