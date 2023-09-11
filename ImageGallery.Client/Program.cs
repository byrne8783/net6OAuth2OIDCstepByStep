using ImageGallery.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(configure => 
        configure.JsonSerializerOptions.PropertyNamingPolicy = null);
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); //clear default claim type mappings

builder.Services.AddAccessTokenManagement();  

// create an HttpClient used for accessing the API
builder.Services.AddHttpClient("APIClient", client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["ImageGalleryAPIRoot"]);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
    })
    .AddUserAccessTokenHandler();  // a handler to add access tokens to each requests

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.AccessDeniedPath = "/Authentication/AccessDenied";
    })
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.Authority = "https://localhost:5001/"; //
        options.ClientId = "imagegalleryclient";   //we defined these in our IDP
        options.ClientSecret = "secret";
        options.ResponseType = "code";
        //options.Scope.Add("openid");              //these too, so he must be able too work them out
        //options.Scope.Add("profile");
        //options.CallbackPath = new PathString("signin-oidc");
        // SignedOutCallbackPath: default = host:port/signout-callback-oidc.
        // Must match with the post logout redirect URI at IDP client config if
        // you want to automatically return to the application after logging out
        // of IdentityServer.
        // To change, set SignedOutCallbackPath
        // eg: options.SignedOutCallbackPath = new PathString("pathaftersignout");
        // we're happy with the default
        options.SaveTokens = true; // tokens will be saved in a cookie
        options.GetClaimsFromUserInfoEndpoint = true;
        //get rid of claims in there by default
        options.ClaimActions.Remove("aud");
        options.ClaimActions.DeleteClaim("sid");
        options.ClaimActions.DeleteClaim("idp");
        options.Scope.Add("roles");  // one thats not included by default
         //options.Scope.Add("imagegalleryapi.fullaccess");
        options.Scope.Add("imagegalleryapi.read");
        options.Scope.Add("imagegalleryapi.write");
        options.Scope.Add("country");
        options.ClaimActions.MapJsonKey("role", "role");
        options.ClaimActions.MapUniqueJsonKey("country", "country");
        options.TokenValidationParameters = new()
        {
            NameClaimType = "given_name",
            RoleClaimType = "role",
        };
    });

builder.Services.AddAuthorization(authorizationOptions =>
{
    authorizationOptions.AddPolicy("UserCanAddImage",
        AuthorizationPolicies.CanAddImage());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Gallery}/{action=Index}/{id?}");

app.Run();
