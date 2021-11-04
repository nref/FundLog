namespace FundLog.Api.Features.Transactions;

public enum TransactionLoadingState
{
  Init,
  Sync,
  Load,
  Idle
}