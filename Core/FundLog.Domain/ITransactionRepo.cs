using FundLog.Model;

namespace FundLog.Domain;

public interface ITransactionRepo
{
  Task<bool> AddTransactionAsync(Transaction transaction, CancellationToken ct = default);
  Task<IEnumerable<Transaction>> GetTransactionsAsync(CancellationToken ct = default);
  Task<bool> UpdateTransactionAsync(Transaction transaction, CancellationToken ct = default);
  Task<bool> DeleteTransactionAsync(Guid id, CancellationToken ct = default);

  Task<IEnumerable<Allocation>> GetAllocationsAsync(CancellationToken ct = default);

  Task<bool> AddInstitutionAsync(Institution inst, CancellationToken ct = default);
  Task<IEnumerable<Institution>> GetInstitutionsAsync(CancellationToken ct = default);
  Task<Institution> GetInstitutionAsync(string name, CancellationToken ct = default);
  Task<Institution> GetInstitutionAsync(Guid id, CancellationToken ct = default);

  Task<bool> AddExternalAuthAsync(ExternalAuth auth, CancellationToken ct = default);
  Task<PlaidAuth> GetPlaidAuthAsync(Guid id, CancellationToken ct = default);
  Task<bool> UpdateExternalAuthAsync(ExternalAuth auth, CancellationToken ct = default);

  Task<bool> AddTransactionIdMapAsync(Guid internalId, string externalId, CancellationToken ct = default);
  Task<string> GetTransactionMapAsync(Guid internalId, CancellationToken ct = default);
  Task<Guid> GetTransactionIdMapAsync(string externalId, CancellationToken ct = default);
  Task<bool> DeleteTransactionIdMapAsync(Guid internalId, CancellationToken ct = default);
}
