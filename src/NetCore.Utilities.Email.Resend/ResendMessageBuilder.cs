using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resend;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ICG.NetCore.Utilities.Email.Resend;

/// <summary>
/// The service takes an incoming request and build the proper Resend message structure for composing the messages
/// </summary>
public interface IResendMessageBuilder
{
    /// <summary>
    /// Creates a simple message for sending with a custom template
    /// </summary>
    /// <param name="from">Who the message is from</param>
    /// <param name="fromName">The name of the sender</param>
    /// <param name="to">Who the message is to</param>
    /// <param name="cc">An optional listing of CC addresses</param>
    /// <param name="subject">The subject of the message</param>
    /// <param name="bodyHtml">The Email's HTML content</param>
    /// <param name="templateName">The name of the template to use</param>
    /// <returns></returns>
    EmailMessage CreateMessage(string from, string fromName, string to, IEnumerable<string> cc, string subject, string bodyHtml,
        string templateName = "");

    /// <summary>
    /// Creates a simple message for sending with a custom template and attachment
    /// </summary>
    /// <param name="from">Who the message is from</param>
    /// <param name="fromName">The name of the sender</param>
    /// <param name="to">Who the message is to</param>
    /// <param name="cc">An optional listing of CC addresses</param>
    /// <param name="fileContent">The content of the attachment in bytes</param>
    /// <param name="fileName">The desired name for the file attachment</param>
    /// <param name="subject">The subject of the message</param>
    /// <param name="bodyHtml">The Email's HTML content</param>
    /// <param name="templateName">The name of the template to use</param>
    EmailMessage CreateMessageWithAttachment(string from, string fromName, string to, IEnumerable<string> cc,
        byte[] fileContent, string fileName, string subject, string bodyHtml, string templateName = "");
}

/// <inheritdoc cref="IResendMessageBuilder"/>
public class ResendMessageBuilder : IResendMessageBuilder
{
    private readonly IHostEnvironment _hostingEnvironment;
    private readonly IEmailTemplateFactory _emailTemplateFactory;
    private readonly ResendServiceOptions _serviceOptions;
    private readonly ILogger _logger;

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="hostingEnvironment"></param>
    /// <param name="emailTemplateFactory"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ResendMessageBuilder(IHostEnvironment hostingEnvironment, IEmailTemplateFactory emailTemplateFactory, 
        IOptions<ResendServiceOptions> options, ILogger<ResendMessageBuilder> logger)
    {
        _hostingEnvironment = hostingEnvironment;
        _emailTemplateFactory = emailTemplateFactory;
        _serviceOptions = options.Value;
        _logger = logger;
    }
        
    /// <inheritdoc />
    public EmailMessage CreateMessage(string from, string fromName, string to, IEnumerable<string> cc, string subject, string bodyHtml, string templateName = "")
    {
        //Validate inputs
        if (string.IsNullOrEmpty(from))
            throw new ArgumentNullException(nameof(from));
        if (string.IsNullOrEmpty(to))
            throw new ArgumentNullException(nameof(to));
        if (string.IsNullOrEmpty(subject))
            throw new ArgumentNullException(nameof(subject));
        if (string.IsNullOrEmpty(bodyHtml))
            throw new ArgumentNullException(nameof(bodyHtml));

        //Setup the default addresses
        var message = new EmailMessage();
        message.From = new EmailAddress
        {
            Email = from,
            DisplayName = string.IsNullOrEmpty(fromName) ? null : fromName
        };
        message.To.Add(to);

        //If we have CC's add them as well
        if (cc != null)
        {
            message.Cc = new EmailAddressList();
            foreach (var item in cc)
            {
                try
                {
                    var toAdd = new EmailAddress{Email = item};
                    message.Cc.Add(toAdd);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Unable to add {item} to email copy list");
                }
            }
        }
            
        //Adjust subject if needed for environment, then set it
        if (_serviceOptions.AddEnvironmentSuffix && !_hostingEnvironment.IsProduction())
        {
            subject = $"{subject} ({_hostingEnvironment.EnvironmentName})";
        }
        message.Subject = subject;
            

        //Perform templating, if needed, and then set the body
        if (_serviceOptions.AlwaysTemplateEmails && string.IsNullOrEmpty(templateName))
        {
            bodyHtml = _emailTemplateFactory.BuildEmailContent(subject, bodyHtml);
        }
        else if (!string.IsNullOrEmpty(templateName))
        {
            bodyHtml = _emailTemplateFactory.BuildEmailContent(subject, bodyHtml, templateName: templateName);
        }
        message.HtmlBody = bodyHtml;
        message.TextBody = Regex.Replace(bodyHtml, "<[^>]*>", "");

        return message;
    }

    /// <inheritdoc />
    public EmailMessage CreateMessageWithAttachment(string from, string fromName, string to, IEnumerable<string> cc,
        byte[] fileContent, string fileName, string subject, string bodyHtml, string templateName = "")
    {
        //Validate inputs
        if (fileContent == null || fileContent.Length == 0)
        {
            throw new ArgumentNullException(nameof(fileContent), "File content cannot be null or empty");
        }

        //Build the  basic message
        var toSend = CreateMessage(from, fromName, to, cc, subject, bodyHtml, templateName);

        //Attach file
        toSend.Attachments = new List<EmailAttachment>();
        toSend.Attachments.Add(new EmailAttachment
        {
            Content = fileContent,
            Filename = fileName,
        });

        return toSend;
    }

}