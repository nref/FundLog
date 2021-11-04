namespace FundLog.Data.Entities;

public class Transaction
{
  public Guid Id { get; set; }
  // Dapper doesn't support DateOnly. https://github.com/DapperLib/Dapper/issues/1715
  public DateTime Date { get; set; }
  public string Name { get; set; } = "";
  public int Amount { get; set; }
  public Guid Institution { get; set; }
  public Guid Category { get; set; }
  public bool Pending { get; set; }
};