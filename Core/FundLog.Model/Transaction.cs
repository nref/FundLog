namespace FundLog.Model;

public class Transaction : Model
{
  // Dapper doesn't support DateOnly. https://github.com/DapperLib/Dapper/issues/1715
  public DateTime Date { get; set; } = DateTime.Now;
  public decimal Amount { get; set; }
  public bool? Pending { get; set; }
  public Institution? Institution { get; set; } = new Institution { Name = "None" };
  public Category? Category { get; set; } = new Category { Name = "None" };
  public string ExternalId { get; set; } = "";

  // UI
  public bool EditingDate { get; set; }
  public bool EditingAmount { get; set; }
  public bool EditingInstitution { get; set; }

  public override bool Equals(object? obj) => obj is Transaction t && Id == t.Id;

  /// <summary>
  /// Test Equality by property comparison
  /// </summary>
  public bool Matches(Transaction t) =>
    Date == t.Date &&
    Amount == t.Amount &&
    Institution?.Name == t.Institution?.Name &&
    Category?.Name == t.Category?.Name;

  public override int GetHashCode() => HashCode.Combine(Date.ToString("yyyy-mm-DD"), $"${Amount}:#.00", Name, Institution?.Name, Category?.Name);

  public override string ToString() => $"{Date} {Amount} {Name}";
}