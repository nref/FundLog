namespace FundLog.Model.Events;

public class LinkCreateRequestedEvent : Event
{
  public Guid InstitutionId { get; set; }
  public string LinkToken { get; set; } = string.Empty;
}