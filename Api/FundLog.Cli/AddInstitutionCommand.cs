using FundLog.Domain;
using FundLog.ObjectGraph;
using Typin;
using Typin.Attributes;
using Typin.Console;

namespace FundLog.Cli;

[Command("addinstitution")]
public class AddInstitutionCommand : ICommand
{
  public async ValueTask ExecuteAsync(IConsole console)
  {
    var adapter = new CompositionRoot().Get<ITransactionAdapter>();
    await adapter.AddInstitution();
  }
}