using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;
using Moq;
using Resend;
using Xunit;

namespace ICG.NetCore.Utilities.Email.Resend.Tests
{
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

        private readonly Mock<IResendMessageBuilder> _ResendMessageBuilderMock;
        private readonly Mock<IResendSender> _ResendSenderMock;
        private readonly IEmailService _service;

        public ResendServiceTests()
        {
            _ResendMessageBuilderMock = new Mock<IResendMessageBuilder>();
            _ResendSenderMock = new Mock<IResendSender>();
            _service = new ResendService(new OptionsWrapper<ResendServiceOptions>(_options),
                _ResendMessageBuilderMock.Object, _ResendSenderMock.Object);
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
            var testService = new ResendService(new OptionsWrapper<ResendServiceOptions>(null), _ResendMessageBuilderMock.Object, _ResendSenderMock.Object);

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
            var testService = new ResendService(new OptionsWrapper<ResendServiceOptions>(null), _ResendMessageBuilderMock.Object, _ResendSenderMock.Object);

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
            _ResendMessageBuilderMock
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
            _ResendMessageBuilderMock
                .Setup(s => s.CreateMessage(_options.AdminEmail, _options.AdminName, to, cc, subject, message, "")).Returns(returnMessage).Verifiable();

            //Act
            _service.SendWithReplyToAsync(replyTo, "", to, cc, subject, message);

            //Verify
            _ResendMessageBuilderMock.Verify();
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
            _ResendMessageBuilderMock
                .Setup(s => s.CreateMessage(_options.AdminEmail, _options.AdminName, to, cc, subject, message,
                    requestedTemplate)).Returns(returnMessage).Verifiable();

            //Act
            _service.SendWithReplyToAsync(replyTo, "", to, cc, subject, message, null, requestedTemplate);

            //Assets
            _ResendMessageBuilderMock.Verify();
        }
    }
}
