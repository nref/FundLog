namespace FundLog.Data;

public static class StringExtensions
{
  public static string Sanitize(this string s) => s.Replace("\'", "\'\'");
}
