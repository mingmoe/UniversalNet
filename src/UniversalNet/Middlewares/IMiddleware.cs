namespace UniversalNet.Middlewares;

/// <summary>
/// It should be non status.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IMiddleware<T> : IAsyncDisposable where T : notnull
{
    delegate Task NextMiddle(IConnectionContext<T> context);

    Task InvokeAsync(IConnectionContext<T> context, NextMiddle next);
}
