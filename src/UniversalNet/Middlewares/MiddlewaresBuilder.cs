using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalNet.Middlewares;
public sealed class MiddlewaresBuilder<T> where T : notnull
{
    public IServiceProvider? ServiceProvider { get; init; }

    public List<Func<IMiddleware<T>, IMiddleware<T>>> Transformers { get; } = [];

    public List<IMiddleware<T>> BeforeHandleException { get; } = [];

    public IMiddleware<T>? ExceptionHandler { get; set; }

    public List<IMiddleware<T>> BeforeRead { get; } = [];

    public IMiddleware<T>? ReadMiddleware { get; set; }

    public List<IMiddleware<T>> BeforeDecode { get; } = [];

    public IMiddleware<T>? DecodeMiddleware { get; set; }

    public List<IMiddleware<T>> BeforeDispatch { get; } = [];

    public IMiddleware<T>? DispatchMiddleware { get; set; }

    public List<IMiddleware<T>> BeforeEncode { get; } = [];

    public IMiddleware<T>? EncodeMiddleware { get; set; }

    public List<IMiddleware<T>> BeforeWrite { get; } = [];

    public IMiddleware<T>? WriteMiddleware { get; set; }

    public List<IMiddleware<T>> AfterWrite { get; } = [];

    public event EventHandler<MiddlewaresBuilder<T>> BeforeBuild = (_, _) => { };

    public event EventHandler<IList<IMiddleware<T>>> AfterBuild = (_, _) => { };

    public IEnumerable<IMiddleware<T>> Build()
    {
        List<IMiddleware<T>?> results = [];

        BeforeBuild(this, this);

        results.AddRange(BeforeHandleException);
        results.Add(ExceptionHandler);
        results.AddRange(BeforeRead);
        results.Add(ReadMiddleware);
        results.AddRange(BeforeDecode);
        results.Add(DecodeMiddleware);
        results.AddRange(BeforeDispatch);
        results.Add(DispatchMiddleware);
        results.AddRange(BeforeEncode);
        results.Add(EncodeMiddleware);
        results.AddRange(BeforeWrite);
        results.Add(WriteMiddleware);
        results.AddRange(AfterWrite);

        results.RemoveAll((m) => m is null);

        var output = results.Select(transform).ToList();
        AfterBuild(this, output);
        return output;

        IMiddleware<T> transform(IMiddleware<T>? middleware)
        {
            foreach (var transformer in Transformers)
            {
                middleware = transformer(middleware!);
            }
            return middleware!;
        }
    }

    public MiddlewaresBuilder()
    {
    }

    public MiddlewaresBuilder(IServiceProvider? serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public static IEnumerable<IMiddleware<T>> BuildFromServices(IServiceProvider provider)
    {
        var middlewares = (IEnumerable<IMiddlewareRegister<T>>?)provider.GetService(typeof(IEnumerable<IMiddlewareRegister<T>>));

        if (middlewares is null)
        {
            return [];
        }

        MiddlewaresBuilder<T> builder = new(provider);

        foreach (var middleware in middlewares)
        {
            middleware.Register(builder);
        }

        return builder.Build();
    }
}
