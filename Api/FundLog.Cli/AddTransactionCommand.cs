using FundLog.Api.Shared;
using FundLog.Model;
using Typin;
using Typin.Attributes;
using Typin.Console;

namespace FundLog.Cli;

[Command("add")]
public class AddTransactionCommand : ICommand, IDisposable
{
  private readonly IFundLogClient _client;
  private readonly IDisposable _toDispose;

  [CommandOption(IsRequired = true)]
  public decimal Amount { get; set; }

  [CommandOption(IsRequired = true)]
  public DateTime Date { get; set; }

  [CommandOption(IsRequired = true)]
  public string Name { get; set; } = "";

  public AddTransactionCommand(IFundLogClient client)
  {
    _client = client;
    _toDispose = _client.TransactionAdded.Subscribe(t =>
    {
      Log.Info($"Transaction added: {t}");
    });
  }

  public async ValueTask ExecuteAsync(IConsole console)
  {
    while (!_client.IsConnected)
    {
      await Task.Delay(1000);
    }

    await _client.AddTransactionAsync(new Transaction
    {
      Name = Name,
      Date = Date,
      Amount = Amount,
    });
  }

  public void Dispose()
  {
    _toDispose.Dispose();
  }
}
