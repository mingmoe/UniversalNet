using MemoryPack;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Unicode;
using UniversalNet;
using UniversalNet.Kestrel;
using UniversalNet.MemoryPack;
using UniversalNet.Middlewares;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Sample;

[MemoryPackable]
public sealed partial class KeyValuePacket : IWithPacketId<string>
{
    [MemoryPackIgnore]
    public static string PacketId => "key value";

    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}

public class Program
{
    public sealed class KeyValuePacketHandler : IWithPacketId<string>, IPacketHandler<string>
    {
        public static string PacketId => KeyValuePacket.PacketId;

        public ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        public Task Handle(IConnectionContext<string> context, string packetId, object packet)
        {
            Debug.Assert(packetId == KeyValuePacket.PacketId);
            Debug.Assert(packet.GetType() == typeof(KeyValuePacket));
            var pair = (KeyValuePacket)packet;
            Console.WriteLine($"ACCEPT KEY VALUE FROM:{context.ConnectionId}");
            Console.WriteLine($"PACKET KEY:{pair.Key}");
            Console.WriteLine($"PACKET VALUE:{pair.Value}");
            return Task.CompletedTask;
        }

    }

    public sealed class StringPacketHandler : IPacketHandler<string>, IWithPacketId<string>
    {
        public static string PacketId => "ECHO";

        public ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        public Task Handle(IConnectionContext<string> context, string packetId, object packet)
        {
            Debug.Assert(packetId == "ECHO");
            Debug.Assert(packet.GetType() == typeof(string));
            Console.WriteLine($"ACCEPT PACKET FROM:{context.ConnectionId}");
            Console.WriteLine($"PACKET ID:{packetId}");
            Console.WriteLine($"PACKET CONTENT:{packet}");
            return Task.CompletedTask;
        }
    }

    public sealed class StringPacketFormatter : IPacketFormatter<string>, IWithPacketId<string>
    {
        static string IWithPacketId<string>.PacketId { get; } = "ECHO";

        public object GetValue(string packetId, ReadOnlySequence<byte> packet)
        {
            Debug.Assert(packetId == "ECHO");
            var value = Encoding.UTF8.GetString(packet);
            Console.WriteLine("decode packet:" + (string)value);
            return value;
        }

        public Memory<byte> ToPacket(string packetId, object value)
        {
            Debug.Assert(packetId == "ECHO");
            Debug.Assert(value.GetType() == typeof(string));
            Console.WriteLine("encode packet:" + (string)value);
            return Encoding.UTF8.GetBytes(value.ToString()!);
        }
    }

    public sealed class ConsoleInputMiddleware : IMiddleware<string>
    {
        private bool sent = false;
        private readonly string[] echos = ["hello", "world", "!!!", "测试"];
        private readonly string[] keyValues = ["key-value", "fuck-you"];

        public ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        public async Task InvokeAsync(IConnectionContext<string> context, IMiddleware<string>.NextMiddle next)
        {
            if (!sent)
            {
                foreach (var echo in echos)
                {
                    await context.PacketToSend.Writer.WriteAsync(new("ECHO", echo));
                    await next(context);
                    await Task.Delay(200);
                }

                foreach (var keyValue in keyValues)
                {
                    var pair = keyValue.Split('-');
                    await context.PacketToSend.Writer.WriteAsync(
                        new(KeyValuePacket.PacketId,
                        new KeyValuePacket()
                        {
                            Key = pair[0],
                            Value = pair[1]
                        }
                        ));
                    await next(context);
                    await Task.Delay(200);
                }

                sent = true;
            }

            await next(context);
        }
    }

    public sealed class ConsoleInputMiddlewareRegister : IMiddlewareRegister<string>
    {
        public void Register(MiddlewaresBuilder<string> builder)
        {
            builder.BeforeEncode.Add(new ConsoleInputMiddleware());
        }
    }

    static void SampleHost(int port, bool registerConsoleRead)
    {
        HostBuilder builder = new();

        builder.ConfigureServices(services =>
            {
                // utilities
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                });
                services.AddOptions();

                services.AddSingleton<SocketTransportFactory>();
                services.AddSingleton<IConnectionListenerFactory>((provider) =>
                {
                    return provider.GetRequiredService<SocketTransportFactory>();
                });

                services.Configure<SocketTransportOptions>(options =>
                {
                    options.NoDelay = true;
                });

                services.AddSingleton<KestrelServer>();

                // add packets
                services.AddSingleton<MemorypackPacketFormatter<string, KeyValuePacket>>();
                services.AddSingleton<KeyValuePacketHandler>();
                services.AddSingleton<IDispatcherRegister<string>, PacketHandlerRegister<string, KeyValuePacketHandler>>();
                services.AddSingleton<IPacketFormatterRegister<string>, PacketFormatterRegister<string, MemorypackPacketFormatter<string, KeyValuePacket>>>();

                services.AddSingleton<StringPacketFormatter>();
                services.AddSingleton<StringPacketHandler>();
                services.AddSingleton<IPacketFormatterRegister<string>, PacketFormatterRegister<string, StringPacketFormatter>>();
                services.AddSingleton<IDispatcherRegister<string>, PacketHandlerRegister<string, StringPacketHandler>>();

                // utilities,too
                if (registerConsoleRead)
                {
                    services.AddSingleton<IMiddlewareRegister<string>>(new ConsoleInputMiddlewareRegister());
                }

                services.UseAndInitiateStandardDispatcher<string>();
                services.UseAndInitiateStandardMiddlewares<string>();
                services.UseAndInitiateStandardFormatters<string, UniversalNet.StringPacketFormatter>();

                services.AddSingleton<IHttpApplication<string>>(new EmptyApplication());

                // configure
                services.AddSingleton<IOptions<KestrelServerOptions>>((provider) =>
                {
                    var options = new KestrelServerOptions() { ApplicationServices = provider };

                    options.AddServerHeader = false;

                    options.Listen(IPAddress.Loopback, port, configure =>
                    {
                        configure.UseUniversalNet<string>();
                    });

                    return Options.Create(options);
                });
            });

        var host = builder.Build();
        var server = host.Services.GetRequiredService<KestrelServer>();
        var application = host.Services.GetRequiredService<IHttpApplication<string>>();
        server.StartAsync(application, CancellationToken.None).Wait();
        CancellationTokenSource source = new();
        Console.CancelKeyPress += (_, _) => { source.Cancel(); };
        source.Token.WaitHandle.WaitOne();
        server.StopAsync(CancellationToken.None);
    }

    static async Task RelaySocket(int receiverPort, int senderPort)
    {
        using var receiver = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await receiver.ConnectAsync(IPEndPoint.Parse($"127.0.0.1:{receiverPort}"));

        using var sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await sender.ConnectAsync(IPEndPoint.Parse($"127.0.0.1:{senderPort}"));

        receiver.NoDelay = true;
        sender.NoDelay = true;

        var receiverBuffer = new byte[512];
        var receiverTask = receiver.ReceiveAsync(receiverBuffer);

        var senderBuffer = new byte[512];
        var senderTask = sender.ReceiveAsync(senderBuffer);

        while (true)
        {
            if (receiverTask.IsCompleted)
            {
                var length = await receiverTask!;
                await sender.SendAsync(receiverBuffer[0..length], SocketFlags.None);
                receiverTask = receiver.ReceiveAsync(receiverBuffer);
            }
            if (receiverTask.IsCanceled || receiverTask.IsFaulted)
            {
                break;
            }
            if (senderTask.IsCompleted)
            {
                var length = await senderTask!;
                await receiver.SendAsync(senderBuffer[0..length], SocketFlags.None);
                senderTask = sender.ReceiveAsync(senderBuffer);
            }
            if (senderTask.IsCanceled || senderTask.IsFaulted)
            {
                break;
            }
            await Task.Yield();
        }

        receiver.Close();
        sender.Close();
    }

    public static void Main(string[] args)
    {
        Thread server = new(() => { SampleHost(9988, true); });
        server.Start();
        Thread client = new(() => { SampleHost(9987, false); });
        client.Start();
        Thread.Sleep(1000);

        var relay = Task.Run(async () => { await RelaySocket(9988, 9987); });

        // clean
        server.Join();
        client.Join();
        relay.Wait();
    }
}
