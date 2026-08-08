namespace AssignmentManagement.Api.Common;

public class ErrorEnvelope
{
    public string Message { get; set; } = null!;
    public IDictionary<string, string[]>? Errors { get; set; }
}
