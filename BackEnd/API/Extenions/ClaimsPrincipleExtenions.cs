using System.Security.Claims;

namespace API.Extenions;
public static class ClaimsPrincipleExtenions
{
    public static string GetUserName(this ClaimsPrincipal user)
    {
       return user.FindFirstValue(ClaimTypes.Name)?? throw new Exception("Can not get user Name");
    }

    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        return Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("Can not get user ID"));
        
    }
}