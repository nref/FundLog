namespace FundLog.Model.Events;

public class TransactionAddedEvent : Event
{
  public Transaction? Transaction { get; set; }
}
