using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShockTheirOsc.Providers;
using ShockTheirOsc.Providers.OpenShock;
using ShockTheirOsc.Providers.PiShock;

namespace ShockTheirOsc;

class Program
{
    static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        var serv = host.Services.GetRequiredService<Server>();
        await host.RunAsync();
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args).ConfigureServices(ConfigureServices);

    private static void ValidateConfiguration(AppSettings settings)
    {
        if (string.IsNullOrEmpty(settings.Provider))
        {
            throw new InvalidOperationException("Provider is not specified in the configuration.");
        }

        if (settings.Provider.Equals("openshock", StringComparison.InvariantCultureIgnoreCase))
        {
            if (string.IsNullOrEmpty(settings.OpenShockSettings.ApiUrl))
            {
                throw new InvalidOperationException("OpenShockSettings.ApiUrl is not specified.");
            }

            if (
                string.IsNullOrEmpty(settings.OpenShockSettings.Token)
                || string.Equals(
                    settings.OpenShockSettings.Token,
                    "https://openshock.app/#/dashboard/tokens",
                    StringComparison.InvariantCultureIgnoreCase
                )
            )
            {
                throw new InvalidOperationException("OpenShockSettings.Token is not specified.");
            }

            if (
                settings.OpenShockSettings.ShockerIds == null
                || !settings.OpenShockSettings.ShockerIds.Any()
                || (
                    settings.OpenShockSettings.ShockerIds.FirstOrDefault() is not null
                    && settings
                        .OpenShockSettings.ShockerIds.FirstOrDefault()!
                        .Equals("https://openshock.app/#/dashboard/shockers/own")
                )
            )
            {
                throw new InvalidOperationException(
                    "OpenShockSettings.ShockerIds is not specified or empty."
                );
            }
        }
        else if (settings.Provider.Equals("pishock", StringComparison.InvariantCultureIgnoreCase))
        {
            throw new NotImplementedException("PiShock provider validation is not implemented.");
        }
        else
        {
            throw new InvalidOperationException($"Unknown provider: {settings.Provider}");
        }
    }

    private static void ConfigureServices(
        HostBuilderContext builderContext,
        IServiceCollection services
    )
    {
        var settings = new AppSettings();
        builderContext.Configuration.Bind(settings);
        services.AddSingleton(settings);

        if (settings.Provider.Equals("openshock", StringComparison.InvariantCultureIgnoreCase))
        {
            services.AddHttpClient(
                "OpenShock",
                httpClient =>
                {
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(settings.ClientName);
                    httpClient.DefaultRequestHeaders.Add(
                        "Open-Shock-Token",
                        settings.OpenShockSettings.Token
                    );
                    httpClient.BaseAddress = new Uri(settings.OpenShockSettings.ApiUrl);
                }
            );
            services.AddSingleton<IShockProvider, OpenShockProvider>();
        }
        else if (settings.Provider.Equals("pishock", StringComparison.InvariantCultureIgnoreCase))
        {
            services.AddHttpClient(
                "PiShock",
                httpClient =>
                {
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(settings.ClientName);
                    httpClient.BaseAddress = new Uri("https://do.pishock.com/api/apioperate");
                }
            );
            services.AddSingleton<IShockProvider, PiShockProvider>();
        }
        else
        {
            throw new InvalidOperationException($"Unknown provider: {settings.Provider}");
        }
        services.AddSingleton<ShockDebouncer>();
        services.AddSingleton<Server>();
    }
}
