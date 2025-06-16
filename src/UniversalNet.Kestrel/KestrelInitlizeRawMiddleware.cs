

using Microsoft.AspNetCore.Connections;

namespace UniversalNet.Kestrel;

public class KestrelInitlizeRawMiddleware<T> where T : notnull
{
	public static string GetContextKey()
	{
		return $"UniversalNet Connection Context[{typeof(T).FullName}]";
	}

	public string Key { get; } = GetContextKey();

	public required Func<ConnectionContext, IConnectionContext<T>> ConstructContext { get; set; }

	public async Task InvokeAsync(ConnectionContext context, ConnectionDelegate callback)
	{
		if (!context.Items.ContainsKey(Key))
		{
			var con = ConstructContext.Invoke(context);

			context.Items[Key] = con;
		}

		await callback.Invoke((KestrelConnectionContext<T>) context.Items[Key]!).ConfigureAwait(false);
	}
}
