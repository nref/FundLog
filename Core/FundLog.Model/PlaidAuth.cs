namespace FundLog.Model;

public class PlaidAuth : ExternalAuth
{
  public string? PublicToken { get; set; }
  public string? AccessToken { get; set; }
}