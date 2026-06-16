using API.Common;
using API.DTOs;
using API.Extenions;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Endpoints;

public static class AccountEndpoint
{
    public static RouteGroupBuilder MapAccountEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/account").WithTags("account");

        group.MapPost("/register", async (HttpContext context, UserManager<AppUser> userManager, [FromForm] string fullName, [FromForm] string email, [FromForm] string password, [FromForm] string userName, [FromForm] IFormFile? profileImage) =>
        {

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(userName))
            {
                return Results.BadRequest(Response<string>.Failure("All fields are required."));
            }
            if (profileImage == null)
            {
                return Results.BadRequest(Response<string>.Failure("Profile image is required."));
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(profileImage.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                return Results.BadRequest(Response<string>.Failure("Invalid file type. Only JPG, JPEG, PNG, and GIF are allowed."));
            }
            var picture = await FileUpload.UploadFileAsync(profileImage);
            // You can store the fileName in the database associated with the user
            picture = $"{context.Request.Scheme}://{context.Request.Host}/uploads/{picture}";


            var userFromDb = await userManager.FindByEmailAsync(email);
            if (userFromDb != null)
            {
                return Results.BadRequest(Response<string>.Failure("User with this email already exists."));
            }


            var user = new AppUser
            {
                FullName = fullName,
                Email = email,
                UserName = userName,
                ProfileImage = picture

            };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return Results.BadRequest(Response<string>.Failure("User registration failed."));
            }
            return Results.Ok(Response<string>.Success("User registered successfully."));

        }).DisableAntiforgery();


        group.MapPost("/login", async (UserManager<AppUser> userManager, TokenService tokenService, LoginDto loginDto) =>
        {
            if (string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Password))
            {
                return Results.BadRequest(Response<string>.Failure("Email and password are required."));
            }

            var user = await userManager.FindByEmailAsync(loginDto.Email);
            if (user == null || !await userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return Results.BadRequest(Response<string>.Failure("Invalid email or password."));
            }

            var token = tokenService.GenerateToken(user.Id, user.UserName!);

            return Results.Ok(Response<string>.Success(token,"Login successful."));

        }).DisableAntiforgery();

        group.MapGet("/me", async (HttpContext context,UserManager<AppUser> userManager) =>
        {
            var currentLoggedInUserId = context.User.GetUserId();

            var currentLoggerInUser = await userManager.Users.SingleOrDefaultAsync(x=> x.Id == currentLoggedInUserId.ToString());
            return Results.Ok(Response<AppUser>.Success(currentLoggerInUser,"User retrieved successfully."));
               
        }).RequireAuthorization();

        return group;
    }
}
