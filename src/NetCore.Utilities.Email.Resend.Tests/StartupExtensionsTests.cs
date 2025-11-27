using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ICG.NetCore.Utilities.Email.Resend.Tests;

public class StartupExtensionsTests
{
    [Fact]
    public void Configuration_ShouldMapAllValues()
    {
        //Arrange
        var collection = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        collection.UseIcgNetCoreUtilitiesEmailResend(configuration);
        var services = collection.BuildServiceProvider();

        //Act
        var myConfig = services.GetService<IOptions<ResendServiceOptions>>();

        //Assert
        Assert.NotNull(myConfig);
        var values = myConfig.Value;
        Assert.Equal("test@test.com", values.AdminEmail);
        Assert.Equal("TestKey", values.ResendApiKey);

        //API Key should be skipped for now
        //Assert.Single(values.AdditionalApiKeys);
        //var specialKeyValue = values.AdditionalApiKeys["SpecialSender"];
        //Assert.Equal("SpecialKey", specialKeyValue);

        Assert.True(values.AlwaysTemplateEmails);
        Assert.True(values.AddEnvironmentSuffix);
    }
        

    [Fact]
    public void ServiceCollection_ShouldRegisterResendService()
    {
        //Arrange
        var collection = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        collection.AddSingleton(new Mock<IHostEnvironment> ().Object);
        collection.UseIcgNetCoreUtilitiesEmailResend(configuration);
        collection.AddLogging();
        var services = collection.BuildServiceProvider();

        //Act
        var result = services.GetService<IEmailService>();

        //Assert
        Assert.NotNull(result);
        Assert.IsType<ResendService>(result);
    }


    [Fact]
    public void ServiceCollection_ShouldRegisterSendMessageBuilder()
    {
        //Arrange
        var collection = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        collection.AddSingleton(new Mock<IHostEnvironment>().Object);
        collection.AddLogging();
        collection.UseIcgNetCoreUtilitiesEmailResend(configuration);
        var services = collection.BuildServiceProvider();

        //Act
        var result = services.GetService<IResendMessageBuilder>();

        //Assert
        Assert.NotNull(result);
        Assert.IsType<ResendMessageBuilder>(result);
    }

    [Fact]
    public void ServiceCollection_ShouldRegisterResendSender()
    {
        //Arrange
        var collection = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        collection.AddSingleton(new Mock<IHostEnvironment>().Object);
        collection.UseIcgNetCoreUtilitiesEmailResend(configuration);
        var services = collection.BuildServiceProvider();

        //Act
        var result = services.GetService<IResendSender>();

        //Assert
        Assert.NotNull(result);
        Assert.IsType<ResendSender>(result);
    }
}