using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using WebService.Models;
using WebService.Models.Entities;

var builder = WebApplication.CreateBuilder(args);

#region Database and Identity

builder.Services.Configure<ChatAppDbSettings>(
    builder.Configuration.GetSection("ChatAppDbSettings"));

var linkyAppDbSettings = builder.Configuration.GetSection("ChatAppDbSettings").Get<ChatAppDbSettings>();

builder.Services.AddIdentityApiEndpoints<ChatUser>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 8;
        options.Password.RequiredUniqueChars = 1;

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = false;

        // User settings    
        options.User.RequireUniqueEmail = true;
    }).AddMongoDbStores<ChatUser, ChatRole, ObjectId>(
        linkyAppDbSettings?.ConnectionString,
        linkyAppDbSettings?.DatabaseName)
    .AddDefaultTokenProviders();

#endregion

#region Config

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#endregion

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityApi<ChatUser>();

app.Run();