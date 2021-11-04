using Fluxor;
using FundLog.Api.Features.Transactions.Actions;
using FundLog.Model.Extensions;
using System.Collections.Concurrent;

namespace FundLog.Api.Features.Transactions.Store;

public static class TransactionReducers
{
  [ReducerMethod(typeof(TransactionsSyncAction))]
  public static TransactionState OnSync(TransactionState state) => new(state)
  {
    State = state.State switch
    {
      TransactionLoadingState.Idle => TransactionLoadingState.Sync,
      _ => state.State,
    }
  };

  [ReducerMethod]
  public static TransactionState OnSyncFinish(TransactionState state, TransactionsSyncFinishAction action) => new(state)
  {
    State = state.State switch
    {
      TransactionLoadingState.Sync => TransactionLoadingState.Idle,
      _ => state.State,
    },
    Transactions = state.Transactions.AddRange(action.Transactions, t => t.Id)
  };

  [ReducerMethod(typeof(TransactionsGetAction))]
  public static TransactionState OnGet(TransactionState state) => new(state)
  {
    State = state.State switch
    {
      TransactionLoadingState.Init => TransactionLoadingState.Load,
      TransactionLoadingState.Idle => TransactionLoadingState.Load,
      _ => state.State,
    }
  };

  [ReducerMethod]
  public static TransactionState OnGetFinish(TransactionState state, TransactionsGetFinishAction action) => new(state)
  {
    State = state.State switch
    {
      TransactionLoadingState.Load => TransactionLoadingState.Idle,
      _ => state.State,
    },
    Transactions = new ConcurrentDictionary<Guid, Model.Transaction>().AddRange(action.Transactions, t => t.Id)
  };

  [ReducerMethod]
  public static TransactionState OnAdd(TransactionState state, TransactionAddAction action) => new(state)
  {
    Transactions = state.Transactions.Add(action.Transaction, t => t.Id),
  };

  [ReducerMethod]
  public static TransactionState OnDelete(TransactionState state, TransactionDeleteAction action) => new(state)
  {
    Transactions = state.Transactions.Remove(action.Id),
  };

  /// <summary>
  /// Called when state has been loaded from local storage
  /// </summary>
  [ReducerMethod]
  public static TransactionState OnLoaded(TransactionState state, TransactionsLoadedAction action) => new(action.State)
  {
    Transactions = state.Transactions.IsEmpty
      ? state.Transactions.AddRange(action.State.Transactions.Values, t => t.Id)
      : state.Transactions,
  };
}