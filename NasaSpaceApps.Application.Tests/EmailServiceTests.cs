using System.Collections.Concurrent;
using NasaSpaceApps.Application.Interfaces;
using NasaSpaceApps.Application.Models;
using NasaSpaceApps.Application.Services;

namespace NasaSpaceApps.Application.Tests;

public class EmailServiceTests
{
    private static EmailOptions Options(
        bool isConfigured = true,
        int maxRecipients = 100,
        string? configurationError = null) =>
        new(
            SmtpHost: "smtp.example.com",
            SmtpPort: 587,
            SmtpUsername: "user",
            SmtpPassword: "password",
            FromAddress: "info@spaceappshurghada.com",
            FromName: "NASA Space Apps Hurghada",
            UseSsl: true,
            MaxRecipients: maxRecipients,
            MaxConcurrentSends: 2,
            IsConfigured: isConfigured,
            ConfigurationError: configurationError);

    private static EmailService Service(RecordingTransport transport, EmailOptions? options = null) =>
        new(transport, options ?? Options());

    [Fact]
    public async Task SendAsync_DeduplicatesRecipientsAndSendsEachOnce()
    {
        var transport = new RecordingTransport();
        var service = Service(transport);

        var result = await service.SendAsync(
            "Event update",
            "See you soon.",
            new[] { "A@Example.com", "a@example.com", "b@example.com" });

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.RequestedRecipients);
        Assert.Equal(2, result.Value.UniqueRecipients);
        Assert.Equal(2, result.Value.Sent);
        Assert.Equal(0, result.Value.Failed);
        Assert.Equal(2, transport.Messages.Count);
    }

    [Fact]
    public async Task SendAsync_WhenNotConfigured_Returns503()
    {
        var service = Service(
            new RecordingTransport(),
            Options(isConfigured: false, configurationError: "EMAIL_SMTP_HOST is required."));

        var result = await service.SendAsync("Subject", "Body", new[] { "a@example.com" });

        Assert.False(result.IsSuccess);
        Assert.Equal("email.not_configured", result.Error!.Code);
        Assert.Equal(503, result.Error.HttpStatus);
    }

    [Fact]
    public async Task SendAsync_WhenSubjectOrBodyMissing_Returns400()
    {
        var service = Service(new RecordingTransport());

        var result = await service.SendAsync("", "Body", new[] { "a@example.com" });

        Assert.False(result.IsSuccess);
        Assert.Equal("email.invalid", result.Error!.Code);
        Assert.Equal(400, result.Error.HttpStatus);
    }

    [Fact]
    public async Task SendAsync_WhenSubjectContainsLineBreaks_Returns400()
    {
        var service = Service(new RecordingTransport());

        var result = await service.SendAsync(
            "Subject\r\nBcc: victim@example.com",
            "Body",
            new[] { "a@example.com" });

        Assert.False(result.IsSuccess);
        Assert.Equal("email.invalid", result.Error!.Code);
    }

    [Fact]
    public async Task SendAsync_WhenAllRecipientsInvalid_Returns400()
    {
        var service = Service(new RecordingTransport());

        var result = await service.SendAsync(
            "Subject",
            "Body",
            new[] { "not-an-email", "", "also bad" });

        Assert.False(result.IsSuccess);
        Assert.Equal("email.invalid_recipients", result.Error!.Code);
    }

    [Fact]
    public async Task SendAsync_WhenRecipientLimitExceeded_Returns400()
    {
        var service = Service(new RecordingTransport(), Options(maxRecipients: 2));

        var result = await service.SendAsync(
            "Subject",
            "Body",
            new[] { "a@example.com", "b@example.com", "c@example.com" });

        Assert.False(result.IsSuccess);
        Assert.Equal("email.too_many_recipients", result.Error!.Code);
    }

    [Fact]
    public async Task SendAsync_WhenOneTransportFails_ReportsPartialFailure()
    {
        var transport = new RecordingTransport { FailFor = "bad@example.com" };
        var service = Service(transport);

        var result = await service.SendAsync(
            "Subject",
            "Body",
            new[] { "good@example.com", "bad@example.com" });

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.Sent);
        Assert.Equal(1, result.Value.Failed);
        Assert.Single(result.Value.Failures);
        Assert.Equal("bad@example.com", result.Value.Failures.Single().Recipient);
    }

    private sealed class RecordingTransport : IEmailTransport
    {
        public ConcurrentBag<EmailMessage> Messages { get; } = new();
        public string? FailFor { get; init; }

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            if (string.Equals(message.Recipient, FailFor, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("SMTP rejected the recipient.");
            }

            Messages.Add(message);
            return Task.CompletedTask;
        }
    }
}
