using System.Collections.Concurrent;
using System.Net.Mail;
using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Interfaces;
using NasaSpaceApps.Application.Models;

namespace NasaSpaceApps.Application.Services;

/// <summary>
/// Admin bulk-email service. Each recipient gets an individual message so
/// addresses are never exposed to the rest of the list. Sends are throttled by
/// EMAIL_MAX_CONCURRENCY and capped by EMAIL_MAX_RECIPIENTS.
/// </summary>
public sealed class EmailService
{
    private readonly IEmailTransport _transport;
    private readonly EmailOptions _options;

    public EmailService(IEmailTransport transport, EmailOptions options)
    {
        _transport = transport;
        _options = options;
    }

    public async Task<Result<EmailSendResult>> SendAsync(
        string subject,
        string body,
        IEnumerable<string>? recipients,
        bool isBodyHtml = false,
        CancellationToken cancellationToken = default)
    {
        if (!_options.IsConfigured)
        {
            var detail = string.IsNullOrWhiteSpace(_options.ConfigurationError)
                ? "Set the EMAIL_* environment variables on the server."
                : _options.ConfigurationError;

            return Result<EmailSendResult>.Failure(
                "email.not_configured",
                503,
                $"Email service is not configured. {detail}");
        }

        subject = subject?.Trim() ?? string.Empty;
        body = body?.Trim() ?? string.Empty;

        if (subject.Length == 0 || body.Length == 0)
        {
            return Result<EmailSendResult>.Failure(
                "email.invalid",
                400,
                "Subject and body are required.");
        }

        if (subject.Contains('\r') || subject.Contains('\n'))
        {
            return Result<EmailSendResult>.Failure(
                "email.invalid",
                400,
                "Subject must not contain line breaks.");
        }

        var requested = recipients?.ToList() ?? new List<string>();
        if (requested.Count == 0)
        {
            return Result<EmailSendResult>.Failure(
                "email.invalid_recipients",
                400,
                "At least one recipient is required.");
        }

        var validRecipients = new List<string>();
        var invalidRecipients = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rawRecipient in requested)
        {
            var recipient = rawRecipient?.Trim() ?? string.Empty;

            if (recipient.Length == 0 || !MailAddress.TryCreate(recipient, out var mailAddress))
            {
                invalidRecipients.Add(recipient);
                continue;
            }

            var address = mailAddress.Address.Trim();
            if (seen.Add(address))
            {
                validRecipients.Add(address);
            }
        }

        if (validRecipients.Count == 0)
        {
            return Result<EmailSendResult>.Failure(
                "email.invalid_recipients",
                400,
                "No valid recipient email addresses were provided.");
        }

        if (validRecipients.Count > _options.MaxRecipients)
        {
            return Result<EmailSendResult>.Failure(
                "email.too_many_recipients",
                400,
                $"Too many recipients. The maximum per request is {_options.MaxRecipients}.");
        }

        var htmlBody = EmailTemplateBuilder.BuildHtml(subject, body, isBodyHtml);
        var textBody = EmailTemplateBuilder.BuildText(body, isBodyHtml);
        var failures = new ConcurrentBag<EmailFailure>();
        var sent = 0;

        using var concurrencyGate = new SemaphoreSlim(Math.Max(1, _options.MaxConcurrentSends));

        var sendTasks = validRecipients.Select(async recipient =>
        {
            await concurrencyGate.WaitAsync(cancellationToken);
            try
            {
                var message = new EmailMessage(
                    Recipient: recipient,
                    Subject: subject,
                    HtmlBody: htmlBody,
                    TextBody: textBody,
                    FromAddress: _options.FromAddress,
                    FromName: _options.FromName);

                await _transport.SendAsync(message, cancellationToken);
                Interlocked.Increment(ref sent);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                failures.Add(new EmailFailure(recipient, ex.Message));
            }
            finally
            {
                concurrencyGate.Release();
            }
        });

        await Task.WhenAll(sendTasks);

        return Result<EmailSendResult>.Success(new EmailSendResult(
            RequestedRecipients: requested.Count,
            UniqueRecipients: validRecipients.Count,
            Sent: sent,
            Failed: failures.Count,
            InvalidRecipients: invalidRecipients,
            Failures: failures.OrderBy(f => f.Recipient, StringComparer.OrdinalIgnoreCase).ToList()));
    }
}
