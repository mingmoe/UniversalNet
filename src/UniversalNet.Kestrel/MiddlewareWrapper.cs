using Microsoft.AspNetCore.Connections;
using UniversalNet.Middlewares;

namespace UniversalNet.Kestrel;

public sealed class MiddlewareWrapper<T> where T : notnull
{
    public required IMiddleware<T> Middleware { get; init; }

    public static IEnumerable<MiddlewareWrapper<T>> Transform(IEnumerable<IMiddleware<T>> middlewares)
        => middlewares.Select(m => new MiddlewareWrapper<T> { Middleware = m });

    public async Task InvokeAsync(ConnectionContext context, ConnectionDelegate callback)
    {
        if (context is KestrelConnectionContext<T> con)
        {
            await Middleware.InvokeAsync(con, (context) =>
            {
                return callback.Invoke((KestrelConnectionContext<T>)context!);
            });
            return;
        }

        var key = KestrelInitlizeRawMiddleware<T>.GetContextKey();

        if (!context.Items.ContainsKey(key))
        {
            throw new InvalidOperationException($"The context is not a {typeof(KestrelConnectionContext<T>).FullName}.");
        }

        await Middleware.InvokeAsync((KestrelConnectionContext<T>)context.Items[key]!, (context) =>
        {
            return callback.Invoke((KestrelConnectionContext<T>)context!);
        });
    }
}
