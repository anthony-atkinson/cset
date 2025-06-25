using CSETWebBlazor.Services;
using CSETWebBlazor.Data;
using CSETWebBlazor.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add Authentication
builder.Services.AddAuthentication()
    .AddJwtBearer();

// Add Authorization
builder.Services.AddAuthorization();

// Add SignalR
builder.Services.AddSignalR();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Add CSET Services
builder.Services.AddScoped<IChartService, ChartService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<ICodeEditorService, CodeEditorService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IRealTimeService, RealTimeService>();
builder.Services.AddScoped<IErrorHandlingService, ErrorHandlingService>();
builder.Services.AddScoped<IValidationService, ValidationService>();

// Add Data Services
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IConfigService, ConfigService>();

// Add Authentication Services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ICsetAuthorizationService, CsetAuthorizationService>();
builder.Services.AddScoped<AuthenticationStateProvider, CsetAuthenticationStateProvider>();

// Add API Client Service
builder.Services.AddScoped<IApiClientService, ApiClientService>();

// Add Background Services
builder.Services.AddHostedService<FileCleanupService>();

// Add HttpClient
builder.Services.AddHttpClient();

// Add Memory Cache
builder.Services.AddMemoryCache();

// Add Distributed Cache (Redis if available)
builder.Services.AddDistributedMemoryCache();

// Add Protected Browser Storage
builder.Services.AddProtectedBrowserStorage();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapHub<CSETHub>("/csetHub");
app.MapFallbackToPage("/_Host");

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}

app.Run();
