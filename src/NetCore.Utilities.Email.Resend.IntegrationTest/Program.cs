using ICG.NetCore.Utilities.Email;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NetCore.Utilities.Email.Resend.IntegrationTest
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Register services
                    services.AddTransient<IMyService, MyService>();
                    services.UseIcgNetCoreUtilitiesEmailResend(context.Configuration);
                })
                .Build();

            // Resolve your service
            var myService = host.Services.GetRequiredService<IMyService>();
            myService.Run();
        }
    }

    // Service interface and implementation
    public interface IMyService
    {
        void Run();
    }

    public class MyService(IEmailService emailService) : IMyService
    {
        public void Run()
        {
            var result = emailService.SendMessageAsync("test@test.com", "Testing", "<p>Hello from Resend</p>").Result;
            Console.WriteLine($"Email sent result: {result}");
        }
    }
}
