using System;
using System.Security.Claims;

namespace Backend.Extensions;


//This is a C# extension class that adds custom helper methods to the built-in class ClaimsPrincipal.
//That means — you can call methods like:
// User.GetUserName()
// User.GetUserId()
//inside your controllers, hubs, or services wherever you have access to User.
public static class ClaimsPrincipleExtension
{
    public static string GetUserName(this ClaimsPrincipal user)
    {
        //Looks for a claim of type ClaimTypes.Name
        return user.FindFirstValue(ClaimTypes.Name) ?? throw new Exception("Cannot get Username");
    }//This is how you quickly get the username from the token claims.

    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        //Looks for a claim with type ClaimTypes.NameIdentifier (this usually holds the user’s ID).
        //Converts it into a Guid (because your user IDs are Guid type).
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("Cannot get UserId"));
        return userId;
    }

}
