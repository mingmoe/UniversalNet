using Microsoft.AspNetCore.Connections;

namespace UniversalNet.Kestrel;

public sealed class ConnectionHandler : Microsoft.AspNetCore.Connections.ConnectionHandler
{
    public override Task OnConnectedAsync(ConnectionContext connection)
    {
        return Task.CompletedTask;
    }
}
