using FundLog.Model;
using FundLog.Model.Events;
using System.Collections.Concurrent;

namespace FundLog.Domain;

public interface ITransactionService
{
  Task<bool> AddTransactionAsync(Transaction t);
  Task<bool> UpdateTransactionAsync(Transaction t);
  Task<bool> DeleteTransactionAsync(Guid id);
  Task<List<Transaction>> GetTransactionsAsync();
  Task<List<Transaction>> SyncTransactionsAsync();
  Task<List<Transaction>> SyncTransactionsAsync(ExternalAuth auth);

  Task<bool> AddInstitutionAsync(Institution inst);
  Task<Institution> GetInstitutionAsync(string name);
  Task<Institution> GetInstitutionAsync(Guid id);

  Task<bool> AddExternalAuthAsync(ExternalAuth externalAuth);
  Task<ExternalAuth> GetExternalAuthAsync(Guid id);
}

public class TransactionService : ITransactionService
{
  private readonly ITransactionRepo _repo;
  private readonly ITransactionAdapter _adapter;
  private readonly IEventAgg _agg;
  private readonly ConcurrentDictionary<Guid, Transaction> _cache = new();

  public TransactionService(ITransactionRepo repo, ITransactionAdapter adapter, IEventAgg agg)
  {
    _repo = repo;
    _adapter = adapter;
    _agg = agg;
  }

  public async Task<bool> AddTransactionAsync(Transaction t)
  {
    t.Id = Guid.NewGuid();

    if (t.Institution != null)
    {
      t.Institution = await GetInstitutionAsync(t.Institution.Name);
    }

    if (await _repo.AddTransactionAsync(t))
    {
      _cache[t.Id] = t;

      _agg.Publish(new TransactionAddedEvent { Transaction = t });
      return true;
    }

    return false;
  }

  public async Task<bool> UpdateTransactionAsync(Transaction t)
  {
    bool ok = await _repo.UpdateTransactionAsync(t);
    if (ok)
    {
      _cache[t.Id] = t;
    }
    return ok;
  }

  public async Task<bool> DeleteTransactionAsync(Guid id)
  {
    bool ok = await _repo.DeleteTransactionIdMapAsync(id) 
      && await _repo.DeleteTransactionAsync(id);

    if (ok)
    {
      _cache.TryRemove(id, out _);
      _agg.Publish(new TransactionDeletedEvent { Id = id });
      return true;
    }

    return false;
  }

  public async Task<List<Transaction>> GetTransactionsAsync()
  {
    if (_cache.Count == 0)
    {
      await HydrateCache();
    }

    return _cache.Values.ToList();
  }

  public async Task<List<Transaction>> SyncTransactionsAsync()
  {
    IEnumerable<Institution> insts = await _repo.GetInstitutionsAsync();

    var ts = new List<Transaction>();

    foreach (var inst in insts)
    {
      PlaidAuth auth = await _repo.GetPlaidAuthAsync(inst.Id);
      if (auth == null)
      {
        continue;
      }

      ts.AddRange(await SyncTransactionsAsync(auth));
    }

    return ts;
  }

  /// <summary>
  /// Given the received external IDs, get internal IDs
  /// </summary>
  private async Task<Dictionary<string, Guid>> MapIds(IEnumerable<string> externalIds)
  {
    IEnumerable<KeyValuePair<string, Guid>> tmp = (await externalIds
      .SelectAsync(async id => new KeyValuePair<string, Guid>(id, await _repo.GetTransactionIdMapAsync(id)), 1))
      .Where(tup => tup.Value != Guid.Empty);

    return new(tmp);
  }

  /// <summary>
  /// Set the internal ID of the given transactions.
  /// 
  /// Return the modified transactions.
  /// </summary>
  private void FillIds(IEnumerable<Transaction> transactions, Dictionary<string, Guid> ids)
  {
    IEnumerable<Transaction> nonorphans = transactions.Where(t => ids.ContainsKey(t.ExternalId));

    foreach (var t in nonorphans)
    {
      t.Id = ids[t.ExternalId];
    }
  }

  /// <summary>
  /// Find orphaned transactions, i.e. transactions without an external ID. 
  /// Usually, these were added before ID mapping was implemented.
  /// Do this by comparing properties e.g. Amount, Date, Name.
  /// 
  /// <para/>
  /// Then, sync the internal and external IDs, i.e. set the orphans' external IDs and populate the given ID map with the orphans' internal IDs.
  /// 
  /// <para/>
  /// Return the former orphans.
  /// </summary>
  private IEnumerable<Transaction> Deorphan(IEnumerable<Transaction> transactions, Dictionary<string, Guid> ids)
  {
    IEnumerable<Transaction> orphans = transactions.Where(t => !ids.ContainsKey(t.ExternalId));
    IEnumerable<(Transaction, Transaction)> matches = orphans
      .Select(t => (t, _cache.FirstOrDefault(cached => cached.Value.Matches(t)).Value))
      .Where(tup => tup.Item2 != null);

    foreach (var tup in matches)
    {
      Transaction t = tup.Item1;
      Transaction? match = tup.Item2;

      Log.Info($"Deorphaned transaction \"{t}\". Internal ID => External ID: {match.Id} => {t.ExternalId}");
      t.Id = match.Id;
      ids[t.ExternalId] = match.Id;
      match.ExternalId = t.ExternalId;
    }

    return orphans;
  }

  private async Task SaveIds(IEnumerable<Transaction> transactions)
  {
    foreach (var t in transactions)
    {
      await _repo.AddTransactionIdMapAsync(t.Id, t.ExternalId);
    }
  }

  public async Task<List<Transaction>> SyncTransactionsAsync(ExternalAuth auth)
  {
    Log.Debug($"{nameof(SyncTransactionsAsync)}({auth.Id})");

    List<Transaction> transactions = await _adapter.GetTransactionsAsync(auth);

    Dictionary<string, Guid> ids = await MapIds(transactions.Select(t => t.ExternalId));

    FillIds(transactions, ids);
    var orphans = Deorphan(transactions, ids);
    await SaveIds(orphans);

    List<Transaction> toAdd = transactions
      .Where(t => !ids.ContainsKey(t.ExternalId))
      .ToList();

    List<Transaction> toUpdate = transactions
      .Where(t => ids.ContainsKey(t.ExternalId))
      .Where(t => !_cache[ids[t.ExternalId]].Matches(t))
      .ToList();

    // Save new transactions
    if (toAdd.Count > 0)
    {
      Log.Info($"Found {toAdd.Count} new transactions: ");
      Log.Info($"  {string.Join("\n  ", toAdd)}");
    }

    foreach (Transaction t in toAdd)
    {
      await AddTransactionAsync(t);
    }

    // Update existing transactions
    if (toUpdate.Count > 0)
    {
      Log.Info($"Found {toUpdate.Count} updated transactions: ");
      Log.Info($"  {string.Join("\n  ", toUpdate)}");
    }

    foreach (Transaction t in toUpdate)
    {
      await UpdateTransactionAsync(t);
    }

    return transactions;
  }

  public async Task<bool> AddInstitutionAsync(Institution inst)
  {
    if (inst.Id == Guid.Empty)
    {
      inst.Id = Guid.NewGuid();
    }

    await _repo.AddInstitutionAsync(inst);
    await _adapter.CreateLinkAsync(inst.Id);

    return true;
  }

  public async Task<Institution> GetInstitutionAsync(string name) => await _repo.GetInstitutionAsync(name);
  public async Task<Institution> GetInstitutionAsync(Guid id) => await _repo.GetInstitutionAsync(id);

  public async Task<bool> AddExternalAuthAsync(ExternalAuth externalAuth)
  {
    if (externalAuth is PlaidAuth pa && pa.PublicToken != null && pa.AccessToken == null)
    {
      pa.AccessToken = await _adapter.GetAccessTokenAsync(pa.PublicToken);
    }
    return await _repo.AddExternalAuthAsync(externalAuth);
  }

  public async Task<bool> UpdateExternalAuthAsync(ExternalAuth externalAuth) => await _repo.UpdateExternalAuthAsync(externalAuth);
  public async Task<ExternalAuth> GetExternalAuthAsync(Guid id) => await _repo.GetPlaidAuthAsync(id);

  private async Task HydrateCache()
  {
    IEnumerable<Transaction> all = await _repo.GetTransactionsAsync();

    foreach (Transaction t in all)
    {
      _cache[t.Id] = t;
    }
  }
}