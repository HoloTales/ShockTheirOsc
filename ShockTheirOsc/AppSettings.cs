namespace ShockTheirOsc;

public class AppSettings
{
    public string Provider { get; set; } = string.Empty;
    public OpenShockSettings OpenShockSettings { get; set; } = new OpenShockSettings();
    public OscSettings OscSettings { get; set; } = new OscSettings();
}

public class OpenShockSettings
{
    public string ApiUrl { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public List<string> ShockerIds { get; set; } = new List<string>();
}

public class OscSettings
{
    public string Shock { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string Intensity { get; set; } = string.Empty;
}
