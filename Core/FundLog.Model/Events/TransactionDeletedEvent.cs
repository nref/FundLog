namespace FundLog.Model.Events;

public class TransactionDeletedEvent : Event
{
  public Guid Id { get; set; }
}