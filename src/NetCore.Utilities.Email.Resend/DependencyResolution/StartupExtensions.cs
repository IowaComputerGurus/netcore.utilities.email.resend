using ICG.NetCore.Utilities.Email;
using ICG.NetCore.Utilities.Email.Resend;
using Microsoft.Extensions.Configuration;
using Resend;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to make DI easier
/// </summary>
public static class StartupExtensions
{
    /// <summary>
    ///     Registers the items included in the ICG AspNetCore Utilities project for Dependency Injection
    /// </summary>
    /// <param name="services">Your existing services collection</param>
    /// <param name="configuration">The configuration instance to load settings</param>
    public static void UseIcgNetCoreUtilitiesEmailResend(this IServiceCollection services, IConfiguration configuration)
    {
        //Register internal services
        services.UseIcgNetCoreUtilitiesEmail(configuration);

        //Register resend itself
        services.AddHttpClient<ResendClient>();
        services.Configure<ResendClientOptions>(o =>
        {
            o.ApiToken = configuration.GetValue<string>("ResendServiceOptions:ResendApiKey");
        });
        services.AddTransient<IResend, ResendClient>();

        //Register our additional services
        services.AddTransient<IEmailService, ResendService>();
        services.AddTransient<IResendMessageBuilder, ResendMessageBuilder>();
        services.AddTransient<IResendSender, ResendSender>();
        services.Configure<ResendServiceOptions>(configuration.GetSection(nameof(ResendServiceOptions)));
    }
}