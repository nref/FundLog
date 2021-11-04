namespace FundLog.Model;

public abstract class ExternalAuth
{
  /// <summary>
  /// ID of the institution that this authorization is for
  /// </summary>
  public Guid Id { get; set; }
}