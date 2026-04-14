using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NPA.Components;
using NPA.Data;
using NPA.Data.Models;
using NPA.Services;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<NpaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<NpaDbContext>()
.AddDefaultTokenProviders();

// Cookie config
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.AccessDeniedPath = "/account/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

// Entra ID (OpenID Connect) — uses placeholder values from appsettings
var azureAdSection = builder.Configuration.GetSection("AzureAd");
if (azureAdSection.Exists() && azureAdSection["ClientId"] != "YOUR-CLIENT-ID")
{
    builder.Services.AddAuthentication()
        .AddOpenIdConnect("EntraID", "Sign in with Entra ID", options =>
        {
            options.Authority = $"{azureAdSection["Instance"]}{azureAdSection["TenantId"]}/v2.0";
            options.ClientId = azureAdSection["ClientId"];
            options.ResponseType = "id_token";
            options.CallbackPath = azureAdSection["CallbackPath"] ?? "/signin-oidc";
            options.SignedOutCallbackPath = "/signout-oidc";
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
            options.TokenValidationParameters.NameClaimType = "name";
        });
}

// Application services
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<RiskService>();
builder.Services.AddScoped<IssueService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();

app.MapPost("/account/logout", async (HttpContext context, SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/account/login");
}).DisableAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(NPA.Client._Imports).Assembly);

// Seed database
await SeedData.InitializeAsync(app.Services);

app.Run();
