// See https://aka.ms/new-console-template for more information

using System.Data;
using System.Net;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Oracle.ManagedDataAccess.Client;

Console.WriteLine("Hello, World!");
var builder = WebApplication.CreateBuilder(args);

//Auth0 code
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.Authority = "https://dev-gmprewiciwvaauxn.us.auth0.com";
    options.Audience = "eNIZK1CtP2TTXt6NUGF6AW4ghRfx6VKA";
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = ClaimTypes.NameIdentifier
    };
});

builder.Services.AddControllers();
builder.WebHost.ConfigureKestrel((context, serverOptions) =>
{
    serverOptions.Listen(IPAddress.Any, 80);
    serverOptions.Listen(IPAddress.Any, 443, listenOptions =>
    {
        listenOptions.UseHttps("_.sceneitbefore.org_private_key.pfx", "SceneItBeforeDB1");
    });
});

var app = builder.Build();

//Web socket code is in "WebSocketController.cs" file
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};
app.UseWebSockets(webSocketOptions);


app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
//    FileProvider = compositeProvider, //new PhysicalFileProvider(WebRoot),
    RequestPath = new PathString(""),
  //  ContentTypeProvider = extensionProvider,

    ServeUnknownFileTypes = true
});

//auth0stuff
app.UseAuthentication(); // Enable authentication middleware
app.UseAuthorization();  // Enable authorization middleware

app.MapControllers();

app.Run();
