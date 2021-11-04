using Microsoft.JSInterop;

namespace FundLog.Api.JsInterop;

public static class ObjectExtensions
{
  public static DotNetObjectReference<T> JsRef<T>(this T t) where T : class => DotNetObjectReference.Create(t);
}