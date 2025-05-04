using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Sample;

internal class Program
{

    public static void Main(string[] args)
    {
        WebHostBuilder builder = new();

        builder.UseKestrel(option =>
        {
        });
    }
}
