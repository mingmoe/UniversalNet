﻿namespace UniversalNet.Middlewares;
public class ReturnMiddleware<T> : IMiddleware<T> where T : notnull
{
	public ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);
		return ValueTask.CompletedTask;
	}

	public Task InvokeAsync(IConnectionContext<T> context, IMiddleware<T>.NextMiddleware next)
	{
		return Task.CompletedTask;
	}
}
