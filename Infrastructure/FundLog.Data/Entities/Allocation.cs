namespace FundLog.Data.Entities;

public class Allocation
{
  public Guid Id { get; set; }
  public Guid From { get; set; }
  public Guid To { get; set; }
  public DateTime Created { get; set; }
  public DateTime ForMonth { get; set; }
}
