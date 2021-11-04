namespace FundLog.Data.Entities;

public class TransactionIdMap
{
  public Guid InternalId { get; set; }
  public string ExternalId { get; set; } = "";
}