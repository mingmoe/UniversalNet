using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalNet.Middlewares;
public class ReturnMiddleware<T> : IMiddleware<T> where T : notnull
{
    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    public Task InvokeAsync(IConnectionContext<T> context, IMiddleware<T>.NextMiddle next)
    {
        return Task.CompletedTask;
    }
}
