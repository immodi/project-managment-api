using System.Security.Claims;

namespace API.Extensions;

public static class ClaimsExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? user.FindFirstValue("sub");

        return Guid.Parse(id!);
    }
}