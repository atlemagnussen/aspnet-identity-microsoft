using AspNetIdentity.Data;
using AspNetIdentity.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));


builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

var authMs = builder.Configuration.GetSection("Authentication:Microsoft");
builder.Services.Configure<OidcSettings>(authMs);
var authMsSettings = authMs.Get<OidcSettings>();

if (authMsSettings is null)
    throw new ApplicationException("missing auth ms");

builder.Services.AddAuthentication()
    .AddOpenIdConnect("Microsoft", "Microsoft", options =>
    {
        options.ClientId = authMsSettings.ClientId;
        options.Authority = "https://login.microsoftonline.com/common/v2.0";
        options.ClientSecret = authMsSettings.ClientSecret;
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.SaveTokens = true;
        options.CallbackPath = authMsSettings.CallbackPath;

        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        // ✅ Key part: custom issuer validation for multi-tenant
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            IssuerValidator = (issuer, securityToken, validationParameters) =>
            {
                if (issuer.StartsWith("https://login.microsoftonline.com/", StringComparison.OrdinalIgnoreCase))
                {
                    // Allow any valid Microsoft tenant
                    return issuer;
                }

                throw new SecurityTokenInvalidIssuerException($"Invalid issuer: {issuer}");
            }
        };
    });

builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
