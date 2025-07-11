using System.Text.Json.Serialization;

namespace ShockTheirOsc.Providers.OpenShock;

public class OpenShockControlMessage
{
    [JsonPropertyName("shocks")]
    public OpenShockShocksMessage[] ControlMessages { get; set; } = [];

    [JsonPropertyName("customName")]
    public string? CustomName { get; set; }
}
