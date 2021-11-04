using FundLog.Api.Shared;
using FundLog.Domain;
using FundLog.Model;
using FundLog.Model.Events;

namespace FundLog.Client;

public class LocalFundLogClient : FundLogClient, IFundLogClient
{
  private readonly ITransactionService _service;
  private readonly IEventAgg _agg;

  public LocalFundLogClient(ITransactionService service, IEventAgg agg)
  {
    _service = service;
    _agg = agg;

    var sub = _agg.Published.Subscribe(e =>
    {
      Action a = e switch
      {
        _ when e is TransactionAddedEvent t => () => _ = HandleTransactionAdded(t.Transaction!),
        _ when e is TransactionDeletedEvent t => () => _ = HandleTransactionDeleted(t.Id),
        _ when e is LinkCreateRequestedEvent lcr => () => _ = HandleLinkCreateRequested(lcr),
        _ => () => { },
      };

      a();
    });
    IsConnected = true;
  }

  public async Task<bool> AddTransactionAsync(Transaction t) => await _service.AddTransactionAsync(t);
  public async Task<bool> UpdateTransactionAsync(Transaction t) => await _service.UpdateTransactionAsync(t);
  public async Task<bool> DeleteTransactionAsync(Guid id) => await _service.DeleteTransactionAsync(id);
  public async Task<List<Transaction>> GetTransactionsAsync() => await _service.GetTransactionsAsync().ConfigureAwait(false);
  public async Task<List<Transaction>> SyncTransactionsAsync() => await _service.SyncTransactionsAsync().ConfigureAwait(false);
  public async Task<bool> AddInstitutionAsync(Institution inst) => await _service.AddInstitutionAsync(inst).ConfigureAwait(false);
  public async Task<bool> AddExternalAuthAsync(PlaidAuth auth) => await _service.AddExternalAuthAsync(auth).ConfigureAwait(false);
  public Task<bool> LoginAsync(Login login) => Task.FromResult(true);
}