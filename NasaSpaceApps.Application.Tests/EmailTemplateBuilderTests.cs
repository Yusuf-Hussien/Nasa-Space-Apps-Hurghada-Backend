using NasaSpaceApps.Application.Services;

namespace NasaSpaceApps.Application.Tests;

public class EmailTemplateBuilderTests
{
    [Fact]
    public void BuildHtml_IncludesBodyAndAllSignatureLinks()
    {
        var html = EmailTemplateBuilder.BuildHtml(
            "Important update",
            "Registration closes tonight.",
            isBodyHtml: false);

        Assert.Contains("Registration closes tonight.", html);
        Assert.Contains(EmailTemplateBuilder.EventUrl, html);
        Assert.Contains(EmailTemplateBuilder.WebsiteUrl, html);
        Assert.Contains(EmailTemplateBuilder.ContactEmail, html);
        Assert.Contains(EmailTemplateBuilder.InstagramUrl, html);
        Assert.Contains(EmailTemplateBuilder.FacebookUrl, html);
        Assert.Contains(EmailTemplateBuilder.LinkedInUrl, html);
    }

    [Fact]
    public void BuildHtml_EncodesPlainTextBody()
    {
        var html = EmailTemplateBuilder.BuildHtml(
            "Subject",
            "<script>alert('x')</script>\nSecond line",
            isBodyHtml: false);

        Assert.DoesNotContain("<script>", html);
        Assert.Contains("&lt;script&gt;", html);
        Assert.Contains("Second line", html);
    }

    [Fact]
    public void BuildHtml_PreservesHtmlBodyWhenRequested()
    {
        var html = EmailTemplateBuilder.BuildHtml(
            "Subject",
            "<strong>Bold update</strong>",
            isBodyHtml: true);

        Assert.Contains("<strong>Bold update</strong>", html);
    }

    [Fact]
    public void BuildText_IncludesSignatureAndStripsHtml()
    {
        var text = EmailTemplateBuilder.BuildText(
            "<p>Hello <strong>team</strong></p>",
            isBodyHtml: true);

        Assert.Contains("Hello team", text);
        Assert.Contains(EmailTemplateBuilder.EventUrl, text);
        Assert.Contains(EmailTemplateBuilder.WebsiteUrl, text);
        Assert.Contains(EmailTemplateBuilder.ContactEmail, text);
        Assert.Contains(EmailTemplateBuilder.InstagramUrl, text);
        Assert.Contains(EmailTemplateBuilder.FacebookUrl, text);
        Assert.Contains(EmailTemplateBuilder.LinkedInUrl, text);
    }
}
