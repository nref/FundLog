using Fluxor;

namespace FundLog.Api.Features.Transactions.Store;

public class TransactionFeature : Feature<TransactionState>
{
  public override string GetName() => "Transactions";
  protected override TransactionState GetInitialState() => new TransactionState();
}

