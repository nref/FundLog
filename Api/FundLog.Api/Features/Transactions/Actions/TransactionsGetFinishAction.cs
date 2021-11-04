using FundLog.Model;

namespace FundLog.Api.Features.Transactions.Actions;

public class TransactionsGetFinishAction : FluxAction
{
  public List<Transaction> Transactions { get; }

  public TransactionsGetFinishAction(List<Transaction> transactions)
  {
    Transactions = transactions;
  }

}
