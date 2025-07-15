using ICG.NetCore.Utilities.Email;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NetCore.Utilities.Email.Resend.IntegrationTest
{
    internal class Program
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

    public class MyService : IMyService
    {
        private readonly ILogger<MyService> _logger;
        private readonly IEmailService _emailService;

        public MyService(ILogger<MyService> logger, IEmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
        }

        public void Run()
        {
            var result = _emailService.SendMessageAsync("test@test.com", "Testing", "<p>Hello from Resend</p>").Result;
        }
    }
}
