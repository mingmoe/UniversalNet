
namespace UniversalNet;

public struct ParsedPacket<T> where T : notnull
{
	public T Id { get; set; }

	public object Obj { get; set; }

	public ParsedPacket(T Id, object Obj)
	{
		ArgumentNullException.ThrowIfNull(Obj);
		this.Id = Id;
		this.Obj = Obj;
	}
}
