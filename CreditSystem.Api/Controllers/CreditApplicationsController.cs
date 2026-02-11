using CreditSystem.Api.Contracts;
using CreditSystem.Application.Commands.CreateCreditApplication;
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
                return BadRequest("Invalid request data.");
            }

            var command = new CreateCreditApplicationCommand(
                request.CustomerName,
                request.CustomerId,
                request.RequestedAmount
            );

            var applicationId = await _mediator.Send(command, cancellationToken);

            // Encola para procesamiento en background
            await _channel.Writer.WriteAsync(applicationId, cancellationToken);

            return AcceptedAtAction(
                nameof(GetById),
                new { id = applicationId },
                new { id = applicationId, status = "Created" }
            );
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetCreditApplicationQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            if (result is null)
                return NotFound(new { message = $"Credit application {id} not found." });

            return Ok(result);
        }
    }
}
