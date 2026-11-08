using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NasaSpaceApps.Api.Common;
using NasaSpaceApps.Application.Services;

namespace NasaSpaceApps.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class EmailController : ControllerBase
{
    private readonly EmailService _emailService;

    public EmailController(EmailService emailService)
    {
        _emailService = emailService;
    }

    /// <summary>Send a branded email to one or many recipients. Admin only.</summary>
    [HttpPost("send")]
    public async Task<ActionResult<SendEmailResponse>> Send(
        [FromBody] SendEmailRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _emailService.SendAsync(
            request.Subject,
            request.Body,
            request.Recipients,
            request.IsBodyHtml,
            cancellationToken);

        return result.ToHttpResult(this, SendEmailResponse.From);
    }
}
