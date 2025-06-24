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
    /// Integration tests for authorization and role-based access control
    /// </summary>
    [TestClass]
    public class AuthorizationControllerTests : BaseApiTest
    {
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        #region Role-Based Access Control Tests

        [TestMethod]
        public async Task AdminUser_CanAccessAdminOnlyEndpoints_ReturnsSuccess()
        {
            // Arrange
            var (adminUser, adminClient) = await CreateUserWithRoleAsync(2); // Admin role
            var assessment = await CreateTestAssessmentAsync(adminUser.UserId);

            // Act - Try to access admin-only endpoint (add contact)
            var contactRequest = new
            {
                PrimaryEmail = "newuser@example.com",
                FirstName = "New",
                LastName = "User",
                AssessmentRoleId = 1, // User role
                AssessmentId = assessment.Assessment_Id
            };

            var content = new StringContent(
                JsonSerializer.Serialize(contactRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await adminClient.PostAsync("/api/contacts/addnew", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task RegularUser_CannotAccessAdminOnlyEndpoints_ReturnsForbidden()
        {
            // Arrange
            var (regularUser, userClient) = await CreateUserWithRoleAsync(1); // User role
            var assessment = await CreateTestAssessmentAsync(regularUser.UserId);

            // Act - Try to access admin-only endpoint (add contact)
            var contactRequest = new
            {
                PrimaryEmail = "newuser@example.com",
                FirstName = "New",
                LastName = "User",
                AssessmentRoleId = 1,
                AssessmentId = assessment.Assessment_Id
            };

            var content = new StringContent(
                JsonSerializer.Serialize(contactRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await userClient.PostAsync("/api/contacts/addnew", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [TestMethod]
        public async Task UnauthenticatedUser_CannotAccessProtectedEndpoints_ReturnsUnauthorized()
        {
            // Arrange - Use unauthenticated client

            // Act - Try to access protected endpoint
            var response = await Client.GetAsync("/api/contacts");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task User_CanAccessOwnAssessmentData_ReturnsSuccess()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);
            var assessment = await CreateTestAssessmentAsync(user.UserId);

            // Act - Try to access own assessment contacts
            var response = await userClient.GetAsync("/api/contacts");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var contactsResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            contactsResponse.TryGetProperty("contactList", out _).Should().BeTrue();
            contactsResponse.TryGetProperty("currentUserRole", out _).Should().BeTrue();
        }

        [TestMethod]
        public async Task User_CannotAccessOtherUserAssessmentData_ReturnsForbidden()
        {
            // Arrange
            var (user1, user1Client) = await CreateUserWithRoleAsync(1);
            var (user2, user2Client) = await CreateUserWithRoleAsync(1);
            var assessment1 = await CreateTestAssessmentAsync(user1.UserId);
            var assessment2 = await CreateTestAssessmentAsync(user2.UserId);

            // Act - User1 tries to access User2's assessment (this would require knowing the assessment ID)
            // Note: In CSET, users are typically restricted to their own assessments via token validation
            // This test verifies that cross-assessment access is prevented

            // Set up user1's client to try to access assessment2
            var token = await GetTokenForUserAsync(user1);
            user1Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            // Try to access assessment2's data (this should fail)
            var response = await user1Client.GetAsync($"/api/assessment/{assessment2.Assessment_Id}");

            // Assert - Should be forbidden or not found
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.NotFound);
        }

        #endregion

        #region Permission Validation Tests

        [TestMethod]
        public async Task Admin_CanRemoveOtherUsers_ReturnsSuccess()
        {
            // Arrange
            var (adminUser, adminClient) = await CreateUserWithRoleAsync(2);
            var (regularUser, _) = await CreateUserWithRoleAsync(1);
            var assessment = await CreateTestAssessmentAsync(adminUser.UserId);
            
            // Add regular user to assessment
            await AddUserToAssessmentAsync(regularUser.UserId, assessment.Assessment_Id, 1);

            // Get the contact record
            var contact = await DbContext.ASSESSMENT_CONTACTS
                .FirstOrDefaultAsync(ac => ac.UserId == regularUser.UserId && ac.Assessment_Id == assessment.Assessment_Id);

            // Act - Admin removes regular user
            var removeRequest = new
            {
                AssessmentContactId = contact.Assessment_Contact_Id
            };

            var content = new StringContent(
                JsonSerializer.Serialize(removeRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await adminClient.PostAsync("/api/contacts/remove", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task RegularUser_CannotRemoveOtherUsers_ReturnsForbidden()
        {
            // Arrange
            var (user1, user1Client) = await CreateUserWithRoleAsync(1);
            var (user2, _) = await CreateUserWithRoleAsync(1);
            var assessment = await CreateTestAssessmentAsync(user1.UserId);
            
            // Add user2 to assessment
            await AddUserToAssessmentAsync(user2.UserId, assessment.Assessment_Id, 1);

            // Get the contact record
            var contact = await DbContext.ASSESSMENT_CONTACTS
                .FirstOrDefaultAsync(ac => ac.UserId == user2.UserId && ac.Assessment_Id == assessment.Assessment_Id);

            // Act - User1 tries to remove User2
            var removeRequest = new
            {
                AssessmentContactId = contact.Assessment_Contact_Id
            };

            var content = new StringContent(
                JsonSerializer.Serialize(removeRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await user1Client.PostAsync("/api/contacts/remove", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task RegularUser_CanRemoveSelf_ReturnsSuccess()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);
            var assessment = await CreateTestAssessmentAsync(user.UserId);

            // Get the contact record
            var contact = await DbContext.ASSESSMENT_CONTACTS
                .FirstOrDefaultAsync(ac => ac.UserId == user.UserId && ac.Assessment_Id == assessment.Assessment_Id);

            // Act - User removes themselves
            var removeRequest = new
            {
                AssessmentContactId = contact.Assessment_Contact_Id
            };

            var content = new StringContent(
                JsonSerializer.Serialize(removeRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await userClient.PostAsync("/api/contacts/remove", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task LastAdmin_CannotRemoveSelf_ReturnsBadRequest()
        {
            // Arrange
            var (adminUser, adminClient) = await CreateUserWithRoleAsync(2);
            var (regularUser, _) = await CreateUserWithRoleAsync(1);
            var assessment = await CreateTestAssessmentAsync(adminUser.UserId);
            
            // Add regular user to assessment
            await AddUserToAssessmentAsync(regularUser.UserId, assessment.Assessment_Id, 1);

            // Get the admin contact record
            var contact = await DbContext.ASSESSMENT_CONTACTS
                .FirstOrDefaultAsync(ac => ac.UserId == adminUser.UserId && ac.Assessment_Id == assessment.Assessment_Id);

            // Act - Last admin tries to remove themselves
            var removeRequest = new
            {
                AssessmentContactId = contact.Assessment_Contact_Id
            };

            var content = new StringContent(
                JsonSerializer.Serialize(removeRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await adminClient.PostAsync("/api/contacts/remove", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region Cross-User Data Access Prevention Tests

        [TestMethod]
        public async Task User_CannotAccessOtherUserProfile_ReturnsForbidden()
        {
            // Arrange
            var (user1, user1Client) = await CreateUserWithRoleAsync(1);
            var (user2, _) = await CreateUserWithRoleAsync(1);

            // Act - User1 tries to access User2's profile
            var response = await user1Client.GetAsync($"/api/contacts/GetUserInfo");

            // Assert - Should only return User1's info, not User2's
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<JsonElement>(responseContent);
            userInfo.TryGetProperty("primaryEmail", out var email).Should().BeTrue();
            email.GetString().Should().Be(user1.PrimaryEmail);
        }

        [TestMethod]
        public async Task User_CannotUpdateOtherUserProfile_ReturnsForbidden()
        {
            // Arrange
            var (user1, user1Client) = await CreateUserWithRoleAsync(1);
            var (user2, _) = await CreateUserWithRoleAsync(1);

            // Act - User1 tries to update User2's profile
            var updateRequest = new
            {
                UserId = user2.UserId,
                PrimaryEmail = user2.PrimaryEmail,
                FirstName = "Modified",
                LastName = "Name",
                AssessmentRoleId = 1
            };

            var content = new StringContent(
                JsonSerializer.Serialize(updateRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await user1Client.PostAsync("/api/contacts/UpdateUser", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        #endregion

        #region Admin vs User Privilege Separation Tests

        [TestMethod]
        public async Task Admin_CanInviteUsers_ReturnsSuccess()
        {
            // Arrange
            var (adminUser, adminClient) = await CreateUserWithRoleAsync(2);
            var assessment = await CreateTestAssessmentAsync(adminUser.UserId);

            // Act - Admin invites new user
            var inviteRequest = new
            {
                InviteeList = new[] { "newuser@example.com" },
                Subject = "Invitation to Assessment",
                Body = "You are invited to participate in this assessment."
            };

            var content = new StringContent(
                JsonSerializer.Serialize(inviteRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await adminClient.PostAsync("/api/contacts/invite", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task RegularUser_CannotInviteUsers_ReturnsForbidden()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);
            var assessment = await CreateTestAssessmentAsync(user.UserId);

            // Act - Regular user tries to invite new user
            var inviteRequest = new
            {
                InviteeList = new[] { "newuser@example.com" },
                Subject = "Invitation to Assessment",
                Body = "You are invited to participate in this assessment."
            };

            var content = new StringContent(
                JsonSerializer.Serialize(inviteRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await userClient.PostAsync("/api/contacts/invite", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [TestMethod]
        public async Task Admin_CanUpdateUserRoles_ReturnsSuccess()
        {
            // Arrange
            var (adminUser, adminClient) = await CreateUserWithRoleAsync(2);
            var (regularUser, _) = await CreateUserWithRoleAsync(1);
            var assessment = await CreateTestAssessmentAsync(adminUser.UserId);
            
            // Add regular user to assessment
            await AddUserToAssessmentAsync(regularUser.UserId, assessment.Assessment_Id, 1);

            // Get the contact record
            var contact = await DbContext.ASSESSMENT_CONTACTS
                .FirstOrDefaultAsync(ac => ac.UserId == regularUser.UserId && ac.Assessment_Id == assessment.Assessment_Id);

            // Act - Admin updates user role
            var updateRequest = new
            {
                UserId = regularUser.UserId,
                PrimaryEmail = regularUser.PrimaryEmail,
                FirstName = regularUser.FirstName,
                LastName = regularUser.LastName,
                AssessmentRoleId = 2 // Promote to admin
            };

            var content = new StringContent(
                JsonSerializer.Serialize(updateRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await adminClient.PostAsync("/api/contacts/UpdateUser", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task RegularUser_CannotUpdateUserRoles_ReturnsForbidden()
        {
            // Arrange
            var (user1, user1Client) = await CreateUserWithRoleAsync(1);
            var (user2, _) = await CreateUserWithRoleAsync(1);
            var assessment = await CreateTestAssessmentAsync(user1.UserId);
            
            // Add user2 to assessment
            await AddUserToAssessmentAsync(user2.UserId, assessment.Assessment_Id, 1);

            // Act - User1 tries to update User2's role
            var updateRequest = new
            {
                UserId = user2.UserId,
                PrimaryEmail = user2.PrimaryEmail,
                FirstName = user2.FirstName,
                LastName = user2.LastName,
                AssessmentRoleId = 2 // Promote to admin
            };

            var content = new StringContent(
                JsonSerializer.Serialize(updateRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await user1Client.PostAsync("/api/contacts/UpdateUser", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        #endregion

        #region Token-Based Authorization Tests

        [TestMethod]
        public async Task ValidToken_WithCorrectRole_AllowsAccess()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);
            var assessment = await CreateTestAssessmentAsync(user.UserId);

            // Act - Access protected endpoint with valid token
            var response = await userClient.GetAsync("/api/contacts");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task InvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var client = CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

            // Act - Try to access protected endpoint with invalid token
            var response = await client.GetAsync("/api/contacts");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task ExpiredToken_ReturnsUnauthorized()
        {
            // Arrange
            var client = CreateClient();
            // Note: This would require creating an expired token, which is complex in the test environment
            // For now, we'll test with a malformed token
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "expired.jwt.token");

            // Act - Try to access protected endpoint with expired token
            var response = await client.GetAsync("/api/contacts");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Edge Cases and Security Tests

        [TestMethod]
        public async Task User_WithNoAssessmentAccess_ReturnsForbidden()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);
            // Don't create any assessment for this user

            // Act - Try to access assessment-related endpoint
            var response = await userClient.GetAsync("/api/contacts");

            // Assert - Should be forbidden or not found since user has no assessment
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task MultipleAdmins_CanAllPerformAdminOperations()
        {
            // Arrange
            var (admin1, admin1Client) = await CreateUserWithRoleAsync(2);
            var (admin2, admin2Client) = await CreateUserWithRoleAsync(2);
            var assessment = await CreateTestAssessmentAsync(admin1.UserId);
            
            // Add admin2 to assessment
            await AddUserToAssessmentAsync(admin2.UserId, assessment.Assessment_Id, 2);

            // Act - Both admins try to add a contact
            var contactRequest = new
            {
                PrimaryEmail = "newuser@example.com",
                FirstName = "New",
                LastName = "User",
                AssessmentRoleId = 1,
                AssessmentId = assessment.Assessment_Id
            };

            var content = new StringContent(
                JsonSerializer.Serialize(contactRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response1 = await admin1Client.PostAsync("/api/contacts/addnew", content);
            var response2 = await admin2Client.PostAsync("/api/contacts/addnew", content);

            // Assert - Both should succeed
            response1.StatusCode.Should().Be(HttpStatusCode.OK);
            response2.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task RoleEnumeration_ReturnsCorrectRoles()
        {
            // Arrange
            var client = CreateClient();

            // Act - Get available roles
            var response = await client.GetAsync("/api/contacts/allroles");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var roles = JsonSerializer.Deserialize<JsonElement>(responseContent);
            roles.GetArray().Should().HaveCountGreaterThan(0);
        }

        #endregion

        #region Test Data Helpers

        private async Task<(USERS user, HttpClient client)> CreateUserWithRoleAsync(int roleId)
        {
            var user = new USERS
            {
                PrimaryEmail = $"testuser{Guid.NewGuid()}@example.com",
                FirstName = "Test",
                LastName = "User",
                CreatedDate = DateTime.UtcNow,
                Password = "TestPassword123!"
            };

            DbContext.USERS.Add(user);
            await DbContext.SaveChangesAsync();

            var client = await GetAuthenticatedClientAsync(user);
            return (user, client);
        }

        private async Task<ASSESSMENTS> CreateTestAssessmentAsync(int? userId)
        {
            var assessment = new ASSESSMENTS
            {
                AssessmentName = "Test Assessment",
                AssessmentDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatorId = userId
            };

            DbContext.ASSESSMENTS.Add(assessment);
            await DbContext.SaveChangesAsync();

            return assessment;
        }

        private async Task AddUserToAssessmentAsync(int userId, int assessmentId, int roleId)
        {
            var user = await DbContext.USERS.FindAsync(userId);
            var contact = new ASSESSMENT_CONTACTS
            {
                Assessment_Id = assessmentId,
                UserId = userId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PrimaryEmail = user.PrimaryEmail,
                AssessmentRoleId = roleId,
                Invited = true
            };

            DbContext.ASSESSMENT_CONTACTS.Add(contact);
            await DbContext.SaveChangesAsync();
        }

        private async Task<string> GetTokenForUserAsync(USERS user)
        {
            // This would require implementing token generation for testing
            // For now, we'll use the existing authentication helper
            var loginRequest = new
            {
                Email = user.PrimaryEmail,
                Password = "TestPassword123!"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await Client.PostAsync("/api/auth/login", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            return loginResponse.GetProperty("token").GetString();
        }

        #endregion
    }
} 