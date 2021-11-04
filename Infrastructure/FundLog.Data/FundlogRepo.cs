using Dapper;
using FundLog.Data.Entities;
using FundLog.Data.Mappers;
using FundLog.Domain;
using Npgsql;
using System.Data;

namespace FundLog.Data;

public class FundlogRepo : ITransactionRepo
{
  private readonly string _connectionString;
  private readonly NpgsqlConnection _db;

  public FundlogRepo(string connectionString)
  {
    _connectionString = connectionString;

    _db = new NpgsqlConnection(_connectionString);
  }

  public async Task<bool> AddTransactionAsync(Model.Transaction t, CancellationToken ct = default)
  {
    var sql = "INSERT INTO public.transaction(" +
      "id, date, name, amount, institution, category, pending) " +
      $"VALUES('{t.Id}', '{FormattableString.Invariant($"{t.Date}")}', '{t.Name.Sanitize()}', {FormattableString.Invariant($"{t.Amount * 100}")}, '{t.Institution?.Id ?? Guid.Empty}', '{t.Category?.Id ?? Guid.Empty}', {t.Pending}); ";

    return await QueryAsync(sql, ct);
  }

  public async Task<bool> UpdateTransactionAsync(Model.Transaction t, CancellationToken ct = default)
  {
    var sql = $"UPDATE public.transaction " +
      $"SET amount = '{(int)(t.Amount * 100M)}', name = '{t.Name.Sanitize()}' " +
      $"WHERE id = '{t.Id}' ";

    return await QueryAsync(sql, ct);
  }

  public async Task<bool> DeleteTransactionAsync(Guid id, CancellationToken ct = default)
  {
    var sql = $"DELETE FROM public.transaction WHERE id = '{id}'";
    return await QueryAsync(sql, ct);
  }

  public async Task<IEnumerable<Model.Transaction>> GetTransactionsAsync(CancellationToken ct = default) => await RequireConnection(async () =>
  {
    var sql = "SELECT * FROM transaction t " +
      "LEFT JOIN institution i ON t.institution = i.id " +
      "LEFT JOIN category c ON t.category = c.id " +
      "LEFT JOIN categorygroup cg ON c.group = cg.id " +
      "LEFT JOIN transactionidmap map ON t.id = map.id ";

    var cmd = new CommandDefinition(sql, cancellationToken: ct);

    return await _db.QueryAsync<
      Transaction, 
      Institution, 
      Category, 
      CategoryGroup, 
      TransactionIdMap, 
      Model.Transaction>(cmd, (t, i, c, cg, map) => t.Map(i, c, cg, map)!);
  });

  public async Task<IEnumerable<Model.Allocation>> GetAllocationsAsync(CancellationToken ct = default) => await RequireConnection(async () =>
  {
    var sql = "SELECT * FROM allocation a " +
      "LEFT JOIN category cfrom ON a.from = cfrom.id " +
      "LEFT JOIN category cto ON a.to = cto.id " +
      "LEFT JOIN categorygroup cgfrom ON cfrom.group = cgfrom.id " +
      "LEFT JOIN categorygroup cgto   ON cto.group = cgto.id ";

    var cmd = new CommandDefinition(sql, cancellationToken: ct);
    return await _db.QueryAsync<Allocation, Category, Category, CategoryGroup, CategoryGroup, Model.Allocation>(cmd, (a, cfrom, cto, cgfrom, cgto) => a.Map(cfrom, cto, cgfrom, cgto)!);
  });

  public async Task<IEnumerable<Model.Institution>> GetInstitutionsAsync(CancellationToken ct = default)
  {
    var sql = $"SELECT * FROM public.institution";
    var cmd = new CommandDefinition(sql, cancellationToken: ct);
    return await _db.QueryAsync<Model.Institution>(cmd);
  }

  public async Task<Model.Institution> GetInstitutionAsync(string name, CancellationToken ct = default)
  {
    var sql = $"SELECT * FROM public.institution WHERE Name = '{name}'";
    return await QuerySingleOrDefaultAsync<Model.Institution>(sql, ct);
  }

  public async Task<Model.Institution> GetInstitutionAsync(Guid id, CancellationToken ct = default)
  {
    var sql = $"SELECT * FROM public.institution WHERE id = '{id}'";
    return await QuerySingleOrDefaultAsync<Model.Institution>(sql, ct);
  }

  public async Task<bool> AddInstitutionAsync(Model.Institution inst, CancellationToken ct = default)
  {
    var sql = "INSERT INTO public.institution(id, name) " +
      $"VALUES('{inst.Id}', '{inst.Name}'); ";
    return await QueryAsync(sql, ct);
  }

  public async Task<Model.PlaidAuth> GetPlaidAuthAsync(Guid id, CancellationToken ct = default)
  {
    var sql = $"SELECT * FROM public.plaidauth WHERE id = '{id}'";
    return await QuerySingleOrDefaultAsync<Model.PlaidAuth>(sql, ct);
  }

  public async Task<bool> UpdateExternalAuthAsync(Model.ExternalAuth auth, CancellationToken ct = default) => auth switch
  {
    _ when auth is Model.PlaidAuth pa => await UpdateAuthAsync(pa, ct),
    _ => false,
  };

  public async Task<bool> UpdateAuthAsync(Model.PlaidAuth auth, CancellationToken ct = default)
  {
    try
    {
      var sql = $"UPDATE public.plaidauth " +
        $"SET accesstoken = '{auth.AccessToken}'" +
        $"WHERE id = '{auth.Id}' ";

      var cmd = new CommandDefinition(sql, cancellationToken: ct);
      await _db.QueryAsync(cmd);
      return true;
    }
    catch (Exception e)
    {
      Model.Log.Error(e);
      return false;
    }
  }

  public async Task<bool> AddExternalAuthAsync(Model.ExternalAuth auth, CancellationToken ct = default) => auth switch
  {
    _ when auth is Model.PlaidAuth pa => await AddExternalAuthAsync(pa, ct),
    _ => false,
  };

  public async Task<bool> AddExternalAuthAsync(Model.PlaidAuth auth, CancellationToken ct = default)
  {
    try
    {
      var sql = "INSERT INTO public.plaidauth(id, accesstoken) " +
        $"VALUES('{auth.Id}', '{auth.AccessToken}'); ";

      var cmd = new CommandDefinition(sql, cancellationToken: ct);
      await _db.QueryAsync(cmd);
      return true;
    }
    catch (Exception e)
    {
      Model.Log.Error(e);
      return false;
    }
  }

  public async Task<bool> AddTransactionIdMapAsync(Guid internalId, string externalId, CancellationToken ct = default)
  {
    var sql = "INSERT INTO public.transactionidmap(externalid, id) " +
      $"VALUES('{externalId.Sanitize()}', '{internalId}'); ";

    return await QueryAsync(sql, ct);
  }

  public async Task<string> GetTransactionMapAsync(Guid internalId, CancellationToken ct = default)
  {
    var sql = $"SELECT externalid FROM public.transactionidmap WHERE id = '{internalId}'";
    return await QuerySingleOrDefaultAsync<string>(sql, ct);
  }

  public async Task<Guid> GetTransactionIdMapAsync(string externalId, CancellationToken ct = default)
  {
    var sql = $"SELECT id FROM public.transactionidmap WHERE externalid = '{externalId}'";
    return await QuerySingleOrDefaultAsync<Guid>(sql, ct);
  }

  public async Task<bool> DeleteTransactionIdMapAsync(Guid internalId, CancellationToken ct = default)
  {
    var sql = $"DELETE FROM public.transactionidmap WHERE id = '{internalId}'";
    return await QueryAsync(sql, ct);
  }

  private async Task<T> RequireConnection<T>(Func<Task<T>> f)
  {
    if (_db.State != ConnectionState.Open)
    {
      await _db.OpenAsync();
    }

    return await f();
  }

  private async Task<bool> QueryAsync(string sql, CancellationToken ct = default)
  {
    try
    {
      var cmd = new CommandDefinition(sql, cancellationToken: ct);
      await _db.QueryAsync(cmd);
      return true;
    }
    catch (Exception e)
    {
      Model.Log.Error(e);
      return false;
    }
  }

  private async Task<T> QuerySingleOrDefaultAsync<T>(string sql, CancellationToken ct = default)
  {
    try
    {
      var cmd = new CommandDefinition(sql, cancellationToken: ct);
      return await _db.QuerySingleOrDefaultAsync<T>(cmd);
    }
    catch (Exception e)
    {
      Model.Log.Error(e);
      throw;
    }
  }
}
