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

namespace CSETWebCore.ApiTests.Controllers
{
    /// <summary>
    /// Integration tests for authentication-related API endpoints
    /// </summary>
    [TestClass]
    public class AuthenticationControllerTests : BaseApiTest
    {
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        [TestMethod]
        public async Task Login_ValidCredentials_ReturnsSuccess()
        {
            // Arrange
            var testUser = await CreateTestUserAsync("testuser@example.com", "TestPassword123!");
            var loginRequest = new
            {
                Email = "testuser@example.com",
                Password = "TestPassword123!"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await Client.PostAsync("/api/auth/login", content);

            // Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().NotBeEmpty();
            
            // Verify response contains expected fields (adjust based on actual API response)
            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);
            responseObject.TryGetProperty("token", out _).Should().BeTrue();
        }

        [TestMethod]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new
            {
                Email = "invalid@example.com",
                Password = "WrongPassword"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await Client.PostAsync("/api/auth/login", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task Login_EmptyCredentials_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new
            {
                Email = "",
                Password = ""
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await Client.PostAsync("/api/auth/login", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Login_MalformedJson_ReturnsBadRequest()
        {
            // Arrange
            var content = new StringContent(
                "{ invalid json }",
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await Client.PostAsync("/api/auth/login", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task AccessKeyLogin_ValidKey_ReturnsSuccess()
        {
            // Arrange
            var accessKey = await CreateTestAccessKeyAsync();
            var loginRequest = new
            {
                AccessKey = accessKey.AccessKey
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await Client.PostAsync("/api/auth/accesskey", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task AccessKeyLogin_InvalidKey_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new
            {
                AccessKey = "INVALID-ACCESS-KEY"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await Client.PostAsync("/api/auth/accesskey", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task AccessKeyLogin_ExpiredKey_ReturnsUnauthorized()
        {
            // Arrange
            var expiredKey = await CreateTestAccessKeyAsync(isExpired: true);
            var loginRequest = new
            {
                AccessKey = expiredKey.AccessKey
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await Client.PostAsync("/api/auth/accesskey", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task Logout_AuthenticatedUser_ReturnsSuccess()
        {
            // Arrange
            var authenticatedClient = await GetAuthenticatedClientAsync();

            // Act
            var response = await authenticatedClient.PostAsync("/api/auth/logout", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task Logout_UnauthenticatedUser_ReturnsUnauthorized()
        {
            // Act
            var response = await Client.PostAsync("/api/auth/logout", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task PasswordReset_ValidEmail_ReturnsSuccess()
        {
            // Arrange
            await CreateTestUserAsync("testuser@example.com", "TestPassword123!");
            var resetRequest = new
            {
                Email = "testuser@example.com"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(resetRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await Client.PostAsync("/api/auth/password-reset", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task PasswordReset_InvalidEmail_ReturnsNotFound()
        {
            // Arrange
            var resetRequest = new
            {
                Email = "nonexistent@example.com"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(resetRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await Client.PostAsync("/api/auth/password-reset", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #region Test Data Helpers

        private async Task<USERS> CreateTestUserAsync(string email, string password)
        {
            var user = new USERS
            {
                PrimaryEmail = email,
                FirstName = "Test",
                LastName = "User",
                CreatedDate = DateTime.UtcNow,
                // Note: In a real implementation, you'd hash the password
                Password = password // This should be hashed in real implementation
            };

            DbContext.USERS.Add(user);
            await DbContext.SaveChangesAsync();
            return user;
        }

        private async Task<ACCESS_KEY> CreateTestAccessKeyAsync(bool isExpired = false)
        {
            var assessmentId = await CreateTestAssessmentAsync();
            var accessKey = new ACCESS_KEY
            {
                AccessKey = $"TEST-KEY-{Guid.NewGuid()}",
                Assessment_Id = assessmentId,
                GeneratedDate = DateTime.UtcNow,
                ExpirationDate = isExpired ? DateTime.UtcNow.AddDays(-1) : DateTime.UtcNow.AddDays(30)
            };

            DbContext.ACCESS_KEY.Add(accessKey);
            await DbContext.SaveChangesAsync();
            return accessKey;
        }

        #endregion
    }
} 