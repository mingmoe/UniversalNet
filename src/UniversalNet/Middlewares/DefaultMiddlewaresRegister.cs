using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalNet.Middlewares;

public sealed class DefaultMiddlewaresRegister<T> : IMiddlewareRegister<T> where T : notnull
{
    public void Register(MiddlewaresBuilder<T> builder)
    {
        if (builder.ServiceProvider is null)
        {
            throw new InvalidOperationException("The argument builder must work with service provider.");
        }

        builder.ExceptionHandler = builder.ServiceProvider.GetRequiredService<ExceptionHandlerMiddleware<T>>();
        builder.ReadMiddleware = builder.ServiceProvider.GetRequiredService<PacketReadMiddleware<T>>();
        builder.DecodeMiddleware = builder.ServiceProvider.GetRequiredService<PacketDecodeMiddleware<T>>();
        builder.DispatchMiddleware = builder.ServiceProvider.GetRequiredService<PacketDispatchMiddleware<T>>();
        builder.EncodeMiddleware = builder.ServiceProvider.GetRequiredService<PacketEncodeMiddleware<T>>();
        builder.WriteMiddleware = builder.ServiceProvider.GetRequiredService<PacketWriteMiddleware<T>>();
    }
}
