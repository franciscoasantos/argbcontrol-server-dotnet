namespace ArgbControl.Api.Application.Contracts;

public interface IMessageParser
{
    byte[] ParseFromJson(ArraySegment<byte> buffer);
}
