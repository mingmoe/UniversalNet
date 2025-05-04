using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalNet.Middlewares;
public sealed class MiddlewaresBuilder<T> where T : notnull
{
    public List<Func<IMiddleware<T>, IMiddleware<T>>> Transformers { get; } = [];

    public List<IMiddleware<T>> BeforeRead { get; } = [];

    public IMiddleware<T>? ReadMiddleware { get; set; }

    public List<IMiddleware<T>> BeforeParse { get; } = [];

    public IMiddleware<T>? ParseMiddleware { get; set; }

    public List<IMiddleware<T>> BeforeDispatch { get; } = [];

    public IMiddleware<T>? DispatchMiddleware { get; set; }

    public List<IMiddleware<T>> BeforeEncode { get; } = [];

    public IMiddleware<T>? EncodeMiddleware { get; set; }

    public List<IMiddleware<T>> BeforeWrite { get; } = [];

    public IMiddleware<T>? WriteMiddleware { get; set; }

    public List<IMiddleware<T>> AfterWrite { get; } = [];

    public IEnumerable<IMiddleware<T>>? Build()
    {
        List<IMiddleware<T>?> results = [];

        results.AddRange(BeforeRead);
        results.Add(ReadMiddleware);
        results.AddRange(BeforeParse);
        results.Add(ParseMiddleware);
        results.AddRange(BeforeDispatch);
        results.Add(DispatchMiddleware);
        results.AddRange(BeforeEncode);
        results.Add(EncodeMiddleware);
        results.AddRange(BeforeWrite);
        results.Add(WriteMiddleware);
        results.AddRange(AfterWrite);

        results.RemoveAll((m) => m is null);

        return results.Select(transform);

        IMiddleware<T> transform(IMiddleware<T>? middleware)
        {
            foreach (var transformer in Transformers)
            {
                middleware = transformer(middleware!);
            }
            return middleware!;
        }
    }
}
