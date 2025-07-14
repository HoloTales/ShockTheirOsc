using System.Text.Json.Serialization;

namespace ShockTheirOsc.Providers.PiShock;

public class PiShockMessage
{
    [NonSerialized]
    private int duration;

    [NonSerialized]
    private int intensity;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Apikey")]
    public string Apikey { get; set; } = string.Empty;

    [JsonPropertyName("Code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("Username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("Op")]
    public PiShockOpEnum Op { get; set; }

    [JsonPropertyName("Duration")]
    public int Duration
    {
        get => duration;
        set
        {
            if (value < 1 || value > 15)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(Duration),
                    "Duration must be between 1 and 15."
                );
            }
            duration = value;
        }
    }

    [JsonPropertyName("Intensity")]
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
}