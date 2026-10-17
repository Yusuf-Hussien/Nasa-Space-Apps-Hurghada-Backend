namespace NasaSpaceApps.Application.Common;

public sealed record AppError(string Code, int HttpStatus, string Message);
