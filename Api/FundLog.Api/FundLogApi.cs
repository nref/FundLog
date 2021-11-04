using FundLog.Api.Shared;
using FundLog.Domain;
using FundLog.Model;
using FundLog.Model.Events;
using Microsoft.AspNetCore.SignalR;

public interface IFundLogApi
{
  Task<bool> AddTransactionAsync(Transaction t);
  Task<bool> UpdateTransactionAsync(Transaction t);
  Task<bool> DeleteTransactionAsync(Guid id);
  Task<List<Transaction>> GetTransactionsAsync();
  Task<List<Transaction>> SyncTransactionsAsync();
  Task<bool> AddInstitutionAsync(Institution inst);
  Task<bool> AddExternalAuth(ExternalAuth auth);
}

public class FundLogApi : IFundLogApi
{
  private readonly IHubContext<FundLogHub, IFundLogCallback> _context;
  private readonly ITransactionService _transactions;
  private readonly IEventAgg _agg;

  public FundLogApi(IHubContext<FundLogHub, IFundLogCallback> context, ITransactionService transactions, IEventAgg agg)
  {
    _context = context;
    _transactions = transactions;
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
  }

  public async Task<bool> AddTransactionAsync(Transaction t) => await _transactions.AddTransactionAsync(t);
  public async Task<bool> UpdateTransactionAsync(Transaction t) => await _transactions.UpdateTransactionAsync(t);
  public async Task<bool> DeleteTransactionAsync(Guid id) => await _transactions.DeleteTransactionAsync(id);
  public async Task<List<Transaction>> GetTransactionsAsync() => await _transactions.GetTransactionsAsync().ConfigureAwait(false);
  public async Task<List<Transaction>> SyncTransactionsAsync() => await _transactions.SyncTransactionsAsync().ConfigureAwait(false);
  public async Task<bool> AddInstitutionAsync(Institution inst) => await _transactions.AddInstitutionAsync(inst).ConfigureAwait(false);
  public async Task<bool> AddExternalAuth(ExternalAuth auth) => await _transactions.AddExternalAuthAsync(auth).ConfigureAwait(false);

  public async Task HandleLinkCreateRequested(LinkCreateRequestedEvent e) => await _context.Clients.All.HandleLinkCreateRequested(e).ConfigureAwait(false);
  public async Task HandleTransactionAdded(Transaction t) => await _context.Clients.All.HandleTransactionAdded(t).ConfigureAwait(false);
  public async Task HandleTransactionDeleted(Guid id) => await _context.Clients.All.HandleTransactionDeleted(id).ConfigureAwait(false);
}
