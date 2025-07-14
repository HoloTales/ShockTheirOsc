using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace ShockTheirOsc.Providers.OpenShock;

public class OpenShockProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<OpenShockProvider> logger,
    AppSettings appSettings
) : IShockProvider
{
    public TimeSpan ConvertVrcDurationTimeSpan(float duration)
    {
        var ms = ConvertVrcDuration(duration);
        return TimeSpan.FromMilliseconds(ms);
    }

    public int ConvertVrcIntensity(float intensity)
    {
        return intensity.ConvertVrcFloatToRange(1, 100);
    }

    public int ConvertVrcDuration(float duration)
    {
        return duration.ConvertVrcFloatToRange(300, 30000);
    }

    public async Task<TimeSpan> SendShock(
        float duration,
        float intensity,
        Action onSend
    )
    {
        try
        {
            var message = new OpenShockControlMessage()
            {
                CustomName = "ShockTheirOSC",
                ControlMessages = appSettings.OpenShockSettings.ShockerIds.Select(id => new OpenShockShocksMessage()
                    {
                        ShockerId = id,
                        Operation = OpenShockOpEnum.Shock,
                        Intensity = ConvertVrcIntensity(intensity),
                        Duration = ConvertVrcDuration(duration),
                    })
                    .ToArray(),
            };

            var httpClient = httpClientFactory.CreateClient("OpenShock");

            var serializeOptions = new JsonSerializerOptions()
            {
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            };
            var httpContent = JsonSerializer.Serialize(message, serializeOptions);
            var content = new StringContent(httpContent, Encoding.UTF8, "application/json");
            var stopWatch = Stopwatch.StartNew();
            var responseTask = httpClient.PostAsync("/2/shockers/control", content);
            onSend.Invoke();
            logger.LogInformation("Message sent to openshock: {message}", message);
            var response = await responseTask;
            var time = stopWatch.Elapsed;
            response.EnsureSuccessStatusCode();
            logger.LogInformation(
                "Message returned success from openshock: {statusCode}",
                response.StatusCode
            );
            return time;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Something went wrong when sending a shock to OpenShock.");
        }
        return TimeSpan.Zero;
    }
}
