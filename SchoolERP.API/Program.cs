using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using SchoolERP.API.Data;
using SchoolERP.API.Filters;
using SchoolERP.API.Interfaces;
using SchoolERP.API.Services;
using SchoolERP.API.Utilities;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<MenuPermissionAuthorizationFilter>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<MenuPermissionAuthorizationFilter>();
});
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

// Register Custom Helpers
builder.Services.AddSingleton<SqlHelper>();
builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddSingleton<EncryptionHelper>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IAuthServices, AuthServices>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IUserMenuPermissionService, UserMenuPermissionService>();
builder.Services.AddScoped<SchoolERP.API.Helpers.PermissionHelper>();
builder.Services.AddScoped<IFieldService, FieldService>();
builder.Services.AddScoped<IStudentInformationService, StudentInformationService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IRouteService, RouteService>();
builder.Services.AddScoped<IRoutePickupPointService, RoutePickupPointService>();
builder.Services.AddScoped<IHostelService, HostelService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<ISectionService, SectionService>();
builder.Services.AddScoped<IVehicleAssignService, VehicleAssignService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IAcademicsService, AcademicsService>();
builder.Services.AddScoped<IAccountEntryService, AccountEntryService>();
builder.Services.AddScoped<IAccountHeadService, AccountHeadService>();
builder.Services.AddScoped<IAlumniEventService, AlumniEventService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IDownloadCenterService, DownloadCenterService>();
builder.Services.AddScoped<IEmailConfigService, EmailConfigService>();
builder.Services.AddScoped<IFrontOfficeService, FrontOfficeService>();
builder.Services.AddScoped<IHomeworkService, HomeworkService>();
builder.Services.AddScoped<IHumanResourceService, HumanResourceService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILanguageService, LanguageService>();
builder.Services.AddScoped<ILibraryService, LibraryService>();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddScoped<IOrganisationService, OrganisationService>();
builder.Services.AddScoped<IPaymentMethodService, PaymentMethodService>();
builder.Services.AddScoped<IPickupPointService, PickupPointService>();
builder.Services.AddScoped<ISmsConfigService, SmsConfigService>();
builder.Services.AddScoped<IStaffIDCardService, StaffIDCardService>();
builder.Services.AddScoped<IStudentCertificateService, StudentCertificateService>();
builder.Services.AddScoped<IStudentIDCardService, StudentIDCardService>();
builder.Services.AddScoped<IStudentLeaveService, StudentLeaveService>();
builder.Services.AddScoped<ISubjectGroupService, SubjectGroupService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IDashboardApiService, DashboardApiService>();
builder.Services.AddScoped<IHostelTypeService, HostelTypeService>();
builder.Services.AddScoped<IRoomCoolingTypeService, RoomCoolingTypeService>();
builder.Services.AddScoped<IHolidayTypeService, HolidayTypeService>();
builder.Services.AddScoped<IHolidayService, HolidayService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IStateService, StateService>();
builder.Services.AddScoped<IWardensService, WardensService>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<IPostalCodeService, PostalCodeService>();
builder.Services.AddScoped<IWeeklyHolidaysSettingService, WeeklyHolidaysSettingService>();
builder.Services.AddScoped<ILibraryBudgetService, LibraryBudgetService>();
builder.Services.AddScoped<IPublisherService, PublisherService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<ILibraryLanguageService, LibraryLanguageService>();
builder.Services.AddScoped<ILibraryCategoryService, LibraryCategoryService>();
builder.Services.AddScoped<ILibrarySubjectService, LibrarySubjectService>();
builder.Services.AddScoped<ILibraryCategorySubjectService, LibraryCategorySubjectService>();
builder.Services.AddScoped<ILibraryDocumentTypeService, LibraryDocumentTypeService>();
builder.Services.AddScoped<ILibraryDocumentStatusService, LibraryDocumentStatusService>();
builder.Services.AddScoped<ILibrarySupplierService, LibrarySupplierService>();
builder.Services.AddScoped<ILibrarySeriesService, LibrarySeriesService>();

// Configure API Clients
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7237/"; // Default fallback


// Configure Global Authorization Policy
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMVC", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
// Configure JWT Authentication
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

    // Allow JWT auth for normal MVC page loads by reading token from cookie.
    // (Your login page stores the JWT client-side.)
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
            // If it's not an API request and we're not already heading to Login, redirect to Login page
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

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowMVC");
app.UseAuthorization();

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

app.MapControllers();

app.Run();
