using FundLog.Api.Shared;
using FundLog.Model;
using Typin;
using Typin.Attributes;
using Typin.Console;

namespace FundLog.Cli;

[Command("rm")]
public class DeleteTransactionsCommand : ICommand, IDisposable
{
  private readonly IFundLogClient _client;
  private readonly IDisposable _toDispose;

  [CommandParameter(0)]
  public Guid Id { get; set; }

  public DeleteTransactionsCommand(IFundLogClient client)
  {
    _client = client;
    _toDispose = _client.TransactionDeleted.Subscribe(id =>
    {
      Log.Info($"Transaction deleted: {id}");
    });
  }

  public async ValueTask ExecuteAsync(IConsole console)
  {
    while (!_client.IsConnected)
    {
      await Task.Delay(1000);
    }

    await _client.DeleteTransactionAsync(Id);
  }

  public void Dispose()
  {
    _toDispose.Dispose();
  }
}
