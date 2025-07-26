using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Resend;

namespace ICG.NetCore.Utilities.Email.Resend;

/// <inheritdoc />
public class ResendService : IEmailService
{
    private readonly IResendMessageBuilder _messageBuilder;
    private readonly ResendServiceOptions _serviceOptions;
    private readonly IResendSender _resendSender;

    /// <summary>
    ///     DI Capable Constructor for Resend message delivery using MimeKit/MailKit
    /// </summary>
    /// <param name="serviceOptions">The injected configuration elements</param>
    /// <param name="messageBuilder">The message builder for token replacement</param>
    /// <param name="resendSender">AN ICG Wrapper to the Resend Service for delivery of emails</param>
    public ResendService(IOptions<ResendServiceOptions> serviceOptions, IResendMessageBuilder messageBuilder, IResendSender resendSender)
    {
        _messageBuilder = messageBuilder;
        _resendSender = resendSender;
        _serviceOptions = serviceOptions.Value;
    }

    /// <inheritdoc />
    public string AdminEmail => _serviceOptions?.AdminEmail;

    /// <inheritdoc />
    public string AdminName => _serviceOptions?.AdminName;

    /// <inheritdoc />
    public Task<bool> SendMessageToAdministratorAsync(string subject, string bodyHtml)
    {
        //Force to address
        return SendMessageAsync(_serviceOptions.AdminEmail, null, subject, bodyHtml);
    }

    /// <inheritdoc />
    public Task<bool> SendMessageToAdministratorAsync(IEnumerable<string> ccAddressList, string subject,
        string bodyHtml)
    {
        return SendMessageAsync(_serviceOptions.AdminEmail, ccAddressList, subject, bodyHtml);
    }

    /// <inheritdoc />
    public Task<bool> SendMessageAsync(string toAddress, string subject, string bodyHtml)
    {
        //Call full overload
        return SendMessageAsync(toAddress, null, subject, bodyHtml);
    }

    /// <inheritdoc />
    public Task<bool> SendMessageAsync(string toAddress, string subject, string bodyHtml,
        List<KeyValuePair<string, string>> tokens)
    {
        return SendMessageAsync(toAddress, null, subject, bodyHtml, tokens, "");
    }

    /// <inheritdoc />
    public Task<bool> SendMessageAsync(string toAddress, IEnumerable<string> ccAddressList, string subject,
        string bodyHtml)
    {
        return SendMessageAsync(toAddress, ccAddressList, subject, bodyHtml, null, "");
    }

    /// <inheritdoc />
    public Task<bool> SendMessageAsync(string toAddress, IEnumerable<string> ccAddressList, string subject,
        string bodyHtml, List<KeyValuePair<string, string>> tokens)
    {
        return SendMessageAsync(toAddress, ccAddressList, subject, bodyHtml, tokens, "");
    }


    /// <inheritdoc />
    public Task<bool> SendMessageAsync(string toAddress, IEnumerable<string> ccAddressList, string subject,
        string bodyHtml,
        List<KeyValuePair<string, string>> tokens,
        string templateName, string senderKeyName = "")
    {
        if (!string.IsNullOrEmpty(senderKeyName))
        {
            throw new NotSupportedException("This service does not support a custom sender key at this time");
        }

        if (tokens != null)
            foreach (var item in tokens)
                bodyHtml = bodyHtml.Replace(item.Key, item.Value);

        //Get the message to send
        var toSend = _messageBuilder.CreateMessage(_serviceOptions.AdminEmail, _serviceOptions.AdminName, toAddress,
            ccAddressList, subject,
            bodyHtml, templateName);

        //Send
        return _resendSender.SendMessage(toSend);
    }

    /// <inheritdoc />
    public Task<bool> SendWithReplyToAsync(string replyToAddress, string replyToName, string toAddress, string subject,
        string bodyHtml)
    {
        //Call full overload
        return SendWithReplyToAsync(replyToAddress, replyToName, toAddress, null, subject, bodyHtml);
    }

    /// <inheritdoc />
    public Task<bool> SendWithReplyToAsync(string replyToAddress, string replyToName, string toAddress, string subject,
        string bodyHtml, List<KeyValuePair<string, string>> tokens)
    {
        return SendWithReplyToAsync(replyToAddress, replyToName, toAddress, null, subject, bodyHtml, null, "");
    }

    /// <inheritdoc />
    public Task<bool> SendWithReplyToAsync(string replyToAddress, string replyToName, string toAddress,
        IEnumerable<string> ccAddressList, string subject, string bodyHtml)
    {
        return SendWithReplyToAsync(replyToAddress, replyToName, toAddress, ccAddressList, subject, bodyHtml, null, "");
    }

    /// <inheritdoc />
    public Task<bool> SendWithReplyToAsync(string replyToAddress, string replyToName, string toAddress,
        IEnumerable<string> ccAddressList, string subject, string bodyHtml, List<KeyValuePair<string, string>> tokens)
    {
        return SendWithReplyToAsync(replyToAddress, replyToName, toAddress, ccAddressList, subject, bodyHtml, tokens,
            "");
    }


    /// <inheritdoc />
    public Task<bool> SendWithReplyToAsync(string replyToAddress, string replyToName, string toAddress,
        IEnumerable<string> ccAddressList, string subject, string bodyHtml,
        List<KeyValuePair<string, string>> tokens,
        string templateName, string senderKeyName = "")
    {
        if (string.IsNullOrEmpty(replyToAddress))
        {
            throw new ArgumentNullException(nameof(replyToAddress));
        }

        if (!string.IsNullOrEmpty(senderKeyName))
        {
            throw new NotSupportedException("This service does not support a custom sender key at this time");
        }

        if (tokens != null)
            foreach (var item in tokens)
                bodyHtml = bodyHtml.Replace(item.Key, item.Value);

        //Get the message to send
        var toSend = _messageBuilder.CreateMessage(_serviceOptions.AdminEmail, _serviceOptions.AdminName, toAddress,
            ccAddressList, subject,
            bodyHtml, templateName);

        if (!string.IsNullOrEmpty(replyToAddress))
        {
            var replyTo = new EmailAddress
            {
                Email = replyToAddress,
                DisplayName = string.IsNullOrEmpty(replyToName) ? null : replyToName
            };
            toSend.ReplyTo = [replyTo];
        }

        //Send
        return _resendSender.SendMessage(toSend);
    }

    /// <inheritdoc />
    public Task<bool> SendMessageWithAttachmentAsync(string toAddress, IEnumerable<string> ccAddressList,
        string subject,
        byte[] fileContent, string fileName, string bodyHtml, List<KeyValuePair<string, string>> tokens,
        string templateName = "", string senderKeyName = "")
    {
        if (!string.IsNullOrEmpty(senderKeyName))
        {
            throw new NotSupportedException("This service does not support a custom sender key at this time");
        }

        if (tokens != null)
            foreach (var item in tokens)
                bodyHtml = bodyHtml.Replace(item.Key, item.Value);

        //Get the message to send
        var toSend = _messageBuilder.CreateMessageWithAttachment(_serviceOptions.AdminEmail, _serviceOptions.AdminName,
            toAddress,
            ccAddressList, fileContent, fileName, subject, bodyHtml, templateName);
        
        //Send
        return _resendSender.SendMessage(toSend);
    }
}