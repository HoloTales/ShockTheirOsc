using System.Text.Json.Serialization;

namespace ShockTheirOsc.Providers.OpenShock;

public class OpenShockShocksMessage
{
    [NonSerialized]
    private int duration;

    [NonSerialized]
    private int intensity;

    [JsonPropertyName("id")]
    public string ShockerId { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public OpenShockOpEnum Operation { get; set; }

    [JsonPropertyName("duration")]
    public int Duration
    {
        get => duration;
        set
        {
            if (value < 300 || value > 30000)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(Duration),
                    "Duration must be between 300ms and 30,000ms."
                );
            }
            duration = value;
        }
    }

    [JsonPropertyName("intensity")]
    public int Intensity
    {
        get => intensity;
        set
        {
            if (value < 1 || value > 100)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(Intensity),
                    "Intensity must be between 1 and 100."
                );
            }
            intensity = value;
        }
    }

    [JsonPropertyName("exclusive")]
    public bool Exclusive { get; set; } = false;
}
