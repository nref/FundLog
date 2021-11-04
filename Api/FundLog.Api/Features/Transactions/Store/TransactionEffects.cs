using Fluxor;
using FundLog.Api.Features.Transactions.Actions;
using FundLog.Api.Features.Transactions.Effects;
using FundLog.Api.JsInterop;
using FundLog.Api.Shared;
using FundLog.Model;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;

namespace FundLog.Api.Features.Transactions.Store;

public class TransactionEffects
{
  private readonly IFundLogClient _client;
  private readonly IJSRuntime _js;
  private readonly ProtectedLocalStorage _storage;
  private readonly string _storageKey = "90494173-606a-48f2-90f2-65a5d2e93359";

  public TransactionEffects(IFundLogClient client, IJSRuntime js, ProtectedLocalStorage storage)
  {
    _client = client;
    _js = js;
    _storage = storage;
  }

  [EffectMethod(typeof(TransactionsGetEffect))]
  public async Task GetTransactions(IDispatcher dispatcher)
  {
    dispatcher.Dispatch(new TransactionsGetAction());
    List<Transaction> transactions = await _client.GetTransactionsAsync();
    dispatcher.Dispatch(new TransactionsGetFinishAction(transactions));
  }

  [EffectMethod(typeof(TransactionsSyncEffect))]
  public async Task SyncTransactions(IDispatcher dispatcher)
  {
    dispatcher.Dispatch(new TransactionsSyncAction());
    List<Transaction> anew = await _client.SyncTransactionsAsync();
    dispatcher.Dispatch(new TransactionsSyncFinishAction(anew));
  }

  [EffectMethod]
  public async Task DeleteTransaction(TransactionDeleteEffect effect, IDispatcher dispatcher)
  {
    //if (!await _js.InvokeAsync<bool>("confirm", $"Delete '{effect.Transaction}' ?"))
    //{
    //  return;
    //}

    await _client.DeleteTransactionAsync(effect.Transaction.Id);
  }

  [EffectMethod]
  public async Task AddTransaction(TransactionAddEffect effect, IDispatcher dispatcher)
  {
    await _client.AddTransactionAsync(effect.Transaction);
  }

  [EffectMethod]
  public async Task AddTransaction(TransactionUpdateEffect effect, IDispatcher dispatcher)
  {
    await _client.UpdateTransactionAsync(effect.Transaction);
  }

  [EffectMethod]
  public async Task Persist(TransactionsPersistEffect action, IDispatcher dispatcher)
  {
    try
    {
      await _storage.SetAsync(_storageKey, action.State);
    }
    catch (Exception e)
    {
      Log.Error(e);
    }
  }

  [EffectMethod(typeof(TransactionsLoadEffect))]
  public async Task Load(IDispatcher dispatcher)
  {
    try
    {
      var state = await _storage.GetAsync<TransactionState>(_storageKey);

      if (state.Success && state.Value != null)
      {
        dispatcher.Dispatch(new TransactionsLoadedAction(state.Value));
      }
    }
    catch (Exception e)
    {
      Log.Error(e);
    }
  }

  [EffectMethod]
  public async Task LinkCreate(LinkCreateEffect effect, IDispatcher dispatcher)
  {
    // Given a link token, we get a public token.
    // Using the public token, we can get an access token.
    await _js.InvokeMethod("plaidCreate", effect.LinkToken)
      .WithCallback<LinkCreatedAction.Args>(args =>
      {
        _client.AddExternalAuthAsync(new PlaidAuth
        {
          Id = effect.Id,
          PublicToken = args.PublicToken,
        });

        //_client.NotifyLinkCreated(linkToken, publicToken); // TODO tell client
        dispatcher.Dispatch(new LinkCreatedAction(args));
      });
  }
}