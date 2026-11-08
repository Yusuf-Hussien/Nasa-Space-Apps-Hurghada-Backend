using System.Net;
using System.Net.Mail;
using System.Text;
using NasaSpaceApps.Application.Interfaces;
using NasaSpaceApps.Application.Models;

namespace NasaSpaceApps.Infrastructure.Services;

/// <summary>SMTP implementation of the outbound email transport.</summary>
public sealed class SmtpEmailTransport : IEmailTransport
{
    private const int SendTimeoutMilliseconds = 30_000;

    private readonly EmailOptions _options;

    public SmtpEmailTransport(EmailOptions options)
    {
        _options = options;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        using var mail = new MailMessage
        {
            From = new MailAddress(message.FromAddress, message.FromName),
            Subject = message.Subject,
            SubjectEncoding = Encoding.UTF8,
            Body = message.HtmlBody,
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = true
        };

        mail.To.Add(new MailAddress(message.Recipient));
        mail.AlternateViews.Add(
            AlternateView.CreateAlternateViewFromString(message.TextBody, Encoding.UTF8, "text/plain"));

        using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.UseSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Timeout = SendTimeoutMilliseconds
        };

        if (!string.IsNullOrWhiteSpace(_options.SmtpUsername))
        {
            client.Credentials = new NetworkCredential(_options.SmtpUsername, _options.SmtpPassword);
        }

        await client.SendMailAsync(mail, cancellationToken);
    }
}
