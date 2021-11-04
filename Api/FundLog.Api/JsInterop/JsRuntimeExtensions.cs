using Microsoft.JSInterop;

namespace FundLog.Api.JsInterop;

public static class JsRuntimeExtensions
{
  /// <summary>
  /// Prepare to call the JavaScript method of the given name, passing the given arguments. 
  /// 
  /// <para/>
  /// After this, either <see cref="JsCall.WithCallback{T}(Action{T})"/> or <see cref="JsCall.WithoutCallback()"/> must be called.
  /// </summary>
  public static JsCall InvokeMethod(this IJSRuntime js, string method, params object?[]? args) => new(js, method, args);
  public static JsCall<T> InvokeMethod<T>(this IJSRuntime js, string method, params object?[]? args) => new(js, method, args);
}
