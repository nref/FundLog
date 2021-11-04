using Fluxor;
using FundLog.Api.Features.Accounts.Effects;
using FundLog.Api.Shared;

namespace FundLog.Api.Features.Accounts.Store;

public class AccountEffects
{
  private readonly IFundLogClient _client;

  public AccountEffects(IFundLogClient client)
  {
    _client = client;
  }

  [EffectMethod]
  public async Task AddAccount(AccountAddEffect effect, IDispatcher dispatcher)
  {
    await _client.AddInstitutionAsync(new Model.Institution
    {
      Name = effect.Name,
    });
  }
}