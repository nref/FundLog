using FundLog.Domain;
using Microsoft.JSInterop;

namespace FundLog.Api.JsInterop;

public class JsCall : JsCallBase
{
  /// <summary>
  /// Prepare to call the JavaScript method of the given name, passing the given arguments
  /// 
  /// <para/>
  /// After this, either <see cref="JsCall.WithCallback{T}(Action{T})"/> or <see cref="JsCall.WithoutCallback()"/> must be called.
  /// </summary>
  public JsCall(IJSRuntime js, string methodName, params object?[]? args) : base(js, methodName, args)
  {
  }

  /// <summary>
  /// Call the configured JavaScript method.
  /// Append the given callback to the arguments.
  /// </summary>
  public async Task WithCallback<T>(Action<T> callback) 
    => await Js.InvokeVoidAsync(MethodName, Args.Concat(new JsCallback<T>(callback).Ref));

  /// <summary>
  /// Call the configured JavaScript method.
  /// </summary>
  public async Task WithoutCallback() => await Js.InvokeVoidAsync(MethodName, Args);
}

public class JsCall<TRet> : JsCallBase
{
  /// <summary>
  /// Prepare to call the JavaScript method of the given name, passing the given arguments
  /// 
  /// <para/>
  /// After this, either <see cref="JsCall{TRet}.WithCallback{TCallbackRet}(Action{TCallbackRet})"/> or <see cref="JsCall{TRet}.WithoutCallback()"/> must be called.
  /// </summary>
  public JsCall(IJSRuntime js, string methodName, params object?[]? args) : base(js, methodName, args)
  {
  }

  /// <summary>
  /// Call the configured JavaScript method.
  /// Append the given callback to the arguments.
  /// </summary>
  public async Task<TRet> WithCallback<TCallbackRet>(Action<TCallbackRet> callback)
    => await Js.InvokeAsync<TRet>(MethodName, Args.Concat(new JsCallback<TCallbackRet>(callback).Ref));

  /// <summary>
  /// Call the configured JavaScript method.
  /// </summary>
  public async Task<TRet> WithoutCallback() => await Js.InvokeAsync<TRet>(MethodName, Args);
}
