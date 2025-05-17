using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalNet.Middlewares;

public interface IMiddlewareRegister<T> where T : notnull
{
    public void Register(MiddlewaresBuilder<T> builder);
}
