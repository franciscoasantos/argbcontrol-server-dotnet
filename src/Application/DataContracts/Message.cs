﻿using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Application.DataContracts;

public class Message
{
    [DataMember]
    [JsonPropertyName("M")]
    public string Mode { get; set; }

    [DataMember]
    [JsonPropertyName("R")]
    public string? Red { get; set; }

    [DataMember]
    [JsonPropertyName("G")]
    public string? Green { get; set; }

    [DataMember]
    [JsonPropertyName("B")]
    public string? Blue { get; set; }

    [DataMember]
    [JsonPropertyName("W")]
    public string? White { get; set; }

    [DataMember]
    [JsonPropertyName("A")]
    public string? Arguments { get; set; }

    public Message(string mode, string? red = "", string? green = "", string? blue = "", string? white = "", string? arguments = "")
    {
        Mode = mode;
        Red = red?.PadLeft(3, '0');
        Green = green?.PadLeft(3, '0');
        Blue = blue?.PadLeft(3, '0');
        White = white?.PadLeft(3, '0');
        Arguments = arguments?.PadLeft(5, '0');
    }

    public string GetRgbwMessage() => $"{Mode}{Red}{Green}{Blue}{White}";
    public string GetArgumentsMessage() => $"{Mode}{Arguments}";
}
