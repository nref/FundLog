using FundLog.Model;
using FundLog.Model.Events;
using System.Reactive.Subjects;

namespace FundLog.Client;

public abstract class FundLogClient
{
  public bool IsConnected
  {
    get => _isConnected;
    set
    {
      if (_isConnected == value)
      {
        return;
      }
      _isConnected = value;
      _connected.OnNext(value);
    }
  }
  private bool _isConnected;

  public IObservable<LinkCreateRequestedEvent> LinkCreateRequested => _linkCreateRequested;
  public IObservable<Transaction> TransactionAdded => _transactionAdded;
  public IObservable<Guid> TransactionDeleted => _transactionDeleted;
  public IObservable<object> Connected => _connected;

  protected readonly ISubject<LinkCreateRequestedEvent> _linkCreateRequested = new Subject<LinkCreateRequestedEvent>();
  protected readonly ISubject<Transaction> _transactionAdded = new Subject<Transaction>();
  protected readonly ISubject<Guid> _transactionDeleted = new Subject<Guid>();
  protected readonly ISubject<object> _connected = new ReplaySubject<object>(1);

  public Task HandleTransactionAdded(Transaction t)
  {
    _transactionAdded.OnNext(t);
    return Task.CompletedTask;
  }

  public Task HandleTransactionDeleted(Guid id)
  {
    _transactionDeleted.OnNext(id);
    return Task.CompletedTask;
  }

  public Task HandleLinkCreateRequested(LinkCreateRequestedEvent req)
  {
    _linkCreateRequested.OnNext(req);
    return Task.CompletedTask;
  }
}
