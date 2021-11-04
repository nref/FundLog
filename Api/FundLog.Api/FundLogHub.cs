using FundLog.Api.Shared;
using FundLog.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

public class FundLogHub : Hub<IFundLogCallback>, IFundLogHub, IDisposable
{
  private readonly IFundLogApi _api;
  private readonly SignInManager<IdentityUser> _signInManager;
  private readonly UserManager<IdentityUser> _userManager;

  public FundLogHub(IFundLogApi api, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
  {
    _api = api;
    _signInManager = signInManager;
    _userManager = userManager;
  }

  public async Task<bool> AddTransactionAsync(Transaction t) => await _api.AddTransactionAsync(t);
  public async Task<bool> UpdateTransactionAsync(Transaction t) => await _api.UpdateTransactionAsync(t);
  public async Task<bool> DeleteTransactionAsync(Guid id) => await _api.DeleteTransactionAsync(id);

  //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  //[Authorize]
  public async Task<List<Transaction>> GetTransactionsAsync() => await _api.GetTransactionsAsync().ConfigureAwait(false);
  public async Task<List<Transaction>> SyncTransactionsAsync() => await _api.SyncTransactionsAsync().ConfigureAwait(false);
  public async Task<bool> AddInstitutionAsync(Institution inst) => await _api.AddInstitutionAsync(inst).ConfigureAwait(false);
  public async Task<bool> AddExternalAuthAsync(PlaidAuth auth) => await _api.AddExternalAuth(auth).ConfigureAwait(false);

  public async Task<bool> LoginAsync(Login login)
  {
    try
    {
      var user = await _userManager.FindByEmailAsync(login.Email);
      bool ok = await _userManager.CheckPasswordAsync(user, login.Password);

      return ok;
    }
    catch (Exception e)
    {
      Log.Error(e);
      return false;
    }
  }
}
