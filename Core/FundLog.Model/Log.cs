namespace FundLog.Model;

public static class Log
{
  public static void Info(object? o) => Console.WriteLine($"{DateTime.UtcNow} [INFO] {o}");
  public static void Error(object? o) => Console.WriteLine($"{DateTime.UtcNow} [ERROR] {o}");
  public static void Debug(object? o) => Console.WriteLine($"{DateTime.UtcNow} [DEBUG] {o}");
}