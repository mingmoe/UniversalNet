
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;

namespace UniversalNet.Kestrel;

public class EmptyApplication : IHttpApplication<string>
{
    public string CreateContext(IFeatureCollection contextFeatures)
    {
        return string.Empty;
    }

    public void DisposeContext(string context, Exception? exception)
    {

    }

    public Task ProcessRequestAsync(string context)
    {
        return Task.CompletedTask;
    }
}