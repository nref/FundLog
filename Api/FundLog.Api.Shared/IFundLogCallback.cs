using FundLog.Model;
using FundLog.Model.Events;

namespace FundLog.Api.Shared;

public interface IFundLogCallback
{
  Task HandleTransactionAdded(Transaction t);
  Task HandleTransactionDeleted(Guid id);
  Task HandleLinkCreateRequested(LinkCreateRequestedEvent req);
}
