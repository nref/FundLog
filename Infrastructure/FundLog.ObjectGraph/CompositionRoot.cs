using FundLog.Api.Shared;
using FundLog.Client;
using FundLog.Data;
using FundLog.Domain;
using Going.Plaid;
using Lamar;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace FundLog.ObjectGraph;

public interface ICompositionRoot
{
  ServiceRegistry Registry { get; }
  T Get<T>();
}

public class CompositionRoot : ICompositionRoot
{
  public ServiceRegistry Registry { get; }
  private IContainer _container { get; }

  public CompositionRoot(DomainConfig? config = default)
  {
    Registry = new ServiceRegistry();
    Registry.For<ITransactionRepo>().Use<FundlogRepo>()
      .Ctor<string>("connectionString")
      .Is(config?.ConnectionString ?? "");

    Registry.For<ITransactionAdapter>().Use<PlaidAdapter.PlaidAdapter>();
    Registry.For<ITransactionService>().Use<TransactionService>();
    Registry.For<IEventAgg>().Use<EventAgg>().Singleton();

    if (config?.UseSignalRClient ?? false)
    {
      HubConnection conn = new HubConnectionBuilder()
        .WithUrl("https://localhost:5001/fundlog")
        .ConfigureLogging(logging =>
        {
          logging.AddConsole();
          logging.SetMinimumLevel(LogLevel.Debug);
        })
        .WithAutomaticReconnect()
        .Build();

      _ = conn.StartAsync();

      conn.Closed += ex =>
      {
        Model.Log.Error(ex);
        return conn.StartAsync();
      };

      Registry.For<IFundLogClient>().Use<SignalRFundLogClient>().Ctor<HubConnection>().Is(conn);
    }
    else
    {
      Registry.For<IFundLogClient>().Use<LocalFundLogClient>();
    }

    var client = new PlaidClient(Going.Plaid.Environment.Development, 
      secret: "supersecret", 
      clientId: "supersecret");
    Registry.For<PlaidClient>().Use(client);

    _container = new Container(x => x.IncludeRegistry(Registry));
  }

  public T Get<T>() => _container.GetInstance<T>();
}
