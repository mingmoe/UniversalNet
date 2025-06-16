using System.Net;
using System.Net.Sockets;

namespace UniversalNet;
public sealed class RelaySocket
{
	public Socket First { get; init; }

	public Socket Second { get; init; }

	public RelaySocket(Socket first, Socket second)
	{
		First = first ?? throw new ArgumentNullException(nameof(first));
		Second = second ?? throw new ArgumentNullException(nameof(second));
	}

	public RelaySocket(string fristIpAndPort, string secondIpAndPort)
	{
		ArgumentNullException.ThrowIfNull(fristIpAndPort, nameof(fristIpAndPort));
		ArgumentNullException.ThrowIfNull(secondIpAndPort, nameof(secondIpAndPort));

		First = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		First.Connect(IPEndPoint.Parse(fristIpAndPort));

		Second = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		Second.Connect(IPEndPoint.Parse(secondIpAndPort));
	}

	public async Task Relay(bool noDelay = false)
	{
		var first = Relay(First, Second);
		var second = Relay(Second, First);

		First.NoDelay = noDelay;
		Second.NoDelay = noDelay;

		while (true)
		{
			if (first.IsCompletedSuccessfully)
			{
				first = Relay(First, Second);
			}
			if (second.IsCompletedSuccessfully)
			{
				second = Relay(Second, First);
			}
			if (first.IsFaulted || second.IsFaulted)
			{
				break;
			}
			await Task.Yield();
		}

		First.Close();
		Second.Close();

		static async Task Relay(Socket socket, Socket send)
		{
			var buffer = new byte[512];
			var task = await socket.ReceiveAsync(buffer);
			await send.SendAsync(buffer[0..task], SocketFlags.None);
		}
	}
}
