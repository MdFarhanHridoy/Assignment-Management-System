namespace AssignmentManagement.Application.Common.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "assignment-management-api";
    public string Audience { get; set; } = "assignment-management-client";
    public int ExpiryMinutes { get; set; } = 120;
}
