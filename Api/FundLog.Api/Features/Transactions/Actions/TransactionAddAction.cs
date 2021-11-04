using FundLog.Model;

namespace FundLog.Api.Features.Transactions.Actions;

public class TransactionAddAction : FluxAction
{
  public Transaction Transaction { get; set; } = new();
}
