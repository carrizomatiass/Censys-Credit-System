using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditSystem.IntegrationTests
{
    public class CreditApplicationsApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public CreditApplicationsApiTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateApplication_ShouldReturn202Accepted()
        {
            var request = new
            {
                CustomerName = "Maria Garcia",
                CustomerId = "CUST-100",
                RequestedAmount = 25000m
            };

            var response = await _client.PostAsJsonAsync("/api/creditapplications", request);

            response.StatusCode.Should().Be(HttpStatusCode.Accepted);

            var body = await response.Content.ReadFromJsonAsync<CreateResponse>();
            body!.Id.Should().NotBeEmpty();
            body.Status.Should().Be("Created");
        }

        [Fact]
        public async Task CreateApplication_ShouldReturnBadRequest_WhenInvalidData()
        {
            var request = new
            {
                CustomerName = "",
                CustomerId = "CUST-100",
                RequestedAmount = -1000m
            };

            var response = await _client.PostAsJsonAsync("/api/creditapplications", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetApplication_ShouldReturn404_WhenNotFound()
        {
            var id = Guid.NewGuid();

            var response = await _client.GetAsync($"/api/creditapplications/{id}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateAndGet_ShouldReturnApplicationWithStatus()
        {
            //create
            var request = new
            {
                CustomerName = "Carlos Lopez",
                CustomerId = "CUST-200",
                RequestedAmount = 50000m
            };

            var createResponse = await _client.PostAsJsonAsync("/api/creditapplications", request);
            var created = await createResponse.Content.ReadFromJsonAsync<CreateResponse>();

            //background worker procese
            await Task.Delay(8000);

            //llamdo get
            var getResponse = await _client.GetAsync($"/api/creditapplications/{created!.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var application = await getResponse.Content.ReadFromJsonAsync<ApplicationResponse>();
            application!.CustomerName.Should().Be("Carlos Lopez");
            application.RequestedAmount.Should().Be(50000m);
            application.Status.Should().Be("Approved");
            application.Steps.Should().NotBeEmpty();
        }

        [Fact]
        public async Task CreateAndGet_ShouldRejectLowAmount()
        {
            var request = new
            {
                CustomerName = "Ana Torres",
                CustomerId = "CUST-300",
                RequestedAmount = 500m
            };

            var createResponse = await _client.PostAsJsonAsync("/api/creditapplications", request);
            var created = await createResponse.Content.ReadFromJsonAsync<CreateResponse>();

            await Task.Delay(3000);

            var getResponse = await _client.GetAsync($"/api/creditapplications/{created!.Id}");
            var application = await getResponse.Content.ReadFromJsonAsync<ApplicationResponse>();

            application!.Status.Should().Be("EligibilityFailed");
        }


        [Fact]
        public async Task CreateAndGet_ShouldRejectHighRiskAmount()
        {
            var request = new
            {
                CustomerName = "Roberto Diaz",
                CustomerId = "CUST-400",
                RequestedAmount = 2_000_000m
            };

            var createResponse = await _client.PostAsJsonAsync("/api/creditapplications", request);
            var created = await createResponse.Content.ReadFromJsonAsync<CreateResponse>();

            await Task.Delay(8000);

            var getResponse = await _client.GetAsync($"/api/creditapplications/{created!.Id}");
            var application = await getResponse.Content.ReadFromJsonAsync<ApplicationResponse>();

            application!.Status.Should().Be("RiskRejected");
        }

        //dtos deserializacion de response
        private record CreateResponse(Guid Id, string Status);

        private record ApplicationResponse(
            Guid Id,
            string CustomerName,
            string CustomerId,
            decimal RequestedAmount,
            string Status,
            DateTime CreatedAt,
            DateTime? UpdatedAt,
            List<StepResponse> Steps
        );

        private record StepResponse(
            Guid Id,
            string StepType,
            bool IsCompleted,
            bool HasFailed,
            string? ErrorMessage,
            int RetryCount,
            DateTime StartedAt,
            DateTime? CompletedAt
        );
    }
}
