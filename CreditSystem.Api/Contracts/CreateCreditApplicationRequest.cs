namespace CreditSystem.Api.Contracts
{
    public record CreateCreditApplicationRequest(string CustomerName,string CustomerId,decimal RequestedAmount);
}
