using FundLog.Model;
using FundLog.Model.Events;

namespace FundLog.Api.Shared;

public interface IFundLogClient : IClient, IFundLogCallback, IFundLogHub
{
  IObservable<LinkCreateRequestedEvent> LinkCreateRequested { get; }
  IObservable<Transaction> TransactionAdded { get; }
  IObservable<Guid> TransactionDeleted { get; }
  IObservable<object> Connected { get; }
}