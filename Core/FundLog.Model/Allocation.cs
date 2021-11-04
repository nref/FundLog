using FundLog.Model.Extensions;

namespace FundLog.Model;

public class Allocation : Model
{
  public Category? From { get; set; }
  public Category? To { get; set; }
  public DateTime Created { get; set; }
  public DateTime ForMonth { get; set; }

  // UI
  public bool EditingCreated { get; set; }

  public string CreatedString => Created.Short();
  public string ForMonthString => ForMonth.Short();
  public override string ToString() => $"Transfer from {From} to {To}";
}
