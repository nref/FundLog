using Microsoft.JSInterop;

namespace FundLog.Api.JsInterop;

public class JsCallBase
{
  public IJSRuntime Js { get; set; }
  public string MethodName { get; set; }
  public object?[]? Args { get; set; }

  public JsCallBase(IJSRuntime js, string methodName, params object?[]? args)
  {
    Js = js;
    MethodName = methodName;
    Args = args;
  }
}
