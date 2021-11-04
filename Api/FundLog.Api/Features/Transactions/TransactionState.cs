using FundLog.Model;
using System.Collections.Concurrent;

namespace FundLog.Api.Features.Transactions;

public class TransactionState
{
  public TransactionLoadingState State { get; init; } = TransactionLoadingState.Init;
  public ConcurrentDictionary<Guid, Transaction> Transactions { get; init; } = new();

  public TransactionState() { }
  public TransactionState(TransactionState other)
  {
    State = other.State;
    Transactions = other.Transactions;
  }
}
