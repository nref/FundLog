namespace FundLog.Api.Features.Transactions.Actions;

public class LinkCreatedAction : FluxAction
{
  public class Args
  {
    public string LinkToken { get; set; } = "";
    public string PublicToken { get; set; } = "";
  }
 
  public Args Data { get; set; }

  public LinkCreatedAction(Args data)
  {
    Data = data;
  }
}