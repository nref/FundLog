using FundLog.Model;

namespace FundLog.Api.Shared;

public interface IFundLogHub
{
  Task<bool> AddTransactionAsync(Transaction t);
  Task<bool> UpdateTransactionAsync(Transaction t);
  Task<bool> DeleteTransactionAsync(Guid id);
  Task<List<Transaction>> GetTransactionsAsync();
  Task<List<Transaction>> SyncTransactionsAsync();
  Task<bool> AddInstitutionAsync(Institution inst);
  Task<bool> AddExternalAuthAsync(PlaidAuth auth);
  Task<bool> LoginAsync(Login login);
}
