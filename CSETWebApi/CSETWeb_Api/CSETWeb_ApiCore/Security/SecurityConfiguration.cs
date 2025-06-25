using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;

namespace CSETWebCore.ApiCore.Security
{
    /// <summary>
    /// Centralized security configuration for the CSET application
    /// Provides security headers, authentication, authorization, and security policies
    /// </summary>
    public static class SecurityConfiguration
    {
        /// <summary>
        /// Configure security services for the application
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        public static void ConfigureSecurityServices(IServiceCollection services, IConfiguration configuration)
        {
            // Configure CORS
            ConfigureCors(services, configuration);

            // Configure Authentication
            ConfigureAuthentication(services, configuration);

            // Configure Authorization
            ConfigureAuthorization(services);

            // Configure Security Headers
            ConfigureSecurityHeaders(services);

            // Configure HTTPS Redirection
            ConfigureHttpsRedirection(services);

            // Configure Health Checks
            ConfigureHealthChecks(services, configuration);
        }

        /// <summary>
        /// Configure security middleware for the application
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="env">The web host environment</param>
        public static void ConfigureSecurityMiddleware(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Security headers middleware
            app.Use(async (context, next) =>
            {
                // Security Headers
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-Frame-Options", "DENY");
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
                context.Response.Headers.Add("Content-Security-Policy", 
                    "default-src 'self'; " +
                    "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                    "style-src 'self' 'unsafe-inline'; " +
                    "img-src 'self' data: https:; " +
                    "font-src 'self' data:; " +
                    "connect-src 'self'; " +
                    "frame-ancestors 'none';");

                // Remove server information
                context.Response.Headers.Remove("Server");
                context.Response.Headers.Remove("X-Powered-By");

                await next();
            });

            // CORS middleware
            app.UseCors("CSETCorsPolicy");

            // Authentication middleware
            app.UseAuthentication();

            // Authorization middleware
            app.UseAuthorization();

            // HTTPS redirection (only in production)
            if (!env.IsDevelopment())
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }
        }

        /// <summary>
        /// Configure CORS policy
        /// </summary>
        private static void ConfigureCors(IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CSETCorsPolicy", policy =>
                {
                    policy.WithOrigins(
                            "http://localhost:4200", // Angular dev server
                            "http://localhost:3000", // Alternative dev port
                            "https://localhost:4200", // HTTPS dev server
                            "https://localhost:3000"  // Alternative HTTPS dev port
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .WithExposedHeaders("Content-Disposition"); // For file downloads
                });
            });
        }

        /// <summary>
        /// Configure JWT authentication
        /// </summary>
        private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "CSET_Default_Secret_Key_For_Development_Only");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // Set to true in production
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false, // Set to true and configure issuer in production
                    ValidateAudience = false, // Set to true and configure audience in production
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                // Configure events for additional security
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        // Log authentication failures
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        // Additional token validation logic
                        return Task.CompletedTask;
                    }
                };
            });
        }

        /// <summary>
        /// Configure authorization policies
        /// </summary>
        private static void ConfigureAuthorization(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // Default policy requiring authentication
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                // Admin policy
                options.AddPolicy("AdminPolicy", policy =>
                    policy.RequireRole("Admin"));

                // User policy
                options.AddPolicy("UserPolicy", policy =>
                    policy.RequireRole("User", "Admin"));

                // Assessment policy
                options.AddPolicy("AssessmentPolicy", policy =>
                    policy.RequireClaim("Permission", "Assessment"));

                // Report policy
                options.AddPolicy("ReportPolicy", policy =>
                    policy.RequireClaim("Permission", "Report"));
            });
        }

        /// <summary>
        /// Configure security headers
        /// </summary>
        private static void ConfigureSecurityHeaders(IServiceCollection services)
        {
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
                options.Cookie.Name = "CSRF-TOKEN";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });

            services.Configure<MvcOptions>(options =>
            {
                // Require HTTPS in production
                options.Filters.Add(new RequireHttpsAttribute());
            });
        }

        /// <summary>
        /// Configure HTTPS redirection
        /// </summary>
        private static void ConfigureHttpsRedirection(IServiceCollection services)
        {
            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                options.HttpsPort = 443;
            });
        }

        /// <summary>
        /// Configure health checks
        /// </summary>
        private static void ConfigureHealthChecks(IServiceCollection services, IConfiguration configuration)
        {
            services.AddHealthChecks()
                .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
                .AddSqlServer(
                    configuration.GetConnectionString("CSET_DB"),
                    name: "database",
                    tags: new[] { "db", "sql", "sqlserver" })
                .AddUrlGroup(
                    new Uri("https://www.google.com"), // Replace with actual health check URL
                    name: "external-api",
                    tags: new[] { "external", "api" });
        }

        /// <summary>
        /// Security validation helpers
        /// </summary>
        public static class SecurityValidation
        {
            /// <summary>
            /// Validate input for SQL injection prevention
            /// </summary>
            public static bool IsValidInput(string input)
            {
                if (string.IsNullOrEmpty(input))
                    return true;

                // Basic SQL injection prevention
                var sqlKeywords = new[] { "SELECT", "INSERT", "UPDATE", "DELETE", "DROP", "CREATE", "ALTER", "EXEC", "EXECUTE" };
                var upperInput = input.ToUpper();

                return !sqlKeywords.Any(keyword => upperInput.Contains(keyword));
            }

            /// <summary>
            /// Validate file extension for security
            /// </summary>
            public static bool IsValidFileExtension(string fileName)
            {
                if (string.IsNullOrEmpty(fileName))
                    return false;

                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".csv", ".xlsx", ".xls" };
                var extension = Path.GetExtension(fileName).ToLower();

                return allowedExtensions.Contains(extension);
            }

            /// <summary>
            /// Sanitize HTML content
            /// </summary>
            public static string SanitizeHtml(string html)
            {
                if (string.IsNullOrEmpty(html))
                    return html;

                // Basic HTML sanitization - consider using a library like HtmlSanitizer
                return html.Replace("<script>", "").Replace("</script>", "")
                          .Replace("javascript:", "")
                          .Replace("onload=", "")
                          .Replace("onerror=", "");
            }
        }
    }
} 