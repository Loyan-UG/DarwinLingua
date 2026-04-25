namespace DarwinLingua.WebApi.Models;

public sealed record AuthenticatedUserResponse(
    string UserId,
    string? Email,
    bool IsAuthenticated,
    IReadOnlyList<string> Roles);
