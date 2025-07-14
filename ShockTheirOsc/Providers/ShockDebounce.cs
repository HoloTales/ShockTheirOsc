using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ShockTheirOsc.Providers.OpenShock;

namespace ShockTheirOsc.Providers;

public class ShockDebouncer
{
    private Task offTask = Task.CompletedTask;
    public bool IsShocking => DateTime.UtcNow - (lastShockTime + lastShockDuration) < TimeSpan.Zero;

    private readonly IShockProvider shockProvider;
    private readonly TimeSpan debounceDuration = TimeSpan.FromMilliseconds(100);
    private TimeSpan lastShockDuration;
    private DateTime lastShockTime;
    private readonly ILogger<ShockDebouncer> logger;

    public ShockDebouncer(IShockProvider shockProvider, ILogger<ShockDebouncer> logger)
    {
        this.shockProvider = shockProvider;
        lastShockTime = DateTime.MinValue;
        lastShockDuration = TimeSpan.Zero;
        this.logger = logger;
    }

    public async void SendShock(
        float vrcDuration,
        float vrcIntensity,
        Action onSend,
        Action onFinish
    )
    {
        var currentTime = DateTime.UtcNow;

        // Always debounce for 100ms
        if (currentTime - lastShockTime < debounceDuration)
        {
            return;
        }

        // Also debounce if last shock hasnt finished
        var timeUntilNextShock = lastShockTime + lastShockDuration - currentTime - debounceDuration;
        if (timeUntilNextShock > TimeSpan.Zero)
        {
            return;
        }

        lastShockTime = DateTime.UtcNow;
        lastShockDuration = shockProvider.ConvertVrcDurationTimeSpan(vrcDuration);
        QueueOffMessage(onFinish);
        await shockProvider.SendShock(vrcDuration, vrcIntensity, onSend);
    }

    private void QueueOffMessage(Action onFinish)
    {
        if (offTask.IsCompleted || offTask.IsCanceled || offTask.IsFaulted)
        {
            logger.LogDebug("Creating Off Task");
            offTask = Task.Run(() => SendOffMessage(onFinish));
        }
    }

    private async Task SendOffMessage(Action onFinish)
    {
        var shouldLoop = true;
        while (shouldLoop)
        {
            if (IsShocking)
            {
                var differenceBetweenNowAndNextPossibleTime =
                    lastShockTime - DateTime.UtcNow + lastShockDuration;
                await Task.Delay(differenceBetweenNowAndNextPossibleTime);
            }
            else
            {
                shouldLoop = false;
            }
        }

        onFinish.Invoke();
    }
}
