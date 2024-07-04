using System.Text;
using ChatService.Hubs;
using ChatService.Services;
using ChatService.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Shared.Models;

var builder = WebApplication.CreateBuilder(args);

#region Services

builder.Services.AddScoped<IMessageService, MessageService>();

#endregion

#region Database

builder.Services.Configure<ChatAppDbOptions>(builder.Configuration.GetSection("ChatAppDbOptions"));

#endregion

#region JWT Authentication and Authorization

var jwtSettingSection = builder.Configuration.GetSection("JwtOptions");
var jwtSettings = jwtSettingSection.Get<JwtOptions>();
builder.Services.Configure<JwtOptions>(jwtSettingSection);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtSettings!.Issuer,
            ValidAudience = jwtSettings!.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings!.Key)),

            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/Chat"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("Authentication failed: " + context.Exception.Message +
                                  context.Request.Headers.Authorization);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser().Build());

#endregion

#region Config

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddRazorPages();

builder.Logging.SetMinimumLevel(LogLevel.Debug);

#endregion

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<ChatHub>("/Chat").RequireAuthorization();

app.UseStaticFiles();
app.UseDefaultFiles();

app.Map("/", async context => { context.Response.Redirect("client.html"); });

app.Run();