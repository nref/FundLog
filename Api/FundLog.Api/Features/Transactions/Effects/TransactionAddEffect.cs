using FundLog.Model;

namespace FundLog.Api.Features.Transactions.Effects;

public class TransactionAddEffect : FluxEffect
{
  public Transaction Transaction { get; }

  public TransactionAddEffect(Transaction transaction)
  {
    Transaction = transaction;
  }
}