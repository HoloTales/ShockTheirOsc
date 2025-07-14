using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using LucHeart.CoreOSC;
using Microsoft.Extensions.Logging;
using ShockTheirOsc.Providers;

namespace ShockTheirOsc;

public class Server : IDisposable
{
    private ILogger<Server> logger;
    private Thread? serverThread;
    private float vrcIntensity = 0.01f;
    private float vrcDuration = 0.01f;
    private bool disposed = false;

    private OscSender oscSender;
    private OscListener oscReceiver;
    private readonly ShockDebouncer shockProvider;
    private readonly AppSettings settings;
    private readonly Dictionary<string, Action<string>> oscActions;

    public Server(ILogger<Server> logger, ShockDebouncer shockProvider, AppSettings settings)
    {
        this.logger = logger;
        this.shockProvider = shockProvider;
        this.settings = settings;
        oscActions = new Dictionary<string, Action<string>>
        {
            [$"/avatar/parameters/{settings.OscSettings.Shock}"] = HandleShockCommand,
            [$"/avatar/parameters/{settings.OscSettings.Duration}"] = HandleDurationCommand,
            [$"/avatar/parameters/{settings.OscSettings.Intensity}"] = HandleIntensityCommand,
        };
        logger.LogInformation("Starting Servers...");
        oscSender = new OscSender(new IPEndPoint(IPAddress.Loopback, 9000));
        oscReceiver = new OscListener(new IPEndPoint(IPAddress.Loopback, 9001));
        logger.LogInformation("Started Servers");
        logger.LogInformation("API Url: {message}", this.settings.OpenShockSettings.ApiUrl);
        logger.LogInformation(string.Join(", ", settings.OpenShockSettings.ShockerIds));

        serverThread = new Thread(async () => await StartServer());
        serverThread.Start();
    }

    private async Task StartServer()
    {
        while (!disposed)
        {
            try
            {
                var oscMessageRecieved = await Task.Run(() => oscReceiver.ReceiveMessageAsync());

                var oscMessage = oscMessageRecieved.Arguments.FirstOrDefault()?.ToString() ?? "";
                if (oscActions.TryGetValue(oscMessageRecieved.Address, out var action))
                {
                    action(oscMessage);
                }
            }
            catch (SocketException) { }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                if (!disposed)
                    Console.WriteLine(e);
                break;
            }
        }
    }

    private void HandleShockCommand(string oscMessage)
    {
        logger.LogInformation("Shock command received: {message}", oscMessage);
        if (bool.TryParse(oscMessage, out var enabled) && enabled)
        {
            logger.LogInformation("Sending PiShock command...");
            shockProvider.SendShock(
                vrcDuration,
                vrcIntensity,
                () => SendSuccess(true),
                () => SendSuccess(false)
            );
        }
    }

    private void HandleDurationCommand(string oscMessage)
    {
        var durationFromOscStringReader = Regex.Match(oscMessage, @"\d+\.\d+");
        if (float.TryParse(durationFromOscStringReader.Value, out vrcDuration))
        {
            logger.LogInformation($"Set Duration from VRC to {vrcDuration}");
        }
    }

    private void HandleIntensityCommand(string oscMessage)
    {
        var intensityFromOscStringReader = Regex.Match(oscMessage, @"\d+\.?\d*");
        if (float.TryParse(intensityFromOscStringReader.Value, out vrcIntensity))
        {
            logger.LogInformation($"Set Intensity from VRC to {vrcIntensity}");
        }
    }

    private void SendSuccess(bool success)
    {
        logger.LogInformation("Sending success as: {success}", success.ToString());
        var message = new OscMessage("/avatar/parameters/VRCOSC/PiShock/Success", success);
        Task.Run(() => oscSender.SendAsync(message));
    }

    public void Dispose()
    {
        disposed = true;
    }
}
