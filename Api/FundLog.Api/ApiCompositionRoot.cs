using FundLog.ObjectGraph;
using Lamar;

namespace FundLog.Api
{
  public class ApiCompositionRoot : CompositionRoot
  {
    public ApiCompositionRoot(DomainConfig config) : base(config)
    {
      Registry.For<IFundLogApi>().Use<FundLogApi>().Singleton();
    }
  }
}
