using FundLog.Api.Shared;
using FundLog.Model;
using Typin;
using Typin.Attributes;
using Typin.Console;

namespace FundLog.Cli;

[Command("sync")]
public class SyncTransactionsCommand : ICommand
{
  private readonly IFundLogClient _client;

  public SyncTransactionsCommand(IFundLogClient client)
  {
    _client = client;
  }

  public ValueTask ExecuteAsync(IConsole console)
  {
    _client.Connected.Subscribe(async _ =>
    {
      await console.Output.WriteLineAsync($"Fetching transactions...");
      List<Transaction> transactions = await _client.SyncTransactionsAsync();
      await console.Output.WriteLineAsync($"Got {transactions.Count} transactions");

      foreach (Transaction trans in transactions)
      {
        await console.Output.WriteLineAsync($"  {trans.Date} {trans.Institution} {trans.Name} ${trans.Amount}");
      }
    });

    return ValueTask.CompletedTask;
  }
}