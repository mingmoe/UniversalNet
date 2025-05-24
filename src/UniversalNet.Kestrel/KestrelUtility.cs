using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using UniversalNet.Middlewares;

namespace UniversalNet.Kestrel;

public static class KestrelUtility
{
    public static void UseUniversalNet<T>(this ListenOptions options)
        where T : notnull
    {
        options.Protocols = HttpProtocols.None;
        options.DisableAltSvcHeader = true;

        var serviceProvider = options.ApplicationServices;
        var middlewares = MiddlewaresBuilder<T>.BuildFromServices(serviceProvider);

        var initialMiddleware = new KestrelInitlizeRawMiddleware<T>()
        {
            ConstructContext = (context) =>
            {
                var dispatch = serviceProvider.GetRequiredService<IDispatcher<T>>();
                var packetizer = serviceProvider.GetRequiredService<IPacketFormatters<T>>();

                return new KestrelConnectionContext<T>(context)
                {
                    Dispatcher = dispatch,
                    Packetizer = packetizer
                };
            }
        };

        options.Use(async (c, n) =>
        {
            while (true)
            {
                await n(c);
            }
        });
        options.Use(initialMiddleware.InvokeAsync);
        foreach (var middleware in MiddlewareWrapper<T>.Transform(middlewares))
        {
            options.Use(middleware.InvokeAsync);
        }

        // do nothing but return
        options.UseConnectionHandler<ConnectionHandler>();
    }
}
