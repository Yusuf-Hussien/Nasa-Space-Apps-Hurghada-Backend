using Microsoft.AspNetCore.Mvc;
using NasaSpaceApps.Api.Common;
using NasaSpaceApps.Application.Common;

namespace NasaSpaceApps.Api.Tests;

public class ResultExtensionsTests
{
    private sealed class TestController : ControllerBase;

    [Fact]
    public void ToHttpResult_Should_ReturnOk_WithTheValue()
    {
        var result = Result<string>.Success("payload");

        var action = result.ToHttpResult(new TestController());

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        Assert.Equal("payload", ok.Value);
    }

    [Fact]
    public void ToHttpResult_Should_UseTheErrorContract_OnFailure()
    {
        var result = Result<string>.Failure("team.not_found", 404, "The team does not exist.");

        var action = result.ToHttpResult(new TestController());

        var status = Assert.IsType<ObjectResult>(action.Result);
        Assert.Equal(404, status.StatusCode);
        Assert.Equal(
            """{"error":{"code":"team.not_found","message":"The team does not exist."}}""",
            System.Text.Json.JsonSerializer.Serialize(status.Value));
    }

    [Fact]
    public void ToHttpResult_Should_MapTheValue_WhenMappingOverloadIsUsed()
    {
        var result = Result<int>.Success(7);

        var action = result.ToHttpResult(new TestController(), value => value * 2);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        Assert.Equal(14, ok.Value);
    }

    [Fact]
    public void ToHttpResult_Should_NotCallTheMapFunction_OnFailure()
    {
        var result = Result<int>.Failure("boom", 400, "nope");

        var action = result.ToHttpResult(new TestController(), value => value * 2);

        var status = Assert.IsType<ObjectResult>(action.Result);
        Assert.Equal(400, status.StatusCode);
    }
}
