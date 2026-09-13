using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SmartPrompt.Application.Common.Interfaces;

namespace SmartPrompt.Infrastructure.Authentication;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            var idClaim = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(idClaim, out var id) ? id : null;
        }
    }
}
