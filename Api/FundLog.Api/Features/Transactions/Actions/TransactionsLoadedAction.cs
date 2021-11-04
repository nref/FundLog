namespace FundLog.Api.Features.Transactions.Actions;

public class TransactionsLoadedAction : FluxAction
{
  public TransactionState State { get; set; }
  public TransactionsLoadedAction(TransactionState state) => State = state;
}