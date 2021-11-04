using FundLog.Model;

namespace FundLog.Api.Features.Transactions.Effects;

public class TransactionDeleteEffect : FluxEffect
{
  public Transaction Transaction { get; }

  public TransactionDeleteEffect(Transaction transaction)
  {
    Transaction = transaction;
  }
}