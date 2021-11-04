using Microsoft.JSInterop;

namespace FundLog.Api.JsInterop;

public class JsCallback<T> : IDisposable
{
  public DotNetObjectReference<JsCallback<T>> Ref { get; }

  private readonly Action<T> _callback;

  public JsCallback(Action<T> callback)
  {
    _callback = callback;
    Ref = this.JsRef();
  }

  [JSInvokable]
  public void Run(T args) => _callback(args);

  [JSInvokable]
  public void Dispose() => Ref.Dispose();
}