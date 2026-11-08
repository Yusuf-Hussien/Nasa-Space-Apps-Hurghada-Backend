using System.Net;
using System.Text.RegularExpressions;

namespace NasaSpaceApps.Application.Services;

/// <summary>
/// Builds the branded HTML and plain-text versions of every outbound email.
/// The signature and event links are shared so all admin emails stay consistent.
/// </summary>
public static class EmailTemplateBuilder
{
    public const string EventUrl = "https://www.spaceappschallenge.org/2026/local-events/hurghada/";
    public const string WebsiteUrl = "https://spaceappshurghada.com";
    public const string ContactEmail = "info@spaceappshurghada.com";
    public const string InstagramUrl = "https://www.instagram.com/spaceappshurghada/";
    public const string FacebookUrl = "https://www.facebook.com/spaceappshurghada";
    public const string LinkedInUrl = "https://www.linkedin.com/company/spaceappshurghada/";

    public static string BuildHtml(string subject, string body, bool isBodyHtml)
    {
        var safeSubject = WebUtility.HtmlEncode(subject);
        var bodyHtml = isBodyHtml ? body : ToHtml(body);

        return $$"""
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>{{safeSubject}}</title>
</head>
<body style="margin:0;padding:0;background-color:#f2f5f9;font-family:Arial,Helvetica,sans-serif;color:#1f2937;">
  <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background-color:#f2f5f9;padding:24px 12px;">
    <tr>
      <td align="center">
        <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="max-width:640px;background-color:#ffffff;border-radius:14px;overflow:hidden;border:1px solid #dce3ec;">
          <tr>
            <td style="background-color:#0b1f3a;padding:28px 32px;border-bottom:4px solid #e63946;">
              <div style="color:#ffffff;font-size:13px;letter-spacing:2px;text-transform:uppercase;">NASA Space Apps Challenge</div>
              <div style="color:#ffffff;font-size:26px;font-weight:700;margin-top:6px;">Hurghada 2026</div>
            </td>
          </tr>
          <tr>
            <td style="padding:32px;font-size:16px;line-height:1.65;color:#1f2937;">
              {{bodyHtml}}
            </td>
          </tr>
          <tr>
            <td style="padding:0 32px 32px 32px;">
              <div style="height:1px;background-color:#dce3ec;margin-bottom:22px;"></div>
              <div style="font-size:15px;font-weight:700;color:#0b1f3a;">NASA Space Apps Challenge 2026 - Hurghada</div>
              <div style="font-size:14px;line-height:1.8;color:#4b5563;margin-top:8px;">
                <a href="{{EventUrl}}" style="color:#e63946;text-decoration:none;font-weight:700;">Local Event Hurghada</a>
                &nbsp;|&nbsp;
                <a href="{{WebsiteUrl}}" style="color:#e63946;text-decoration:none;font-weight:700;">spaceappshurghada.com</a>
                <br />
                Email: <a href="mailto:{{ContactEmail}}" style="color:#e63946;text-decoration:none;">{{ContactEmail}}</a>
              </div>
              <div style="font-size:14px;line-height:1.8;color:#4b5563;margin-top:8px;">
                Follow us:
                <a href="{{InstagramUrl}}" style="color:#e63946;text-decoration:none;">Instagram</a>
                &nbsp;|&nbsp;
                <a href="{{FacebookUrl}}" style="color:#e63946;text-decoration:none;">Facebook</a>
                &nbsp;|&nbsp;
                <a href="{{LinkedInUrl}}" style="color:#e63946;text-decoration:none;">LinkedIn</a>
              </div>
            </td>
          </tr>
          <tr>
            <td style="background-color:#0b1f3a;padding:14px 32px;color:#cbd5e1;font-size:12px;text-align:center;">
              You are receiving this email because you are part of the NASA Space Apps Hurghada community.
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>
""";
    }

    public static string BuildText(string body, bool isBodyHtml)
    {
        var plainBody = isBodyHtml ? HtmlToPlainText(body) : body.Trim();

        return $"""
{plainBody}

----------------------------------------
NASA Space Apps Challenge 2026 - Hurghada
Local Event Hurghada: {EventUrl}
Website: {WebsiteUrl}
Email: {ContactEmail}
Instagram: {InstagramUrl}
Facebook: {FacebookUrl}
LinkedIn: {LinkedInUrl}
""";
    }

    private static string ToHtml(string body) =>
        WebUtility.HtmlEncode(body)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal)
            .Replace("\n", "<br />", StringComparison.Ordinal);

    private static string HtmlToPlainText(string html)
    {
        var withLineBreaks = Regex.Replace(html, "<br\\s*/?>", "\n", RegexOptions.IgnoreCase);
        withLineBreaks = Regex.Replace(withLineBreaks, "</p\\s*>", "\n\n", RegexOptions.IgnoreCase);
        withLineBreaks = Regex.Replace(withLineBreaks, "<[^>]+>", string.Empty);
        return WebUtility.HtmlDecode(withLineBreaks).Trim();
    }
}
