using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Resend;

namespace ICG.NetCore.Utilities.Email.Resend;

/// <summary>
/// A wrapper for the Resend service to ensure that we can properly utilize the common ICG wrapping behaviors of the library
/// </summary>
public interface IResendSender
{
    /// <summary>
    /// Sends an email message using the Resend service
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task<bool> SendMessage(EmailMessage message);
}

/// <inheritdoc cref="IResendSender"/>
public class ResendSender(IResend resend, ILogger<ResendSender> logger) : IResendSender
{
    /// <inheritdoc cref="IResendSender" />
    public async Task<bool> SendMessage(EmailMessage message)
    {
        try
        {
            await resend.EmailSendAsync(message);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to send email");
            return false;
        }
    }
}