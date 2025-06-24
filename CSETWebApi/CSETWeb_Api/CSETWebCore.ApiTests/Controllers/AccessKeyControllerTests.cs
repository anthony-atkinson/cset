//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Text.Json;
using CSETWebCore.ApiTests.Infrastructure;
using CSETWebCore.DataLayer.Model;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace CSETWebCore.ApiTests.Controllers
{
    /// <summary>
    /// Integration tests for access key authentication and management
    /// </summary>
    [TestClass]
    public class AccessKeyControllerTests : BaseApiTest
    {
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        #region Access Key Generation Tests

        [TestMethod]
        public async Task GenerateAccessKey_AuthenticatedUser_ReturnsValidKey()
        {
            // Arrange
            var authenticatedClient = await GetAuthenticatedClientAsync();

            // Act
            var response = await authenticatedClient.GetAsync("/api/auth/accesskey");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var accessKey = await response.Content.ReadAsStringAsync();
            accessKey.Should().NotBeNullOrEmpty();
            accessKey.Should().HaveLength(10); // Based on the generation logic
            accessKey.Should().MatchRegex("^[A-Z0-9]{10}$"); // Base32 format, uppercase
            
            // Verify the key was saved to database
            var savedKey = await DbContext.ACCESS_KEY.FirstOrDefaultAsync(ak => ak.AccessKey == accessKey);
            savedKey.Should().NotBeNull();
            savedKey.GeneratedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }

        [TestMethod]
        public async Task GenerateAccessKey_UnauthenticatedUser_ReturnsUnauthorized()
        {
            // Arrange - Use unauthenticated client

            // Act
            var response = await Client.GetAsync("/api/auth/accesskey");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task GenerateAccessKey_MultipleKeys_GeneratesUniqueKeys()
        {
            // Arrange
            var authenticatedClient = await GetAuthenticatedClientAsync();

            // Act
            var response1 = await authenticatedClient.GetAsync("/api/auth/accesskey");
            var response2 = await authenticatedClient.GetAsync("/api/auth/accesskey");

            // Assert
            response1.StatusCode.Should().Be(HttpStatusCode.OK);
            response2.StatusCode.Should().Be(HttpStatusCode.OK);

            var key1 = await response1.Content.ReadAsStringAsync();
            var key2 = await response2.Content.ReadAsStringAsync();

            key1.Should().NotBe(key2);
            key1.Should().HaveLength(10);
            key2.Should().HaveLength(10);
        }

        #endregion

        #region Access Key Authentication Tests

        [TestMethod]
        public async Task LoginWithAccessKey_ValidKey_ReturnsSuccess()
        {
            // Arrange
            var accessKey = await CreateTestAccessKeyAsync();
            var loginRequest = new
            {
                AccessKey = accessKey.AccessKey,
                TzOffset = 0,
                Scope = "CSET"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await Client.PostAsync("/api/auth/login/accesskey", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            // Verify response contains expected fields
            loginResponse.TryGetProperty("token", out _).Should().BeTrue();
            loginResponse.TryGetProperty("exportExtension", out _).Should().BeTrue();
            loginResponse.TryGetProperty("importExtensions", out _).Should().BeTrue();
            loginResponse.TryGetProperty("linkerTime", out _).Should().BeTrue();
        }

        [TestMethod]
        public async Task LoginWithAccessKey_InvalidKey_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new
            {
                AccessKey = "INVALID-KEY-123",
                TzOffset = 0,
                Scope = "CSET"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await Client.PostAsync("/api/auth/login/accesskey", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task LoginWithAccessKey_EmptyKey_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new
            {
                AccessKey = "",
                TzOffset = 0,
                Scope = "CSET"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await Client.PostAsync("/api/auth/login/accesskey", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task LoginWithAccessKey_NullKey_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new
            {
                AccessKey = (string)null,
                TzOffset = 0,
                Scope = "CSET"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await Client.PostAsync("/api/auth/login/accesskey", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task LoginWithAccessKey_MalformedJson_ReturnsBadRequest()
        {
            // Arrange
            var content = new StringContent(
                "{ invalid json }",
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await Client.PostAsync("/api/auth/login/accesskey", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task LoginWithAccessKey_WithValidScope_ReturnsCorrectExtensions()
        {
            // Arrange
            var accessKey = await CreateTestAccessKeyAsync();
            var loginRequest = new
            {
                AccessKey = accessKey.AccessKey,
                TzOffset = 0,
                Scope = "CSET"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await Client.PostAsync("/api/auth/login/accesskey", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            loginResponse.TryGetProperty("exportExtension", out var exportExt).Should().BeTrue();
            loginResponse.TryGetProperty("importExtensions", out var importExts).Should().BeTrue();
            
            // Verify extensions are appropriate for CSET scope
            exportExt.GetString().Should().Be(".cset");
            importExts.GetArray().Should().NotBeEmpty();
        }

        [TestMethod]
        public async Task LoginWithAccessKey_WithDifferentTimezone_ReturnsSuccess()
        {
            // Arrange
            var accessKey = await CreateTestAccessKeyAsync();
            var loginRequest = new
            {
                AccessKey = accessKey.AccessKey,
                TzOffset = -300, // EST timezone
                Scope = "CSET"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await Client.PostAsync("/api/auth/login/accesskey", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            loginResponse.TryGetProperty("token", out _).Should().BeTrue();
        }

        #endregion

        #region Security Tests

        [TestMethod]
        public async Task LoginWithAccessKey_SqlInjectionAttempt_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new
            {
                AccessKey = "'; DROP TABLE ACCESS_KEY; --",
                TzOffset = 0,
                Scope = "CSET"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await Client.PostAsync("/api/auth/login/accesskey", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            
            // Verify no tables were dropped
            var accessKeyCount = await DbContext.ACCESS_KEY.CountAsync();
            accessKeyCount.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public async Task LoginWithAccessKey_XssAttempt_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new
            {
                AccessKey = "<script>alert('xss')</script>",
                TzOffset = 0,
                Scope = "CSET"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await Client.PostAsync("/api/auth/login/accesskey", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task LoginWithAccessKey_ExcessiveLength_ReturnsBadRequest()
        {
            // Arrange
            var longKey = new string('A', 1000); // Very long key
            var loginRequest = new
            {
                AccessKey = longKey,
                TzOffset = 0,
                Scope = "CSET"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await Client.PostAsync("/api/auth/login/accesskey", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public async Task LoginWithAccessKey_ConcurrentRequests_SameKey_HandlesCorrectly()
        {
            // Arrange
            var accessKey = await CreateTestAccessKeyAsync();
            var loginRequest = new
            {
                AccessKey = accessKey.AccessKey,
                TzOffset = 0,
                Scope = "CSET"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act - Make concurrent requests
            var tasks = new List<Task<HttpResponseMessage>>();
            for (int i = 0; i < 5; i++)
            {
                tasks.Add(Client.PostAsync("/api/auth/login/accesskey", content));
            }

            var responses = await Task.WhenAll(tasks);

            // Assert - All should succeed
            responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
        }

        [TestMethod]
        public async Task GenerateAccessKey_ConcurrentRequests_GeneratesUniqueKeys()
        {
            // Arrange
            var authenticatedClient = await GetAuthenticatedClientAsync();

            // Act - Generate multiple keys concurrently
            var tasks = new List<Task<HttpResponseMessage>>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(authenticatedClient.GetAsync("/api/auth/accesskey"));
            }

            var responses = await Task.WhenAll(tasks);

            // Assert
            responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));

            var keys = new List<string>();
            foreach (var response in responses)
            {
                var key = await response.Content.ReadAsStringAsync();
                keys.Add(key);
            }

            // All keys should be unique
            keys.Distinct().Count().Should().Be(keys.Count);
        }

        [TestMethod]
        public async Task LoginWithAccessKey_DeletedKey_ReturnsBadRequest()
        {
            // Arrange
            var accessKey = await CreateTestAccessKeyAsync();
            var loginRequest = new
            {
                AccessKey = accessKey.AccessKey,
                TzOffset = 0,
                Scope = "CSET"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Delete the key from database
            DbContext.ACCESS_KEY.Remove(accessKey);
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.PostAsync("/api/auth/login/accesskey", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region Test Data Helpers

        private async Task<ACCESS_KEY> CreateTestAccessKeyAsync()
        {
            var accessKey = new ACCESS_KEY
            {
                AccessKey = GenerateUniqueAccessKey(),
                GeneratedDate = DateTime.UtcNow,
                Encryption = false,
                CisaAssessorWorkflow = false,
                Lang = "en"
            };

            DbContext.ACCESS_KEY.Add(accessKey);
            await DbContext.SaveChangesAsync();
            return accessKey;
        }

        private string GenerateUniqueAccessKey()
        {
            // Generate a unique 10-character base32 key
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var key = new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            
            return $"TEST-{key}";
        }

        #endregion
    }
} 