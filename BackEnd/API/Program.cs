using System.Text;
using Anthropic;
using Anthropic.Models.Beta.Agents;
using Anthropic.Models.Beta.Messages;
using Anthropic.Models.Messages;
using API.Data;
using API.Endpoints;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var JwtSettings = builder.Configuration.GetSection("JwtSettings");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=chat.db"));

builder.Services.AddIdentityCore<AppUser>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<TokenService>();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {

        ValidateIssuerSigningKey = true,
        ValidIssuer = JwtSettings["Issuer"],
        ValidAudience = JwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT secret key is not configured. Please set JwtSettings:SecretKey in configuration."))),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
    };
});

// var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
// if (string.IsNullOrEmpty(apiKey))
// {
//     throw new InvalidOperationException("ANTHROPIC_API_KEY environment variable is not set.");
// }
// const string prompt = "Hello, how can I assist you today?";
// AnthropicClient anthropicClient = new()
// {
//     ApiKey = apiKey
// };
// Anthropic.Models.Beta.Messages.MessageCreateParams messageParams = new()
// {
//     Model = "claude-sonnet-4-20250514",
//     MaxTokens = 1024,
//     Messages =
//     [
//         new() { Role = Anthropic.Models.Beta.Messages.Role.User, Content = prompt }
//     ]
// };
//await SyncMessage(anthropicClient, messageParams);

// static async Task SyncMessage(AnthropicClient anthropicClient, Anthropic.Models.Beta.Messages.MessageCreateParams messageParams)
// {

//     var response = await anthropicClient.Beta.Messages.Create(messageParams);
//     var message = string.Join("",
//          response
//             .Content.Select(message => message.Value)
//             .OfType<TextBlock>()
//             .Select(textBlock => textBlock.Text)
    
//     );
//     Console.WriteLine(message);
//     Console.ReadLine();
// }

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapAccountEndpoints();

app.Run();


