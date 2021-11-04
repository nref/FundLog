namespace FundLog.Api.Features.Transactions.Actions;

public class TransactionDeleteAction : FluxAction
{
  public Guid Id { get; set; }
}