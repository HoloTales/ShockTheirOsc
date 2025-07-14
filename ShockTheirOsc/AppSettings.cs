namespace ShockTheirOsc;

public class AppSettings
{
    public string Provider { get; set; } = string.Empty;
    public string ClientName { get; set; } = "ShockTheirOSC/1.0";
    public OpenShockSettings OpenShockSettings { get; set; } = new OpenShockSettings();
    public OscSettings OscSettings { get; set; } = new OscSettings();
    public PiShockSettings PiShockSettings { get; set; } = new PiShockSettings();
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

public class PiShockSettings
{
    public string Username { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
