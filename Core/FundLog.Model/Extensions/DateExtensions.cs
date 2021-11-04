using System.Globalization;

namespace FundLog.Model.Extensions;

public static class DateExtensions
{
  public static string Short(this DateTime dt) => dt.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
}
