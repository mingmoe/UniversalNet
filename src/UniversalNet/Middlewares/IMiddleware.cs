namespace UniversalNet.Middlewares;

public interface IMiddleware<T> where T : notnull
{
    delegate Task NextMiddle(IConnectionContext<T> context);

    Task InvokeAsync(IConnectionContext<T> context, NextMiddle next);
}
