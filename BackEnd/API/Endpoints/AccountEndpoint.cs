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
        var group = app.MapGroup("/api/account").WithTags("account");

        group.MapPost("/register", async (IFormCollection form, UserManager<AppUser> userManager) =>
        {
            try
            {
                var fullName = form["fullname"].ToString();
                var email = form["email"].ToString();
                var password = form["password"].ToString();
                var username = form["username"].ToString(); // Using email as username

                if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    return Results.BadRequest(Response<string>.Failure("FullName, Email, and Password are required."));
                }

                var userFromDb = await userManager.FindByEmailAsync(email);
                if (userFromDb != null)
                {
                    return Results.BadRequest(Response<string>.Failure("User with this email already exists."));
                }

                var user = new AppUser
                {
                    FullName = fullName,
                    Email = email,
                    UserName = username
                };

                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Results.BadRequest(Response<string>.Failure(errors));
                }

                return Results.Ok(Response<string>.Success("User registered successfully."));
            }
            catch (Exception ex)
            {
                return Results.Json(Response<string>.Failure($"Internal server error: {ex.Message}"), statusCode: 500);
            }
        }).DisableAntiforgery();

        return group;
    }
}
