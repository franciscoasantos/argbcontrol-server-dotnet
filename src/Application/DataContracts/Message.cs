using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ArgbControl.Api.Application.DataContracts;

public class Message(string mode, string? red = "", string? green = "", string? blue = "", string? white = "", string? arguments = "")
{
    [DataMember]
    [JsonPropertyName("M")]
    public string Mode { get; set; } = mode;

    [DataMember]
    [JsonPropertyName("R")]
    public string? Red { get; set; } = red?.PadLeft(3, '0');

    [DataMember]
    [JsonPropertyName("G")]
    public string? Green { get; set; } = green?.PadLeft(3, '0');

    [DataMember]
    [JsonPropertyName("B")]
    public string? Blue { get; set; } = blue?.PadLeft(3, '0');

    [DataMember]
    [JsonPropertyName("W")]
    public string? White { get; set; } = white?.PadLeft(3, '0');

    [DataMember]
    [JsonPropertyName("A")]
    public string? Arguments { get; set; } = arguments?.PadLeft(5, '0');

    public string GetRgbwMessage() => $"{Mode}{Red}{Green}{Blue}{White}";
    public string GetArgumentsMessage() => $"{Mode}{Arguments}";
}
