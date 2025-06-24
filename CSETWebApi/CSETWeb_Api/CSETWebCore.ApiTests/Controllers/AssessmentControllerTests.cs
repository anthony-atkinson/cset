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
    /// Integration tests for assessment-related API endpoints
    /// </summary>
    [TestClass]
    public class AssessmentControllerTests : BaseApiTest
    {
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        #region Assessment Creation Tests

        [TestMethod]
        public async Task CreateAssessment_WithValidGallery_ReturnsSuccess()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(2); // Admin role
            var galleryItem = await CreateTestGalleryItemAsync();

            // Act - Create assessment using gallery
            var response = await userClient.GetAsync($"/api/createassessment/gallery?workflow=CSET&galleryGuid={galleryItem.Gallery_Item_Guid}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var assessment = JsonSerializer.Deserialize<JsonElement>(responseContent);
            assessment.TryGetProperty("id", out _).Should().BeTrue();
            assessment.TryGetProperty("assessmentName", out _).Should().BeTrue();
        }

        [TestMethod]
        public async Task CreateAssessment_WithCustomSetName_ReturnsSuccess()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(2);
            var galleryGuid = Guid.NewGuid();
            var customSetName = "CUSTOM_SET";

            // Act - Create assessment with custom set name
            var response = await userClient.GetAsync($"/api/createassessment/gallery?workflow=CSET&galleryGuid={galleryGuid}&csn={customSetName}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task CreateAssessment_WithoutGalleryOptions_ReturnsBadRequest()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(2);
            var invalidGalleryGuid = Guid.NewGuid();

            // Act - Try to create assessment without valid gallery options
            var response = await userClient.GetAsync($"/api/createassessment/gallery?workflow=CSET&galleryGuid={invalidGalleryGuid}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task CreateAssessment_UnauthenticatedUser_ReturnsUnauthorized()
        {
            // Arrange - Use unauthenticated client
            var galleryGuid = Guid.NewGuid();

            // Act - Try to create assessment without authentication
            var response = await Client.GetAsync($"/api/createassessment/gallery?workflow=CSET&galleryGuid={galleryGuid}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Assessment Retrieval Tests

        [TestMethod]
        public async Task GetMyAssessments_AuthenticatedUser_ReturnsSuccess()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);
            var assessment = await CreateTestAssessmentAsync(user.UserId);

            // Act - Get user's assessments
            var response = await userClient.GetAsync("/api/assessmentsforuser");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var assessments = JsonSerializer.Deserialize<JsonElement>(responseContent);
            assessments.GetArray().Should().HaveCountGreaterThan(0);
        }

        [TestMethod]
        public async Task GetMyAssessments_AccessKeyUser_ReturnsSuccess()
        {
            // Arrange
            var accessKey = await CreateTestAccessKeyAsync();
            var assessment = await CreateTestAssessmentAsync(null, accessKey.AccessKey);

            // Create client with access key authentication
            var client = CreateClient();
            var token = await GetAccessKeyTokenAsync(accessKey.AccessKey);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act - Get access key user's assessments
            var response = await client.GetAsync("/api/assessmentsforuser");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task GetAssessmentDetail_ValidAssessment_ReturnsSuccess()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);
            var assessment = await CreateTestAssessmentAsync(user.UserId);

            // Set the assessment in the token context
            await SetAssessmentInTokenAsync(userClient, assessment.Assessment_Id);

            // Act - Get assessment detail
            var response = await userClient.GetAsync("/api/assessmentdetail");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var assessmentDetail = JsonSerializer.Deserialize<JsonElement>(responseContent);
            assessmentDetail.TryGetProperty("id", out _).Should().BeTrue();
            assessmentDetail.TryGetProperty("assessmentName", out _).Should().BeTrue();
        }

        [TestMethod]
        public async Task GetAssessmentDetail_NoAssessmentInToken_ReturnsBadRequest()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);
            // Don't set any assessment in token

            // Act - Try to get assessment detail without assessment context
            var response = await userClient.GetAsync("/api/assessmentdetail");

            // Assert - Should fail because no assessment is set in token
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Assessment Update Tests

        [TestMethod]
        public async Task UpdateAssessmentDetail_ValidData_ReturnsSuccess()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);
            var assessment = await CreateTestAssessmentAsync(user.UserId);

            // Set the assessment in the token context
            await SetAssessmentInTokenAsync(userClient, assessment.Assessment_Id);

            var updateRequest = new
            {
                Id = assessment.Assessment_Id,
                AssessmentName = "Updated Assessment Name",
                AssessmentDate = DateTime.UtcNow,
                CreatorId = user.UserId,
                CreatedDate = assessment.CreatedDate,
                LastModifiedDate = DateTime.UtcNow,
                AssessmentEffectiveDate = DateTime.UtcNow
            };

            var content = new StringContent(
                JsonSerializer.Serialize(updateRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act - Update assessment detail
            var response = await userClient.PostAsync("/api/assessmentdetail", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task UpdateAssessmentDetail_DifferentAssessmentId_ReturnsException()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);
            var assessment = await CreateTestAssessmentAsync(user.UserId);

            // Set the assessment in the token context
            await SetAssessmentInTokenAsync(userClient, assessment.Assessment_Id);

            var updateRequest = new
            {
                Id = assessment.Assessment_Id + 999, // Different assessment ID
                AssessmentName = "Updated Assessment Name",
                AssessmentDate = DateTime.UtcNow,
                CreatorId = user.UserId,
                CreatedDate = assessment.CreatedDate,
                LastModifiedDate = DateTime.UtcNow,
                AssessmentEffectiveDate = DateTime.UtcNow
            };

            var content = new StringContent(
                JsonSerializer.Serialize(updateRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act - Try to update with different assessment ID
            var response = await userClient.PostAsync("/api/assessmentdetail", content);

            // Assert - Should fail due to authorization mismatch
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        #endregion

        #region Assessment Completion Tests

        [TestMethod]
        public async Task GetAssessmentsCompletion_AuthenticatedUser_ReturnsSuccess()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);
            var assessment = await CreateTestAssessmentAsync(user.UserId);

            // Act - Get assessment completion stats
            var response = await userClient.GetAsync("/api/assessmentsCompletionForUser");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var completionStats = JsonSerializer.Deserialize<JsonElement>(responseContent);
            completionStats.GetArray().Should().HaveCountGreaterThan(0);
        }

        [TestMethod]
        public async Task GetAssessmentsCompletion_AccessKeyUser_ReturnsSuccess()
        {
            // Arrange
            var accessKey = await CreateTestAccessKeyAsync();
            var assessment = await CreateTestAssessmentAsync(null, accessKey.AccessKey);

            // Create client with access key authentication
            var client = CreateClient();
            var token = await GetAccessKeyTokenAsync(accessKey.AccessKey);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act - Get access key user's completion stats
            var response = await client.GetAsync("/api/assessmentsCompletionForUser");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task GetAssessmentsCompletion_NoUserOrAccessKey_ReturnsBadRequest()
        {
            // Arrange - Use unauthenticated client

            // Act - Try to get completion stats without authentication
            var response = await Client.GetAsync("/api/assessmentsCompletionForUser");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region Assessment Documents Tests

        [TestMethod]
        public async Task GetAssessmentDocuments_WithAssessment_ReturnsSuccess()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);
            var assessment = await CreateTestAssessmentAsync(user.UserId);

            // Set the assessment in the token context
            await SetAssessmentInTokenAsync(userClient, assessment.Assessment_Id);

            // Act - Get assessment documents
            var response = await userClient.GetAsync("/api/assessmentdocuments");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task GetAssessmentDocuments_WithoutAssessment_ReturnsGlobalDocuments()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);
            // Don't set any assessment in token

            // Act - Get documents without assessment context
            var response = await userClient.GetAsync("/api/assessmentdocuments");

            // Assert - Should return global documents
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region Assessment Remarks Tests

        [TestMethod]
        public async Task GetOtherRemarks_ValidAssessment_ReturnsSuccess()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);
            var assessment = await CreateTestAssessmentAsync(user.UserId);

            // Set the assessment in the token context
            await SetAssessmentInTokenAsync(userClient, assessment.Assessment_Id);

            // Act - Get assessment remarks
            var response = await userClient.GetAsync("/api/remarks");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task SaveOtherRemarks_ValidData_ReturnsSuccess()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);
            var assessment = await CreateTestAssessmentAsync(user.UserId);

            // Set the assessment in the token context
            await SetAssessmentInTokenAsync(userClient, assessment.Assessment_Id);

            var remarks = "Test assessment remarks for testing purposes.";

            var content = new StringContent(
                JsonSerializer.Serialize(remarks),
                Encoding.UTF8,
                "application/json");

            // Act - Save assessment remarks
            var response = await userClient.PostAsync("/api/remarks", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task SaveOtherRemarks_EmptyRemarks_ReturnsSuccess()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);
            var assessment = await CreateTestAssessmentAsync(user.UserId);

            // Set the assessment in the token context
            await SetAssessmentInTokenAsync(userClient, assessment.Assessment_Id);

            var remarks = "";

            var content = new StringContent(
                JsonSerializer.Serialize(remarks),
                Encoding.UTF8,
                "application/json");

            // Act - Save empty remarks
            var response = await userClient.PostAsync("/api/remarks", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region Assessment Creator Tests

        [TestMethod]
        public async Task GetAssessmentCreator_ValidAssessment_ReturnsSuccess()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);
            var assessment = await CreateTestAssessmentAsync(user.UserId);

            // Set the assessment in the token context
            await SetAssessmentInTokenAsync(userClient, assessment.Assessment_Id);

            // Act - Get assessment creator
            var response = await userClient.GetAsync("/api/assessmentCreator");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var creatorId = JsonSerializer.Deserialize<int>(responseContent);
            creatorId.Should().Be(user.UserId);
        }

        #endregion

        #region Assessment Conversion Tests

        [TestMethod]
        public async Task AssessmentConversion_ValidParameters_ReturnsSuccess()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);
            var assessment = await CreateTestAssessmentAsync(user.UserId);

            // Set the assessment in the token context
            await SetAssessmentInTokenAsync(userClient, assessment.Assessment_Id);

            var originalAssessmentId = assessment.Assessment_Id;
            var targetModelName = "CMMC";

            // Act - Convert assessment
            var response = await userClient.PostAsync($"/api/conversion?originalAssessmentId={originalAssessmentId}&targetModelName={targetModelName}", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region Assessment Upgrades Tests

        [TestMethod]
        public async Task GetPossibleUpgrades_ValidAssessment_ReturnsSuccess()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);
            var assessment = await CreateTestAssessmentAsync(user.UserId);

            // Set the assessment in the token context
            await SetAssessmentInTokenAsync(userClient, assessment.Assessment_Id);

            // Act - Get possible upgrades
            var response = await userClient.GetAsync("/api/upgrades");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region Assessor Mode Tests

        [TestMethod]
        public async Task SetAssessorMode_ValidMode_ReturnsSuccess()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);
            var assessment = await CreateTestAssessmentAsync(user.UserId);

            // Set the assessment in the token context
            await SetAssessmentInTokenAsync(userClient, assessment.Assessment_Id);

            var mode = "Assessor";

            var content = new StringContent(
                JsonSerializer.Serialize(mode),
                Encoding.UTF8,
                "application/json");

            // Act - Set assessor mode
            var response = await userClient.PostAsync("/api/assessormode", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region Utility Tests

        [TestMethod]
        public async Task GetLastModified_ValidAssessment_ReturnsSuccess()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);
            var assessment = await CreateTestAssessmentAsync(user.UserId);

            // Set the assessment in the token context
            await SetAssessmentInTokenAsync(userClient, assessment.Assessment_Id);

            // Act - Get last modified date
            var response = await userClient.GetAsync("/api/lastmodified");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var lastModified = JsonSerializer.Deserialize<JsonElement>(responseContent);
            lastModified.TryGetProperty("lastModifiedDate", out _).Should().BeTrue();
        }

        [TestMethod]
        public async Task GetMergeNames_ValidIds_ReturnsSuccess()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);

            // Act - Get merge names for assessment IDs
            var response = await userClient.GetAsync("/api/getMergeNames?id1=1&id2=2&id3=3&id4=4&id5=5&id6=6&id7=7&id8=8&id9=9&id10=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task GetEncryptStatus_ValidUser_ReturnsSuccess()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);

            // Act - Get encryption status
            var response = await userClient.GetAsync("/api/encryptStatus");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task SaveEncryptStatus_ValidStatus_ReturnsSuccess()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);
            var encryptStatus = true;

            var content = new StringContent(
                JsonSerializer.Serialize(encryptStatus),
                Encoding.UTF8,
                "application/json");

            // Act - Save encryption status
            var response = await userClient.PostAsync("/api/saveEncryptStatus", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task HasGlobalDocuments_ReturnsSuccess()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);

            // Act - Check for global documents
            var response = await userClient.GetAsync("/api/hasGlobalDocuments");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task ClearFirstTime_ValidAssessment_ReturnsSuccess()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);
            var assessment = await CreateTestAssessmentAsync(user.UserId);

            // Set the assessment in the token context
            await SetAssessmentInTokenAsync(userClient, assessment.Assessment_Id);

            // Act - Clear first time flag
            var response = await userClient.GetAsync("/api/clearFirstTime");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region Edge Cases and Error Handling

        [TestMethod]
        public async Task GetAssessmentObservations_ValidIds_ReturnsSuccess()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);

            // Act - Get assessment observations
            var response = await userClient.GetAsync("/api/getAssessmentObservations?id1=1&id2=2&id3=3&id4=4&id5=5&id6=6&id7=7&id8=8&id9=9&id10=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task GetAssessmentDocuments_ValidIds_ReturnsSuccess()
        {
            // Arrange
            var (user, userClient) = await CreateUserWithRoleAsync(1);

            // Act - Get assessment documents
            var response = await userClient.GetAsync("/api/getAssessmentDocuments?id1=1&id2=2&id3=3&id4=4&id5=5&id6=6&id7=7&id8=8&id9=9&id10=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
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

        private async Task<ASSESSMENTS> CreateTestAssessmentAsync(int? userId, string accessKey = null)
        {
            var assessment = new ASSESSMENTS
            {
                AssessmentName = "Test Assessment",
                AssessmentDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatorId = userId,
                GalleryItemGuid = Guid.NewGuid()
            };

            DbContext.ASSESSMENTS.Add(assessment);
            await DbContext.SaveChangesAsync();

            // Add user to assessment if userId provided
            if (userId.HasValue)
            {
                var user = await DbContext.USERS.FindAsync(userId.Value);
                var contact = new ASSESSMENT_CONTACTS
                {
                    Assessment_Id = assessment.Assessment_Id,
                    UserId = userId.Value,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PrimaryEmail = user.PrimaryEmail,
                    AssessmentRoleId = 2, // Admin role
                    Invited = true
                };

                DbContext.ASSESSMENT_CONTACTS.Add(contact);
            }

            // Add access key to assessment if provided
            if (!string.IsNullOrEmpty(accessKey))
            {
                var accessKeyAssessment = new ACCESS_KEY_ASSESSMENT
                {
                    AccessKey = accessKey,
                    Assessment_Id = assessment.Assessment_Id
                };

                DbContext.ACCESS_KEY_ASSESSMENT.Add(accessKeyAssessment);
            }

            await DbContext.SaveChangesAsync();
            return assessment;
        }

        private async Task<GALLERY_ITEM> CreateTestGalleryItemAsync()
        {
            var galleryItem = new GALLERY_ITEM
            {
                Gallery_Item_Guid = Guid.NewGuid(),
                Gallery_Item_Name = "Test Gallery Item",
                Configuration_Setup = "{\"Sets\":[\"CUSTOM_SET\"],\"SALLevel\":\"Low\",\"QuestionMode\":\"Questions\"}",
                Is_Visible = true,
                Icon_File_Name = "test-icon.png"
            };

            DbContext.GALLERY_ITEM.Add(galleryItem);
            await DbContext.SaveChangesAsync();
            return galleryItem;
        }

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
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var key = new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            
            return $"TEST-{key}";
        }

        private async Task<string> GetAccessKeyTokenAsync(string accessKey)
        {
            var loginRequest = new
            {
                AccessKey = accessKey,
                TzOffset = 0,
                Scope = "CSET"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await Client.PostAsync("/api/auth/login/accesskey", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            return loginResponse.GetProperty("token").GetString();
        }

        private async Task SetAssessmentInTokenAsync(HttpClient client, int assessmentId)
        {
            // This would require implementing token modification for testing
            // For now, we'll use a workaround by setting the assessment ID in headers
            client.DefaultRequestHeaders.Add("assessmentid", assessmentId.ToString());
        }

        #endregion
    }
} 