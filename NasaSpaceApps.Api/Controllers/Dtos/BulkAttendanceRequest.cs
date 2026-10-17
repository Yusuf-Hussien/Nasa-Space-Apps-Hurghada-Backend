namespace NasaSpaceApps.Api.Controllers;

public sealed record BulkAttendanceRequest(DateTime EventDate, List<AttendanceEntryInput> Entries);
