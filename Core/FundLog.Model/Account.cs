namespace FundLog.Model;

public class Account
{
  public string Name { get; set; } = "";
  public List<Transaction> Transactions { get; set; } = new();

  public decimal Balance => Transactions.Sum(t => t.Amount);
}