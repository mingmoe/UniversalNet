namespace UniversalNet.Middlewares;

public interface IMiddlewareRegister<T> where T : notnull
{
	public void Register(MiddlewaresBuilder<T> builder);
}
