namespace ArgbControl.Api.Application.Constants;

public static class WebSocketConstants
{
    public const int BufferSize = 4096;
    public const string DefaultInitialData = "0000000000255";
    
    public static class Roles
    {
        public const string Sender = "sender";
        public const string Receiver = "receiver";
    }

    public static class MessageModes
    {
        public const string RgbwMode = "0";
    }
}
