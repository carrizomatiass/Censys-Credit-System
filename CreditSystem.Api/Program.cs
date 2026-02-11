using CreditSystem.Application.Interfaces;
using CreditSystem.Application.Orchestrator;
using CreditSystem.Application.Steps;
using CreditSystem.Domain.Interfaces;
using CreditSystem.Infrastructure.BackgroundServices;
using CreditSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);

// EF Core InMemory
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("CreditSystemDb"));

// Repository
builder.Services.AddScoped<ICreditApplicationRepository, CreditApplicationRepository>();

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreditSystem.Application.Commands.CreateCreditApplication.CreateCreditApplicationCommand).Assembly));

// Process Steps
builder.Services.AddScoped<IProcessStepExecutor, EligibilityValidationStep>();
builder.Services.AddScoped<IProcessStepExecutor, RiskEvaluationStep>();
builder.Services.AddScoped<IProcessStepExecutor, CreditConditionsCalculationStep>();
builder.Services.AddScoped<IProcessStepExecutor, FinalDecisionStep>();

// Orchestrator
builder.Services.AddScoped<CreditApplicationOrchestrator>();
//Channel como cola in-memory
builder.Services.AddSingleton(Channel.CreateUnbounded<Guid>());

//Background Worker
builder.Services.AddHostedService<CreditApplicationWorker>();

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

//Tests
public partial class Program { }