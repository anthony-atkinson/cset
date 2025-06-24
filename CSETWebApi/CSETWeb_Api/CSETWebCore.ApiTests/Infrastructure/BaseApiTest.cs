//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSETWebCore.DataLayer.Model;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace CSETWebCore.ApiTests.Infrastructure
{
    /// <summary>
    /// Base class for API integration tests providing common setup and utilities
    /// </summary>
    [TestClass]
    public abstract class BaseApiTest : IDisposable
    {
        protected WebApplicationFactory<Program> Factory { get; private set; } = null!;
        protected HttpClient Client { get; private set; } = null!;
        protected CSETContext DbContext { get; private set; } = null!;
        protected IServiceScope ServiceScope { get; private set; } = null!;

        [TestInitialize]
        public virtual async Task TestInitializeAsync()
        {
            Factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Remove the existing DbContext registration
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<CSETContext>));
                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        // Add in-memory database for testing
                        services.AddDbContext<CSETContext>(options =>
                        {
                            options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                        });

                        // Override logging to reduce noise in tests
                        services.AddLogging(logging =>
                        {
                            logging.ClearProviders();
                            logging.AddConsole();
                            logging.SetMinimumLevel(LogLevel.Warning);
                        });
                    });

                    builder.UseEnvironment("Testing");
                });

            Client = Factory.CreateClient();
            ServiceScope = Factory.Services.CreateScope();
            DbContext = ServiceScope.ServiceProvider.GetRequiredService<CSETContext>();

            // Ensure the database is created
            await DbContext.Database.EnsureCreatedAsync();

            // Seed test data if needed
            await SeedTestDataAsync();
        }

        [TestCleanup]
        public virtual async Task TestCleanupAsync()
        {
            await CleanupTestDataAsync();
            Dispose();
        }

        /// <summary>
        /// Override this method to seed test data specific to your test class
        /// </summary>
        protected virtual async Task SeedTestDataAsync()
        {
            // Default implementation - override in derived classes
            await Task.CompletedTask;
        }

        /// <summary>
        /// Override this method to cleanup test data specific to your test class
        /// </summary>
        protected virtual async Task CleanupTestDataAsync()
        {
            // Default implementation - override in derived classes
            await Task.CompletedTask;
        }

        /// <summary>
        /// Helper method to authenticate requests (implement based on your auth system)
        /// </summary>
        protected virtual async Task<HttpClient> GetAuthenticatedClientAsync(string? accessKey = null)
        {
            // TODO: Implement authentication logic based on CSET's auth system
            // This might involve setting headers, tokens, or cookies
            
            if (!string.IsNullOrEmpty(accessKey))
            {
                Client.DefaultRequestHeaders.Add("Authorization", $"AccessKey {accessKey}");
            }

            return Client;
        }

        /// <summary>
        /// Helper method to create a test assessment
        /// </summary>
        protected async Task<int> CreateTestAssessmentAsync(string assessmentName = "Test Assessment")
        {
            var assessment = new ASSESSMENTS
            {
                Assessment_Name = assessmentName,
                Assessment_Date = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };

            DbContext.ASSESSMENTS.Add(assessment);
            await DbContext.SaveChangesAsync();
            return assessment.Assessment_Id;
        }

        public virtual void Dispose()
        {
            ServiceScope?.Dispose();
            Client?.Dispose();
            Factory?.Dispose();
        }
    }
} 