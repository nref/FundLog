using FundLog.Domain;
using FundLog.Model;
using FundLog.Model.Events;
using FundLog.Model.Extensions;
using Going.Plaid;
using Going.Plaid.Entity;
using Going.Plaid.Institutions;
using Going.Plaid.Item;
using Going.Plaid.Link;
using Going.Plaid.Transactions;
using System.Diagnostics;
using Transaction = FundLog.Model.Transaction;

namespace PlaidAdapter;

public class PlaidAdapter : ITransactionAdapter
{
  private readonly PlaidClient _client;
  private readonly IEventAgg _agg;

  private LinkTokenCreateRequest _LinkTokenCreateRequest => new()
  {
    ClientName = "PlaidAdapter",
    CountryCodes = new List<CountryCode> { CountryCode.Us },
    User = new LinkTokenCreateRequestUser
    {
      ClientUserId = "0",
      LegalName = "John Doe",
      PhoneNumber = "+11231231234",
      PhoneNumberVerifiedTime = DateTimeOffset.UnixEpoch,
      EmailAddress = "fake@fake.com",
      EmailAddressVerifiedTime = DateTimeOffset.UnixEpoch,
    },
    Products = new List<Products> { Products.Transactions, Products.Auth },
  };

  public PlaidAdapter(PlaidClient client, IEventAgg agg)
  {
    _client = client;
    _agg = agg;
  }

  public async Task<List<Transaction>> GetTransactionsAsync(ExternalAuth auth) => auth switch
  {
    _ when auth is PlaidAuth pa => await GetTransactionsAsync(pa),
    _ => await Task.FromResult(new List<Transaction>()),
  };

  public async Task<List<Transaction>> GetTransactionsAsync(PlaidAuth pa)
  {
    if (pa.AccessToken == null)
    {
      return new List<Transaction>();
    }

    try
    {
      TransactionsGetResponse result = await GetAsync(pa.AccessToken);

      if (!await Validate(result, pa.AccessToken))
      {
        return new List<Transaction>();
      }

      return result.Transactions.Select(t => new Transaction
      {
        Amount = t.Amount,
        Institution = new FundLog.Model.Institution
        {
          Name = result.Accounts.FirstOrDefault(acc => acc.AccountId == t.AccountId)?.Name ?? "None"
        },
        Name = t.Name ?? "",
        Date = t.Date.ToDateTime(default),
        Pending = t.Pending,
        ExternalId = t.TransactionId,
      }).ToList();
    }
    catch (Exception e)
    {
      Log.Error(e);
      return new List<Transaction>();
    }

  }

  private async Task<bool> Validate(TransactionsGetResponse result, string accessToken)
  {
    if ((result?.Transactions) != null && result.IsSuccessStatusCode)
    {
      return true;
    }

    if (result?.Exception == null)
    {
      return true;
    }

    Log.Error(result.Exception.ErrorMessage);

    if (result.Exception.ErrorType != ErrorType.ItemError
     || result.Exception.ErrorCode != ErrorCode.ItemLoginRequired)
    {
      return true;
    }

    await UpdateLinkAsync(accessToken);
    return false;
  }

  public async Task UpdateLinkAsync(string accessToken)
  {
    Log.Debug($"{nameof(UpdateLinkAsync)}({accessToken})");

    LinkTokenCreateRequest request = _LinkTokenCreateRequest
      .WithAccessToken(accessToken)
      .WithoutProducts();

    await CreateLinkAsync(Guid.Empty, request); // Guid.Empty: No new info to store for the associated insitution
  }

  public async Task CreateLinkAsync(Guid institutionId) => await CreateLinkAsync(institutionId, _LinkTokenCreateRequest);

  private async Task CreateLinkAsync(Guid institutionId, LinkTokenCreateRequest request)
  {
    Log.Debug($"{nameof(UpdateLinkAsync)}({institutionId}, {request})");

    LinkTokenCreateResponse response = await _client.LinkTokenCreateAsync(request);

    if (response?.IsSuccessStatusCode ?? false)
    {
      _agg.Publish(new LinkCreateRequestedEvent 
      { 
        InstitutionId = institutionId,
        LinkToken = response.LinkToken 
      });
    }
  }

  public async Task<string?> GetAccessTokenAsync(string publicToken)
  {
    Log.Debug($"{nameof(GetAccessTokenAsync)}({publicToken})");

    var exchange = await _client.ItemPublicTokenExchangeAsync(new()
    {
      PublicToken = publicToken,
    }).AnyContext();

    if (exchange.IsSuccessStatusCode == false && exchange.Exception != null)
    {
      Console.WriteLine(exchange.Exception.ErrorMessage);
      return null;
    }

    Log.Debug($"{nameof(GetAccessTokenAsync)}({publicToken}): Got access token {exchange.AccessToken}");
    return exchange.AccessToken;
  }

  private async Task<TransactionsGetResponse> GetAsync(string accessToken) => await _client.TransactionsGetAsync(new ()
  {
    AccessToken = accessToken,
    StartDate = DateOnly.FromDateTime(DateTime.UtcNow - TimeSpan.FromDays(30)),
    EndDate = DateOnly.FromDateTime(DateTime.UtcNow),
  });

  public async Task AddInstitution()
  {
    Console.WriteLine("Adding institution...");

    // How to link a new account:
    // 1. Create a link token. You must request the right products.
    //   For Savings/Checking accounts, e.g. USAA, need to send only transaction, auth.
    //   If you send liabilities then USAA won't show up
    LinkTokenCreateResponse created = await _client.LinkTokenCreateAsync(_LinkTokenCreateRequest);

    Console.WriteLine($"Got link token {created.LinkToken}");

    // 2. With the link token, use the Plaid Link flow to select and authenticate with the institution.
    //   1. Open index.html in editor
    //   2. Paste in the link token to the "token" parameter
    //   3. Save index.html
    //   4. Open index.html in a web browser
    //   5. Search and select institution, follow steps to authenticate.
    //   6. Find onSuccess in developer tools log,
    //      Find public_token, e.g. "public-development-asdf"
    string path = @$"{Directory.GetCurrentDirectory()}\index.html";
    Process.Start(@"cmd.exe ", @$"/c {path}");

    Console.WriteLine($"In the opened browser window, search and select your institution, then follow the steps to authenticate");
    Console.WriteLine($"When authentication is successful, find the onSuccess message in the developer tools log (Ctrl+Shift+J in Chrome).");
    Console.Write($"Find public_token and paste it here: ");
    string? publicToken = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(publicToken))
    {
      Console.WriteLine($"Invalid public token");
      return;
    }

    // e.g. publicToken = "public-development-asdf";
    Console.WriteLine($"Got public token '{publicToken}'");

    // 3. Exchange public token for an access token
    var exchange = await _client.ItemPublicTokenExchangeAsync(new()
    {
      PublicToken = publicToken,
    }).AnyContext();

    if (exchange.IsSuccessStatusCode == false && exchange.Exception != null)
    {
      Console.WriteLine(exchange.Exception.ErrorMessage);
      return;
    }

    Console.WriteLine($"Got access token {exchange.AccessToken}");

    // Get institution ID
    ItemGetResponse item = await _client.ItemGetAsync(new()
    {
      AccessToken = exchange.AccessToken
    });

    // Using ID, Get institution name
    InstitutionsGetByIdResponse inst = await _client.InstitutionsGetByIdAsync(new()
    {
      InstitutionId = item.Item?.InstitutionId ?? "",
      CountryCodes = new List<CountryCode> { CountryCode.Us }
    });

    // Get transactions
    await GetTransactionsAsync(new PlaidAuth
    {
      AccessToken = exchange.AccessToken,
    });
  }
}
