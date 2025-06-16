using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UniversalNet.Kestrel;

namespace UniversalNet.Autofac;

public class UniversalNetModule<T, U> : Module where T : notnull where U : PacketFormatters<T>
{
	public List<Action<KestrelServerOptions>> ConfigureServer { get; } = [];
	public List<Action<SocketTransportOptions>> ConfigureSocket { get; } = [];

	protected override void Load(ContainerBuilder builder)
	{
		// build kestrel and its configuration
		builder.RegisterType<KestrelServer>().AsSelf().SingleInstance();
		builder.RegisterType<SocketTransportFactory>().As<IConnectionListenerFactory>().SingleInstance();
		builder.RegisterType<EmptyApplication>().As<IHttpApplication<string>>().SingleInstance();

		builder.Register<IOptions<KestrelServerOptions>>((provider) =>
		{
			var services = provider.Resolve<IServiceProvider>();
			var options = new KestrelServerOptions
			{
				ApplicationServices = services,
				AddServerHeader = false,
			};

			foreach (var configure in ConfigureServer)
			{
				configure(options);
			}

			return Options.Create(options);
		}).AsSelf().SingleInstance();

		builder.Register<IOptions<SocketTransportOptions>>((_) =>
		{
			var options = new SocketTransportOptions
			{
				NoDelay = true
			};

			foreach (var configure in ConfigureSocket)
			{
				configure(options);
			}

			return Options.Create(options);
		}).AsSelf().SingleInstance();

		// configure universal net self
		{
			ServiceCollection serviceDescriptors = new();
			serviceDescriptors.UseAndInitiateStandardDispatcher<T>();
			serviceDescriptors.UseAndInitiateStandardMiddlewares<T>();
			serviceDescriptors.UseAndInitiateStandardFormatters<T, U>();
			builder.Populate(serviceDescriptors);
		}
	}

}
