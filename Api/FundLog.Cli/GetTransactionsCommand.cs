using FundLog.Api.Shared;
using FundLog.Model;
using Typin;
using Typin.Attributes;
using Typin.Console;

namespace FundLog.Cli;

[Command("get")]
public class GetTransactionsCommand : ICommand
{
  private readonly IFundLogClient _client;

  public GetTransactionsCommand(IFundLogClient client)
  {
    _client = client;
  }

  public async ValueTask ExecuteAsync(IConsole console)
  {
    while (!_client.IsConnected)
    {
      await console.Output.WriteLineAsync($"Waiting for connection...");
      await Task.Delay(1000);
    }

    await console.Output.WriteLineAsync($"Fetching transactions...");
    List<Transaction> transactions = await _client.GetTransactionsAsync();
    await console.Output.WriteLineAsync($"Got {transactions.Count} transactions");

    foreach (Transaction trans in transactions)
    {
      await console.Output.WriteLineAsync($"  {trans.Date} {trans.Institution} {trans.Name} ${trans.Amount}");
    }
  }
}