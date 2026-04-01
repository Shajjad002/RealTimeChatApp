using System;
using API.Common;
using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Endpoints;

public static class AccountEndpoint
{
    public static RouteGroupBuilder MapAccountEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/register").WithTags("account");

        group.MapPost("/register", async (HttpContext context, UserManager<AppUser> userManager, [FromForm] string fullName, [FromForm] string email, [FromForm] string password) =>
        {
            var userFromDb = await userManager.FindByEmailAsync(email);
            if (userFromDb != null)
            {
                return Results.BadRequest(Response<string>.Failure("User with this email already exists."));
            }
            var user = new AppUser
            {
                FullName = fullName,
                Email = email,
            };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)          {
                return Results.BadRequest(Response<string>.Failure(result.Errors.FirstOrDefault()?.Description ?? "Failed to create user."));
            }
            return Results.Ok(Response<string>.Success("User registered successfully."));
        }   );

        return group;
    }
}
