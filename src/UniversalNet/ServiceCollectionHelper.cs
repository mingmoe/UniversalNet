using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalNet.Middlewares;

namespace UniversalNet;
public static class ServiceCollectionHelper
{
    public static void UseAndInitiateStandardDispatcher<T>(this IServiceCollection services)
        where T : notnull
    {
        services.AddSingleton<IDispatcher<T>>((provider) =>
        {
            var dispatcher = new Dispatcher<T>();
            IDispatcherRegister<T>.RegisterAll(provider, dispatcher);
            return dispatcher;
        });
    }
    public static void UseAndInitiateStandardMiddlewares<T>(this IServiceCollection services)
        where T : notnull
    {
        services.AddSingleton<PacketReadMiddleware<T>>();
        services.AddSingleton<PacketDecodeMiddleware<T>>();
        services.AddSingleton<PacketDispatchMiddleware<T>>();
        services.AddSingleton<PacketEncodeMiddleware<T>>();
        services.AddSingleton<PacketWriteMiddleware<T>>();
        services.AddSingleton<DefaultMiddlewaresRegister<T>>();
        services.AddSingleton<IMiddlewareRegister<T>, DefaultMiddlewaresRegister<T>>();
    }

    public static void UseAndInitiateStandardFormatters<T, U>(
        this IServiceCollection services)
        where T : notnull
        where U : PacketFormatters<T>
    {
        services.AddSingleton<U>();
        services.AddSingleton<IPacketFormatters<T>>((provider) =>
        {
            var packetizer = provider.GetRequiredService<U>();
            IPacketFormatterRegister<T>.RegisterAll(provider, packetizer);
            return packetizer;
        });
    }
}
