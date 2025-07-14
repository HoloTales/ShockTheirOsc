using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ShockTheirOsc.Providers.PiShock;

public class PiShockProvider(IHttpClientFactory httpClientFactory, AppSettings appSettings, ILogger<PiShockProvider> logger) : IShockProvider
{
    public TimeSpan ConvertVrcDurationTimeSpan(float duration)
    {
        var s = ConvertVrcDuration(duration);
        return TimeSpan.FromSeconds(s);
    }

    public int ConvertVrcIntensity(float intensity)
    {
        return intensity.ConvertVrcFloatToRange(1, 100);
    }

    public int ConvertVrcDuration(float duration)
    {
        return duration.ConvertVrcFloatToRange(1, 15);
    }

    public async Task<TimeSpan> SendShock(float duration, float intensity, Action onSend)
    {
        try
        {
            var message = new PiShockMessage()
            {
                Username = appSettings.PiShockSettings.Username,
                Name = appSettings.ClientName,
                Apikey = appSettings.PiShockSettings.ApiKey,
                Code = appSettings.PiShockSettings.Code,
                Op = PiShockOpEnum.Shock,
                Intensity = ConvertVrcIntensity(intensity),
                Duration = ConvertVrcDuration(duration)
            };

            var httpClient = httpClientFactory.CreateClient("PiShock");
            
            var httpContent = JsonSerializer.Serialize(message);
            var content = new StringContent(httpContent, Encoding.UTF8, "application/json");
            var stopWatch = Stopwatch.StartNew();
            var responseTask = httpClient.PostAsync("/", content);
            onSend.Invoke();
            logger.LogInformation("Message sent to PiShock: {message}", message);
            var response = await responseTask;
            var time = stopWatch.Elapsed;
            response.EnsureSuccessStatusCode();
            logger.LogInformation(
                "Message returned success from PiShock: {statusCode}",
                response.StatusCode
            );
            return time;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Something went wrong when sending a shock to PiShock.");
        }
        return TimeSpan.Zero;
    }
}