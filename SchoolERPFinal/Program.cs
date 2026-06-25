using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using SchoolERP.Net.Helpers;
using SchoolERP.Net.Services;
using SchoolERP.Net.Services.Clients;
using SchoolERP.Net.Utilities;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Minimal services for UI only
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

// Single shared API base address (point to SchoolERP.API)
var apiBaseUrl = builder.Configuration["ApiUrl:BaseUrl"] ?? "https://localhost:7205/";

builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddScoped<SchoolERP.Net.Helpers.PermissionHelper>();

// Add AuthClient
builder.Services.AddHttpClient<IAuthClientService, AuthClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
// Add AuthClient
builder.Services.AddHttpClient<IPhotoUploadService, PhotoUploadService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
// Add UserClient
builder.Services.AddHttpClient<IUserClientService, UserClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<IUserMenuPermissionClientService, UserMenuPermissionClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
// Add RoleClient
builder.Services.AddHttpClient<IRoleClientService, RoleClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Add UserTypeClient
builder.Services.AddHttpClient<IUserTypeClientService, UserTypeClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Add MenuClient
builder.Services.AddHttpClient<IMenuClientService, MenuClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Add SettingsClient
builder.Services.AddHttpClient<ISettingsClientService, SettingsClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Add UtilityClient
builder.Services.AddHttpClient<IUtilityClientService, UtilityClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Add OrganisationClient
builder.Services.AddHttpClient<IOrganisationClientService, OrganisationClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<ICompanyClientService, CompanyClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<ISessionClientService, SessionClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Add CurrencyClient
builder.Services.AddHttpClient<ICurrencyClientService, CurrencyClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Add LanguageClient
builder.Services.AddHttpClient<ILanguageClientService, LanguageClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IEmailConfigClientService, EmailConfigClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<ISmsConfigClientService, SmsConfigClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IPaymentMethodClientService, PaymentMethodClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<ISectionClientService, SectionClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IClassClientService, ClassClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<ISubjectClientService, SubjectClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<ISubjectGroupClientService, SubjectGroupClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IFrontOfficeClientService, FrontOfficeClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IHostelClientService, HostelClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IAccountHeadClientService, AccountHeadClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IAccountEntryClientService, AccountEntryClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IPickupPointClientService, PickupPointClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IRouteClientService, RouteClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IVehicleClientService, VehicleClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IVehicleAssignClientService, VehicleAssignClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IRoutePickupPointClientService, RoutePickupPointClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IHumanResourceClientService, HumanResourceClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IHomeworkClientService, HomeworkClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IStudentCertificateClientService, StudentCertificateClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IStudentIDCardClientService, StudentIDCardClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IStudentInformationClientService, StudentInformationClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});


builder.Services.AddHttpClient<IStaffIDCardClientService, StaffIDCardClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IAcademicsClientService, AcademicsClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IStudentLeaveClientService, StudentLeaveClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<IAttendanceClientService, AttendanceClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<IMenuApiClient, MenuApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<IAlumniEventClientService, AlumniEventClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<ILibraryClientService, LibraryClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<IFieldClientService, FieldClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<IDownloadCenterClientService, DownloadCenterClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<IDashboardClientService, DashboardClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<IUserManagementClientService, UserManagementClientService>(client =>
{
    client.BaseAddress = client.BaseAddress = new Uri(
        builder.Configuration["ApiSettings:BaseUrl"]);
});

builder.Services.AddHttpClient("SchoolERPApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// MVC
builder.Services.AddControllersWithViews();

// Global authorization: require authenticated user by default
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// JWT authentication (API issues tokens)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "SchoolERP_Default_Key_1234567890";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };

    // Read token from cookie for MVC page loads (UI stores token client-side)
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (string.IsNullOrEmpty(context.Token))
            {
                var path = context.Request.Path;
                if (!path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
                {
                    var cookieToken = context.Request.Cookies["token"];
                    if (!string.IsNullOrEmpty(cookieToken))
                    {
                        context.Token = cookieToken;
                    }
                }
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var path = context.Request.Path;
            if (!path.StartsWithSegments("/api") && !path.StartsWithSegments("/Auth"))
            {
                context.HandleResponse();
                context.Response.Redirect("/Auth/Login");
            }
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// If you still use LocalizationMiddleware in UI keep it; otherwise remove:
// app.UseMiddleware<LocalizationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Optional: Protect API routes served by this project (if any)
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    var isApiRoute = path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
    var isAnonymousApiRoute = path.StartsWithSegments("/api/auth/login", StringComparison.OrdinalIgnoreCase);

    if (isApiRoute && !isAnonymousApiRoute && context.User?.Identity?.IsAuthenticated != true)
    {
        await context.ChallengeAsync(JwtBearerDefaults.AuthenticationScheme);
        return;
    }

    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();