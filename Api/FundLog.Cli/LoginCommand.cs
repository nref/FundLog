using FundLog.Api.Shared;
using FundLog.Model;
using Typin;
using Typin.Attributes;
using Typin.Console;

namespace FundLog.Cli;

[Command("login")]
public class LoginCommand : ICommand
{
  private readonly IFundLogClient _client;

  [CommandParameter(0)]
  public string Email { get; set; } = "";

  [CommandParameter(1)]
  public string Password { get; set; } = "";

  [CommandOption("remember-me")]
  public bool RememberMe { get; set; }

  public LoginCommand(IFundLogClient client)
  {
    _client = client;
  }

  public async ValueTask ExecuteAsync(IConsole console)
  {
    while (!_client.IsConnected)
    {
      await Task.Delay(1000);
    }

    bool ok = await _client.LoginAsync(new Login
    {
      Email = Email,
      Password = Password,
      RememberMe = RememberMe,
    });

    await console.Output.WriteLineAsync($"ok ? {ok}");
  }
}
