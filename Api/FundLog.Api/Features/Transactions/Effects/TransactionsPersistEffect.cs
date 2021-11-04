namespace FundLog.Api.Features.Transactions.Effects;

/// <summary>
/// Persist to browser storage
/// </summary>
public class TransactionsPersistEffect : FluxEffect
{
  public TransactionState State { get; set; }
  public TransactionsPersistEffect(TransactionState state) => State = state;
}
