namespace FundLog.Api.Features.Transactions.Actions;

public class LinkCreateEffect : FluxEffect
{
  public Guid Id { get; set; }
  public string? LinkToken { get; set; }
}