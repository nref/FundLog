namespace FundLog.Data.Entities;

public class Category
{
  public Guid Id { get; set; }
  public Guid Group { get; set; }
  public string Name { get; set; } = "";
}
