namespace FundLog.Model;

public class Model
{
  public Guid Id { get; set; }
  public string Name { get; set; } = "";

  // UI
  public bool EditingName { get; set; }
  public bool IsSelected { get; set; }

  public override string ToString() => Name;
}