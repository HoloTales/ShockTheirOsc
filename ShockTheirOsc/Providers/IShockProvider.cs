namespace ShockTheirOsc.Providers;

public interface IShockProvider
{
    internal int ConvertVrcDuration(float duration);
    internal int ConvertVrcIntensity(float intensity);
    public TimeSpan ConvertVrcDurationTimeSpan(float duration);
    public Task<TimeSpan> SendShock(
        IEnumerable<string> ids,
        float duration,
        float intensity,
        Action onSend
    );
}
