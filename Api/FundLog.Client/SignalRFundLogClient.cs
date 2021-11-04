using FundLog.Api.Shared;
using FundLog.Model;
using FundLog.Model.Events;
using Microsoft.AspNetCore.SignalR.Client;

namespace FundLog.Client;

public class SignalRFundLogClient : FundLogClient, IFundLogClient
{
  private bool _isConnected => _conn?.State == HubConnectionState.Connected;

  private readonly HubConnection _conn;
  private readonly CancellationTokenSource _cts = new CancellationTokenSource();

  public SignalRFundLogClient(HubConnection conn)
  {
    _conn = conn;
    _conn.On<LinkCreateRequestedEvent>(nameof(HandleLinkCreateRequested), async e => await HandleLinkCreateRequested(e));
    _conn.On<Transaction>(nameof(HandleTransactionAdded), async t => await HandleTransactionAdded(t));
    _conn.On<Guid>(nameof(HandleTransactionDeleted), async id => await HandleTransactionDeleted(id));

    _ = Task.Run(async () =>
    {
      while (!_cts.IsCancellationRequested)
      {
        IsConnected = _isConnected;
        await Task.Delay(1000);
      }
    });
  }

  public async Task<bool> AddTransactionAsync(Transaction t) => await _conn
    .InvokeAsync<bool>(nameof(AddTransactionAsync), t)
    .ConfigureAwait(false);

  public async Task<bool> UpdateTransactionAsync(Transaction t) => await _conn
    .InvokeAsync<bool>(nameof(UpdateTransactionAsync), t)
    .ConfigureAwait(false);

  public async Task<bool> DeleteTransactionAsync(Guid id) => await _conn
    .InvokeAsync<bool>(nameof(DeleteTransactionAsync), id)
    .ConfigureAwait(false);

  public async Task<List<Transaction>> GetTransactionsAsync() =>  await _conn
    .InvokeAsync<List<Transaction>>(nameof(GetTransactionsAsync))
    .ConfigureAwait(false);

  public async Task<List<Transaction>> SyncTransactionsAsync() => await _conn
    .InvokeAsync<List<Transaction>>(nameof(SyncTransactionsAsync))
    .ConfigureAwait(false);

  public async Task<bool> AddInstitutionAsync(Institution inst) => await _conn
    .InvokeAsync<bool>(nameof(AddInstitutionAsync), inst)
    .ConfigureAwait(false);

  public async Task<bool> AddExternalAuthAsync(PlaidAuth auth) => await _conn
    .InvokeAsync<bool>(nameof(AddExternalAuthAsync), auth)
    .ConfigureAwait(false);

  public async Task<bool> LoginAsync(Login login) => await _conn.InvokeAsync<bool>(nameof(LoginAsync), login).ConfigureAwait(false);
}
