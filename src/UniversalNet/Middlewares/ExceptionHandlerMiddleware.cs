using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalNet.Middlewares;

public class ExceptionHandlerMiddleware<T>(ILogger<ExceptionHandlerMiddleware<T>> logger) : IMiddleware<T> where T : notnull
{
    private const string CountKey = $"{nameof(ExceptionHandlerMiddleware<T>)}.ExceptionCounts";
    private const string TimeKey = $"{nameof(ExceptionHandlerMiddleware<T>)}.LastExceptionThrowTime";

    private readonly ILogger<ExceptionHandlerMiddleware<T>> logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public virtual ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Try handle the exception that the middleware of the connection throw.
    /// </summary>
    /// <param name="exception">the exception</param>
    /// <param name="context">the connection context</param>
    protected virtual void Handle(IConnectionContext<T> context, Exception exception)
    {
        logger.LogError(exception,
            "Get an exception from the connection {ConnectionId}",
            context.ConnectionId);

        context.Items.TryGetValue(CountKey, out var count);
        context.Items.TryGetValue(TimeKey, out var time);

        if (time is null)
        {
            context.Items.Add(TimeKey, DateTime.UtcNow);
        }

        if (count is null)
        {
            context.Items.Add(CountKey, 1);
        }

        if (count is null || time is null)
        {
            return;
        }

        // clear the count if the time is more than 1 minute
        if (DateTime.UtcNow >= ((DateTime)time).AddMinutes(1))
        {
            context.Items[CountKey] = 1;
            context.Items[TimeKey] = DateTime.UtcNow;
            return;
        }

        // check
        if (((int)count) >= 5)
        {
            logger.LogWarning("The connection {ConnectionId} has throwed {Count} exceptions in limited times, close the connection.",
                context.ConnectionId,
                count);
            context.Abort();
            return;
        }

        context.Items[CountKey] = (int)count + 1;
        context.Items[TimeKey] = DateTime.UtcNow;
        return;
    }

    public async Task InvokeAsync(IConnectionContext<T> context, IMiddleware<T>.NextMiddleware next)
    {
        try
        {
            await next(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Handle(context, ex);
        }
    }

}
