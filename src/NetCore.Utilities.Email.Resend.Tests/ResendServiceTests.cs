using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Resend;
using Xunit;

namespace ICG.NetCore.Utilities.Email.Resend.Tests;

public class ResendServiceTests
{
    private readonly ResendServiceOptions _options = new ResendServiceOptions()
    {
        AdminEmail = "admin@test.com",
        AdminName = "John Smith",
        ResendApiKey = "APIKEY",
        AddEnvironmentSuffix = false,
        AlwaysTemplateEmails = false
    };

    private readonly Mock<IResendMessageBuilder> _resendMessageBuilderMock;
    private readonly Mock<IResendSender> _resendSenderMock;
    private readonly ResendService _service;

    public ResendServiceTests()
    {
        _resendMessageBuilderMock = new Mock<IResendMessageBuilder>();
        _resendSenderMock = new Mock<IResendSender>();
        _service = new ResendService(new OptionsWrapper<ResendServiceOptions>(_options),
            _resendMessageBuilderMock.Object, _resendSenderMock.Object);
    }

    [Fact]
    public void AdminEmail_ShouldReturnConfigurationEmail()
    {
        //Arrange
        var expectedEmail = "admin@test.com";

        //Act
        var result = _service.AdminEmail;

        //Assert
        Assert.Equal(expectedEmail, result);
    }

    [Fact]
    public void AdminEmail_ShouldReturnNullWhenNoConfiguration()
    {
        //Arrange
        var testService = new ResendService(new OptionsWrapper<ResendServiceOptions>(null), _resendMessageBuilderMock.Object, _resendSenderMock.Object);

        //Act
        var result = testService.AdminEmail;

        //Assert
        Assert.Null(result);
    }

    [Fact]
    public void AdminName_ShouldReturnConfigurationName()
    {
        //Arrange
        var expectedName = "John Smith";

        //Act
        var result = _service.AdminName;

        //Assert
        Assert.Equal(expectedName, result);
    }

    [Fact]
    public void AdminName_ShouldReturnNullWhenNoConfiguration()
    {
        //Arrange
        var testService = new ResendService(new OptionsWrapper<ResendServiceOptions>(null), _resendMessageBuilderMock.Object, _resendSenderMock.Object);

        //Act
        var result = testService.AdminName;

        //Assert
        Assert.Null(result);
    }

    [Fact]
    public void SendToAdministratorAsync_ShouldSend_DefaultingFromAndToAddress()
    {
        //Arrange
        var subject = "Test";
        var message = "Message";

        //Act
        _service.SendMessageToAdministratorAsync(subject, message);

        //Verify
    }

    [Fact]
    public void SendToAdministratorAsync_ShouldSend_DefaultingFromAndToAddress_WithCCRecipients()
    {
        //Arrange
        var subject = "Test";
        var message = "Message";
        var cc = new List<string> {"recipient@test.com"};

        //Act
        _service.SendMessageToAdministratorAsync(cc, subject, message);

        //Verify
    }

    [Fact]
    public void SendMessageAsync_WithoutCCRecipients_ShouldSend_DefaultingFromAddress()
    {
        //Arrange
        var to = "tester@test.com";
        var subject = "test";
        var message = "message";

        //Act
        _service.SendMessageAsync(to, subject, message);

        //Verify
    }

    [Fact]
    public void SendMessageAsync_WithCCRecipients_ShouldSend_DefaultingFromAddress()
    {
        //Arrange
        var to = "tester@test.com";
        var cc = new List<string> {"Person1@test.com"};
        var subject = "test";
        var message = "message";

        //Act
        _service.SendMessageAsync(to, cc, subject, message);

        //Verify
    }

    [Fact]
    public void SendMessageWithAttachmentAsync_ShouldSend_DefaultingFromAddress()
    {
        //Arrange
        var to = "tester@test.com";
        var cc = new List<string> { "Person1@test.com" };
        var subject = "test";
        var fileContent = Encoding.ASCII.GetBytes("Testing");
        var fileName = "test.txt";
        var message = "message";

        //Act
        _service.SendMessageWithAttachmentAsync(to, cc, subject, fileContent, fileName, message, null);

        //Assets
    }

    [Fact]
    public void SendMessageAsync_ShouldPassOptionalTemplateName_ToMessageMethods()
    {
        //Arrange
        var to = "tester@test.com";
        var cc = new List<string> { "Person1@test.com" };
        var subject = "test";
        var message = "message";
        var requestedTemplate = "Test";

        //Act
        _service.SendMessageAsync(to, cc, subject, message, null, requestedTemplate);

        //Assets
    }

    [Fact]
    public void SendMessageWithAttachmentAsync_ShouldPassOptionalTemplateName_ToMessageMethods()
    {
        //Arrange
        var to = "tester@test.com";
        var cc = new List<string> { "Person1@test.com" };
        var subject = "test";
        var fileContent = Encoding.ASCII.GetBytes("Testing");
        var fileName = "test.txt";
        var message = "message";
        var requestedTemplate = "Test";

        //Act
        _service.SendMessageWithAttachmentAsync(to, cc, subject, fileContent, fileName, message, null, requestedTemplate);

        //Assets
    }

    [Fact]
    public async void SendWithReplyToAsync_ShouldThrowArgumentException_WhenReplyToMissing()
    {
        //Arrange
        var to = "tester@test.com";
        var subject = "test";
        var message = "message";

        //Act/Assert
        var result = await Assert.ThrowsAsync<ArgumentNullException>(() => _service.SendWithReplyToAsync("", "", to, subject, message));
    }

    [Fact]
    public void SendWithReplyToAsync_WithoutCCRecipients_ShouldSend_DefaultingFromAddress()
    {
        //Arrange
        var replyTo = "sender@sendy.com";
        var to = "tester@test.com";
        var subject = "test";
        var message = "message";
        var returnMessage = new EmailMessage();
        _resendMessageBuilderMock
            .Setup(s => s.CreateMessage(_options.AdminEmail, _options.AdminName, to, null, subject, message,
                "")).Returns(returnMessage).Verifiable();

        //Act
        _service.SendWithReplyToAsync(replyTo, "", to, subject, message);

        //Verify
    }

    [Fact]
    public void SendWithReplyToAsync_WithCCRecipients_ShouldSend_DefaultingFromAddress()
    {
        //Arrange
        var replyTo = "sender@sendy.com";
        var to = "tester@test.com";
        var cc = new List<string> { "Person1@test.com" };
        var subject = "test";
        var message = "message";
        var returnMessage = new EmailMessage();
        _resendMessageBuilderMock
            .Setup(s => s.CreateMessage(_options.AdminEmail, _options.AdminName, to, cc, subject, message, "")).Returns(returnMessage).Verifiable();

        //Act
        _service.SendWithReplyToAsync(replyTo, "", to, cc, subject, message);

        //Verify
        _resendMessageBuilderMock.Verify();
    }

    [Fact]
    public void SendWithReplyToAsync_ShouldPassOptionalTemplateName_ToMessageMethods()
    {
        //Arrange
        var replyTo = "sender@sendy.com";
        var to = "tester@test.com";
        var cc = new List<string> { "Person1@test.com" };
        var subject = "test";
        var message = "message";
        var requestedTemplate = "Test";
        var returnMessage = new EmailMessage();
        _resendMessageBuilderMock
            .Setup(s => s.CreateMessage(_options.AdminEmail, _options.AdminName, to, cc, subject, message,
                requestedTemplate)).Returns(returnMessage).Verifiable();

        //Act
        _service.SendWithReplyToAsync(replyTo, "", to, cc, subject, message, null, requestedTemplate);

        //Assets
        _resendMessageBuilderMock.Verify();
    }

    [Fact]
    public async Task SendWithCustomFromEmailAsync_ShouldSend_WithBasicParameters()
    {
        var from = "from@test.com";
        var fromName = "Sender";
        var to = "to@test.com";
        var subject = "subject";
        var body = "body";
        var returnMessage = new EmailMessage();
        _resendMessageBuilderMock.Setup(s => s.CreateMessage(from, fromName, to, null, subject, body, "")).Returns(returnMessage).Verifiable();
        _resendSenderMock.Setup(s => s.SendMessage(returnMessage)).ReturnsAsync(true).Verifiable();
        var result = await _service.SendWithCustomFromEmailAsync(from, fromName, to, subject, body);
        Assert.True(result);
        _resendMessageBuilderMock.Verify();
        _resendSenderMock.Verify();
    }

    [Fact]
    public async Task SendWithCustomFromEmailAsync_ShouldSend_WithTokens()
    {
        var from = "from@test.com";
        var fromName = "Sender";
        var to = "to@test.com";
        var subject = "subject";
        var body = "body {Name}";
        var tokens = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("{Name}", "Test") };
        var expectedBody = "body Test";
        var returnMessage = new EmailMessage();
        _resendMessageBuilderMock.Setup(s => s.CreateMessage(from, fromName, to, null, subject, expectedBody, "")).Returns(returnMessage).Verifiable();
        _resendSenderMock.Setup(s => s.SendMessage(returnMessage)).ReturnsAsync(true).Verifiable();
        var result = await _service.SendWithCustomFromEmailAsync(from, fromName, to, subject, body, tokens);
        Assert.True(result);
        _resendMessageBuilderMock.Verify();
        _resendSenderMock.Verify();
    }

    [Fact]
    public async Task SendWithCustomFromEmailAsync_ShouldSend_WithCC()
    {
        var from = "from@test.com";
        var fromName = "Sender";
        var to = "to@test.com";
        var cc = new List<string> { "cc@test.com" };
        var subject = "subject";
        var body = "body";
        var returnMessage = new EmailMessage();
        _resendMessageBuilderMock.Setup(s => s.CreateMessage(from, fromName, to, cc, subject, body, "")).Returns(returnMessage).Verifiable();
        _resendSenderMock.Setup(s => s.SendMessage(returnMessage)).ReturnsAsync(true).Verifiable();
        var result = await _service.SendWithCustomFromEmailAsync(from, fromName, to, cc, subject, body);
        Assert.True(result);
        _resendMessageBuilderMock.Verify();
        _resendSenderMock.Verify();
    }

    [Fact]
    public async Task SendWithCustomFromEmailAsync_ShouldSend_WithCCAndTokens()
    {
        var from = "from@test.com";
        var fromName = "Sender";
        var to = "to@test.com";
        var cc = new List<string> { "cc@test.com" };
        var subject = "subject";
        var body = "body {Name}";
        var tokens = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("{Name}", "Test") };
        var expectedBody = "body Test";
        var returnMessage = new EmailMessage();
        _resendMessageBuilderMock.Setup(s => s.CreateMessage(from, fromName, to, cc, subject, expectedBody, "")).Returns(returnMessage).Verifiable();
        _resendSenderMock.Setup(s => s.SendMessage(returnMessage)).ReturnsAsync(true).Verifiable();
        var result = await _service.SendWithCustomFromEmailAsync(from, fromName, to, cc, subject, body, tokens);
        Assert.True(result);
        _resendMessageBuilderMock.Verify();
        _resendSenderMock.Verify();
    }

    [Fact]
    public async Task SendWithCustomFromEmailAsync_ShouldSend_WithAllParameters()
    {
        var from = "from@test.com";
        var fromName = "Sender";
        var to = "to@test.com";
        var cc = new List<string> { "cc@test.com" };
        var subject = "subject";
        var body = "body {Name}";
        var tokens = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("{Name}", "Test") };
        var expectedBody = "body Test";
        var template = "template";
        var returnMessage = new EmailMessage();
        _resendMessageBuilderMock.Setup(s => s.CreateMessage(from, fromName, to, cc, subject, expectedBody, template)).Returns(returnMessage).Verifiable();
        _resendSenderMock.Setup(s => s.SendMessage(returnMessage)).ReturnsAsync(true).Verifiable();
        var result = await _service.SendWithCustomFromEmailAsync(from, fromName, to, cc, subject, body, tokens, template);
        Assert.True(result);
        _resendMessageBuilderMock.Verify();
        _resendSenderMock.Verify();
    }

    [Fact]
    public async Task SendWithCustomFromEmailAndAttachmentAsync_ShouldSend_WithAllParameters()
    {
        var from = "from@test.com";
        var fromName = "Sender";
        var to = "to@test.com";
        var cc = new List<string> { "cc@test.com" };
        var subject = "subject";
        var fileContent = Encoding.ASCII.GetBytes("Attachment");
        var fileName = "file.txt";
        var body = "body {Name}";
        var tokens = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("{Name}", "Test") };
        var expectedBody = "body Test";
        var template = "template";
        var returnMessage = new EmailMessage();
        _resendMessageBuilderMock.Setup(s => s.CreateMessageWithAttachment(from, fromName, to, cc, fileContent, fileName, subject, expectedBody, template)).Returns(returnMessage).Verifiable();
        _resendSenderMock.Setup(s => s.SendMessage(returnMessage)).ReturnsAsync(true).Verifiable();
        var result = await _service.SendWithCustomFromEmailAndAttachmentAsync(from, fromName, to, cc, subject, fileContent, fileName, body, tokens, template);
        Assert.True(result);
        _resendMessageBuilderMock.Verify();
        _resendSenderMock.Verify();
    }
}