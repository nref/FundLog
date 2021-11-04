using FundLog.Model;

namespace FundLog.Domain;

public interface ITransactionAdapter
{
  Task<List<Transaction>> GetTransactionsAsync(ExternalAuth auth);
  Task AddInstitution();

  /// <summary>
  /// Invoke Link Create flow in update mode, given an existing access token.
  /// https://plaid.com/docs/link/update-mode/
  /// 
  /// <para/>
  /// Typical use case is the access token is invalid but can become valid if the user re-authorizes.
  /// 
  /// <para/>
  /// On success, raises an event with the new link token which should be passed to the Plaid auth JS script.
  /// 
  /// <para/>
  /// There is nothing to store after the link flow completes; the given token becomes valid again.
  /// </summary>
  Task UpdateLinkAsync(string accessToken);

  /// <summary>
  /// Start a Plaid Link flow. The results e.g. access token will be associated with the given institution.
  /// </summary>
  Task CreateLinkAsync(Guid institutionId);

  /// <summary>
  /// Exchange public token for an access token
  /// </summary>
  Task<string?> GetAccessTokenAsync(string publicToken);
}
