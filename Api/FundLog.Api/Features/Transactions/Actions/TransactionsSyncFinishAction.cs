using FundLog.Model;

namespace FundLog.Api.Features.Transactions.Actions;

public class TransactionsSyncFinishAction : FluxAction
{
  public List<Transaction> Transactions { get; }

  public TransactionsSyncFinishAction(List<Transaction> transactions)
  {
    Transactions = transactions;
  }
}