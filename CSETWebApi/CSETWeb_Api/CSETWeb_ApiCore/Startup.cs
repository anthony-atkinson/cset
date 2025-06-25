//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.Business.AdminTab;
using CSETWebCore.Business.Aggregation;
using CSETWebCore.Business.Assessment;
using CSETWebCore.Business.Common;
using CSETWebCore.Business.Contact;
using CSETWebCore.Business.Demographic;
using CSETWebCore.Business.Diagram;
using CSETWebCore.Business.Document;
using CSETWebCore.Business.FileRepository;
using CSETWebCore.Business.Framework;
using CSETWebCore.Business.Maturity;
using CSETWebCore.Business.ModuleBuilder;
using CSETWebCore.Business.Notification;
using CSETWebCore.Business.Question;
using CSETWebCore.Business.ReportEngine;
using CSETWebCore.Business.Reports;
using CSETWebCore.Business.RepositoryLibrary;
using CSETWebCore.Business.Sal;
using CSETWebCore.Business.Standards;
using CSETWebCore.Business.User;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Helpers;
using CSETWebCore.Interfaces;
using CSETWebCore.Interfaces.AdminTab;
using CSETWebCore.Interfaces.Aggregation;
using CSETWebCore.Interfaces.Assessment;
using CSETWebCore.Interfaces.Common;
using CSETWebCore.Interfaces.Contact;
using CSETWebCore.Interfaces.Demographic;
using CSETWebCore.Interfaces.Document;
using CSETWebCore.Interfaces.FileRepository;
using CSETWebCore.Interfaces.Framework;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Interfaces.Maturity;
using CSETWebCore.Interfaces.ModuleBuilder;
using CSETWebCore.Interfaces.Notification;
using CSETWebCore.Interfaces.Question;
using CSETWebCore.Interfaces.ReportEngine;
using CSETWebCore.Interfaces.Reports;
using CSETWebCore.Interfaces.ResourceLibrary;
using CSETWebCore.Interfaces.Sal;
using CSETWebCore.Interfaces.Standards;
using CSETWebCore.Interfaces.User;
using CSETWebCore.Business.GalleryParser;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Rewrite;
using CSETWebCore.Interfaces.Analytics;
using CSETWebCore.Business.Analytics;
using CSETWebCore.Api.Error;
using System;
using CSETWebCore.Business.AssessmentIO.Import;
using CSETWebCore.Interfaces.Malcolm;
using CSETWebCore.Business.Malcolm;
using CSETWebCore.Interfaces.Cmu;
using CSETWebCore.Business.Version;
using CSETWebCore.Interfaces.Version;
using CSETWebCore.Business.Demographic.Import;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using CSETWeb_ApiCore.Swagger;
using Microsoft.ApplicationInsights.Extensibility;
using CSETWebCore.Business.Collaboration;
using Microsoft.AspNetCore.SignalR;
using CSETWeb_ApiCore.Models.Caching;
using CSETWeb_ApiCore.Interfaces;
using CSETWeb_ApiCore.Services;

namespace CSETWeb_ApiCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(o =>
            {
                o.AddPolicy(
                    name: "AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .WithExposedHeaders("Content-Disposition");
                    });
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
            services.AddAuthorization();
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

                }).AddXmlDataContractSerializerFormatters();
            services.AddHttpContextAccessor();
            
            // Configure database with performance monitoring
            services.AddDbContext<CSETContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("CSET_DB"));
                
                // Add performance interceptor
                var interceptor = serviceProvider.GetService<DatabasePerformanceInterceptor>();
                if (interceptor != null)
                {
                    options.AddInterceptors(interceptor);
                }
            });

            // Application Insights Configuration
            services.AddApplicationInsightsTelemetry(options =>
            {
                options.InstrumentationKey = Configuration["ApplicationInsights:InstrumentationKey"];
                options.EnableAdaptiveSampling = Configuration.GetValue<bool>("ApplicationInsights:EnableAdaptiveSampling", true);
                options.EnablePerformanceCounterCollectionModule = Configuration.GetValue<bool>("ApplicationInsights:EnablePerformanceCounterCollectionModule", true);
                options.EnableDependencyTrackingTelemetryModule = Configuration.GetValue<bool>("ApplicationInsights:EnableDependencyTrackingTelemetryModule", true);
                options.EnableQuickPulseMetricStream = Configuration.GetValue<bool>("ApplicationInsights:EnableQuickPulseMetricStream", true);
                options.EnableHeartbeat = Configuration.GetValue<bool>("ApplicationInsights:EnableHeartbeat", true);
                options.EnableAzureInstanceMetadataTelemetryModule = Configuration.GetValue<bool>("ApplicationInsights:EnableAzureInstanceMetadataTelemetryModule", true);
                options.EnableEventCounterCollectionModule = Configuration.GetValue<bool>("ApplicationInsights:EnableEventCounterCollectionModule", true);
                options.EnableDiagnosticsTelemetryModule = Configuration.GetValue<bool>("ApplicationInsights:EnableDiagnosticsTelemetryModule", true);
            });

            // Configure Application Insights sampling
            services.Configure<ApplicationInsightsServiceOptions>(options =>
            {
                var samplingSettings = Configuration.GetSection("ApplicationInsights:SamplingSettings");
                if (samplingSettings.Exists())
                {
                    options.EnableAdaptiveSampling = true;
                    options.EnableFixedRateSampling = false;
                }
            });

            //Services
            services.AddTransient<IAdminTabBusiness, AdminTabBusiness>();
            services.AddTransient<IAnalyticsBusiness, AnalyticsBusiness>();
            services.AddTransient<IAdvancedAnalyticsBusiness, AdvancedAnalyticsBusiness>();
            services.AddTransient<IAssessmentBusiness, AssessmentBusiness>();
            services.AddTransient<IAssessmentModeData, AssessmentModeData>();
            services.AddTransient<IAssessmentUtil, AssessmentUtil>();
            services.AddTransient<IContactBusiness, ContactBusiness>();
            services.AddTransient<IDemographicBusiness, DemographicBusiness>();
            services.AddTransient<ICisDemographicBusiness, CisDemographicBusiness>();
            services.AddTransient<IDiagramManager, DiagramManager>();
            services.AddTransient<IDocumentBusiness, DocumentBusiness>();
            services.AddTransient<IHtmlFromXamlConverter, HtmlFromXamlConverter>();
            services.AddTransient<IMaturityBusiness, MaturityBusiness>();
            services.AddTransient<INotificationBusiness, NotificationBusiness>();
            services.AddTransient<IParameterContainer, ParameterContainer>();
            services.AddTransient<IPasswordHash, PasswordHash>();
            services.AddTransient<IQuestionBusiness, QuestionBusiness>();
            services.AddTransient<IQuestionPoco, QuestionPoco>();
            services.AddTransient<IQuestionRequirementManager, QuestionRequirementManager>();
            services.AddTransient<IRequirementBusiness, RequirementBusiness>();
            services.AddTransient<IResourceHelper, ResourceHelper>();
            services.AddTransient<ISalBusiness, SalBusiness>();
            services.AddTransient<IStandardsBusiness, StandardsBusiness>();
            services.AddTransient<IStandardSpecficLevelRepository, StandardSpecficLevelRepository>();
            services.AddTransient<ITokenManager, TokenManager>();
            services.AddTransient<ICmuScoringHelper, CmuScoringHelper>();
            services.AddTransient<IApiKeyManager, ApiKeyManager>();
            services.AddTransient<IImportManager, ImportManager>();
            services.AddTransient<IDemographicImportManager, DemographicImportManager>();
            services.AddTransient<ILocalInstallationHelper, LocalInstallationHelper>();
            services.AddTransient<IUserAuthentication, UserAuthentication>();
            services.AddTransient<IUserBusiness, UserBusiness>();
            services.AddTransient<IUtilities, Utilities>();
            services.AddTransient<ITrendDataProcessor, TrendDataProcessor>();
            services.AddTransient<IReportsDataBusiness, ReportsDataBusiness>();
            services.AddTransient<IAggregationBusiness, AggregationBusiness>();
            services.AddTransient<IFrameworkBusiness, FrameworkBusiness>();
            services.AddTransient<IModuleBuilderBusiness, ModuleBuilderBusiness>();
            services.AddTransient<IFlowDocManager, FlowDocManager>();
            services.AddTransient<IFileRepository, FileRepository>();
            services.AddTransient<IDataHandling, DataHandling>();
            services.AddTransient<IGalleryState, GalleryState>();
            services.AddTransient<IGalleryEditor, GalleryEditor>();
            services.AddTransient<IMalcolmBusiness, MalcolmBusiness>();
            services.AddScoped<IVersionBusiness, VersionBusiness>();

            // Collaboration Services
            services.AddTransient<CollaborationManager>();

            // SignalR Configuration
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = env.IsDevelopment();
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
                options.HandshakeTimeout = TimeSpan.FromSeconds(15);
                options.KeepAliveInterval = TimeSpan.FromSeconds(10);
                options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
            });

            // Telemetry Services
            services.AddScoped<ITelemetryService, TelemetryService>();
            services.AddScoped<DatabasePerformanceInterceptor>();

            // Error Handling Services
            services.AddScoped<Services.IErrorAnalyticsService, Services.ErrorAnalyticsService>();
            services.AddScoped<Services.IErrorRecoveryService, Services.ErrorRecoveryService>();

            // Caching Configuration
            services.Configure<CachingConfiguration>(Configuration.GetSection("Caching"));

            // Memory Cache Configuration
            services.AddMemoryCache(options =>
            {
                var memoryConfig = Configuration.GetSection("Caching:Memory").Get<MemoryCacheConfiguration>();
                if (memoryConfig != null && memoryConfig.Enabled)
                {
                    options.SizeLimit = memoryConfig.SizeLimit;
                }
            });

            // Redis Cache Configuration
            var redisConnectionString = Configuration.GetConnectionString("Redis");
            var redisConfig = Configuration.GetSection("Caching:Redis").Get<RedisCacheConfiguration>();
            
            if (redisConfig != null && redisConfig.Enabled && !string.IsNullOrEmpty(redisConnectionString))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = redisConfig.InstanceName;
                });
            }
            else
            {
                // Fallback to in-memory distributed cache if Redis is not available
                services.AddDistributedMemoryCache();
            }

            // Caching Services
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<StandardsCacheService>();
            services.AddScoped<AssessmentCacheService>();
            services.AddScoped<CacheMonitoringService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo 
                { 
                    Title = "CSET API", 
                    Version = "v1",
                    Description = "Cyber Security Evaluation Tool (CSET) API for cybersecurity assessments and compliance evaluation.",
                    Contact = new OpenApiContact
                    {
                        Name = "CISA CSET Team",
                        Email = "cset_PMO@cisa.dhs.gov",
                        Url = new Uri("https://www.cisa.gov/resources-tools/services/cset")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = new Uri("https://github.com/cisagov/cset/blob/main/License.txt")
                    }
                });
                
                // Include XML comments for documentation
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
                
                c.ResolveConflictingActions(apiDescription => apiDescription.First());

                // Include 'SecurityScheme' to use JWT Authentication
                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    BearerFormat = "JWT",
                    Name = "JWT Authentication",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtSecurityScheme, Array.Empty<string>() }
                });
                
                // Add operation filters for better documentation
                c.OperationFilter<SwaggerDefaultValues>();
                
                // Group operations by controller
                c.TagActionsBy(api =>
                {
                    if (api.GroupName != null)
                    {
                        return new[] { api.GroupName };
                    }

                    var controllerActionDescriptor = api.ActionDescriptor as ControllerActionDescriptor;
                    if (controllerActionDescriptor != null)
                    {
                        return new[] { controllerActionDescriptor.ControllerName };
                    }

                    throw new InvalidOperationException("Unable to determine tag for endpoint.");
                });
                
                c.DocInclusionPredicate((name, api) => true);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Application Insights Request Tracking
            app.UseApplicationInsightsRequestTelemetry();

            // Performance Monitoring Middleware
            app.UsePerformanceMonitoring();

            // Enable Swagger in all environments for API documentation
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CSET API v1");
                c.RoutePrefix = "api-docs";
                c.DocumentTitle = "CSET API Documentation";
                c.DefaultModelsExpandDepth(2);
                c.DefaultModelExpandDepth(2);
                c.DisplayRequestDuration();
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            });

            System.AppDomain.CurrentDomain.SetData("ContentRootPath", env.ContentRootPath);
            System.AppDomain.CurrentDomain.SetData("WebRootPath", env.WebRootPath);

            using (StreamReader iisUrlRewriteStreamReader =
            File.OpenText("IISUrlRewrite.xml"))
            {
                var options = new RewriteOptions()
                    .AddIISUrlRewrite(iisUrlRewriteStreamReader);
                app.UseRewriter(options);
            }

            // Serve up index.html from webapp when root url is hit
            app.UseRewriter(new RewriteOptions().AddRewrite("^$", "index.html", true));

            //app.UseHttpsRedirection();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(env.ContentRootPath, "Diagram")),
                RequestPath = "/Diagram"
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(env.ContentRootPath, "Documents")),
                RequestPath = "/Documents"
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(env.ContentRootPath, "WebApp")),
                RequestPath = ""
            });
            app.ConfigureExceptionHandler();
            app.UseRouting();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                
                // SignalR Hub Mapping
                endpoints.MapHub<Hubs.CollaborationHub>("/collaborationHub");
            });
        }
    }
}
