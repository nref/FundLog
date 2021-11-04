using FundLog.Model;

namespace FundLog.Api.Features.Transactions.Effects;

public class TransactionUpdateEffect : FluxEffect
{
  public Transaction Transaction { get; }

  public TransactionUpdateEffect(Transaction transaction)
  {
    Transaction = transaction;
  }
}