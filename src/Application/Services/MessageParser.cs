using ArgbControl.Api.Application.Constants;
using ArgbControl.Api.Application.Contracts;
using ArgbControl.Api.Application.DataContracts;
using System.Text.Json;
using System.Text;

namespace ArgbControl.Api.Application.Services;

public sealed class MessageParser : IMessageParser
{
    public byte[] ParseFromJson(ArraySegment<byte> buffer)
    {
        var jsonString = Encoding.UTF8.GetString(buffer);
        var message = JsonSerializer.Deserialize<Message>(jsonString);
        
        if (message is null)
        {
            throw new InvalidOperationException("Failed to deserialize message");
        }

        var stringMessage = message.Mode is WebSocketConstants.MessageModes.RgbwMode
            ? message.GetRgbwMessage() 
            : message.GetArgumentsMessage();

        return Encoding.UTF8.GetBytes(stringMessage);
    }
}
