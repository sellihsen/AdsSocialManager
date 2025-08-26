using FbAIAssistant.App.Components;
using FbAIAssistant.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// EF Core + Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
	.AddIdentityCore<ApplicationUser>(options =>
	{
		options.SignIn.RequireConfirmedAccount = false;
	})
	.AddRoles<IdentityRole>()
	.AddEntityFrameworkStores<ApplicationDbContext>()
	.AddSignInManager()
	.AddDefaultTokenProviders();

var auth = builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
	options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
	options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
});

auth.AddIdentityCookies();
auth.AddFacebook(FacebookDefaults.AuthenticationScheme, options =>
{
	options.AppId = builder.Configuration["Authentication:Facebook:AppId"] ?? string.Empty;
	options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"] ?? string.Empty;
	options.AccessDeniedPath = "/login/denied";
	options.CallbackPath = "/signin-facebook";
	options.SaveTokens = true;
	options.Scope.Add("public_profile");
	options.Scope.Add("email");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapGet("/login/facebook", async (HttpContext httpContext) =>
{
	var properties = new AuthenticationProperties { RedirectUri = "/" };
	await httpContext.ChallengeAsync(FacebookDefaults.AuthenticationScheme, properties);
});

app.MapGet("/logout", async (SignInManager<ApplicationUser> signInManager, HttpContext httpContext) =>
{
	await signInManager.SignOutAsync();
	httpContext.Response.Redirect("/");
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode();

app.Run();
