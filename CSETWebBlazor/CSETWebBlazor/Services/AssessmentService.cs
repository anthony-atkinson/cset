using CSETWebBlazor.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CSETWebBlazor.Services
{
    /// <summary>
    /// Service for assessment management operations
    /// </summary>
    public class AssessmentService : IAssessmentService
    {
        private readonly IApiClientService _apiClient;
        private readonly IAuthenticationService _authService;
        private readonly IConfigService _configService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AssessmentService> _logger;
        private readonly IErrorHandlingService _errorHandling;

        // Current assessment state
        private AssessmentDetail? _currentAssessment;
        private int _userRoleId;
        private string _currentTab = string.Empty;
        private string _applicationMode = string.Empty;
        private bool _isBrandNew = false;
        private bool _hideUpgradeAlert = false;
        private string _galleryItemGuid = string.Empty;
        private string _convertToModel = string.Empty;

        // Cached data
        private List<Role>? _roles;
        private List<MaturityModel>? _maturityModels;
        private List<OrganizationType>? _organizationTypes;

        // Events
        public event Action<AssessmentDetail?>? AssessmentChanged;
        public event Action<string>? AssessmentStateChanged;

        public AssessmentService(
            IApiClientService apiClient,
            IAuthenticationService authService,
            IConfigService configService,
            IMemoryCache cache,
            ILogger<AssessmentService> logger,
            IErrorHandlingService errorHandling)
        {
            _apiClient = apiClient;
            _authService = authService;
            _configService = configService;
            _cache = cache;
            _logger = logger;
            _errorHandling = errorHandling;

            InitializeAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Initialize the service by loading roles and maturity models
        /// </summary>
        private async Task InitializeAsync()
        {
            try
            {
                await LoadRolesAsync();
                await LoadMaturityModelsAsync();
                await LoadOrganizationTypesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize AssessmentService");
                await _errorHandling.HandleErrorAsync(ex, "Failed to initialize assessment service");
            }
        }

        #region Assessment Management

        public async Task<List<AssessmentDetail>> GetAssessmentsAsync()
        {
            try
            {
                var cacheKey = "assessments_for_user";
                if (_cache.TryGetValue(cacheKey, out List<AssessmentDetail>? cachedAssessments))
                {
                    return cachedAssessments ?? new List<AssessmentDetail>();
                }

                var response = await _apiClient.GetAsync<List<AssessmentDetail>>("assessmentsforuser");
                if (response != null)
                {
                    _cache.Set(cacheKey, response, TimeSpan.FromMinutes(5));
                    return response;
                }

                return new List<AssessmentDetail>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get assessments");
                await _errorHandling.HandleErrorAsync(ex, "Failed to retrieve assessments");
                return new List<AssessmentDetail>();
            }
        }

        public async Task<List<AssessmentCompletion>> GetAssessmentsCompletionAsync()
        {
            try
            {
                var cacheKey = "assessments_completion_for_user";
                if (_cache.TryGetValue(cacheKey, out List<AssessmentCompletion>? cachedCompletions))
                {
                    return cachedCompletions ?? new List<AssessmentCompletion>();
                }

                var response = await _apiClient.GetAsync<List<AssessmentCompletion>>("assessmentsCompletionForUser");
                if (response != null)
                {
                    _cache.Set(cacheKey, response, TimeSpan.FromMinutes(5));
                    return response;
                }

                return new List<AssessmentCompletion>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get assessments completion");
                await _errorHandling.HandleErrorAsync(ex, "Failed to retrieve assessment completion data");
                return new List<AssessmentCompletion>();
            }
        }

        public async Task<AssessmentDetail?> GetAssessmentDetailAsync()
        {
            try
            {
                if (_currentAssessment != null)
                {
                    return _currentAssessment;
                }

                var response = await _apiClient.GetAsync<AssessmentDetail>("assessmentdetail");
                if (response != null)
                {
                    _currentAssessment = response;
                    AssessmentChanged?.Invoke(_currentAssessment);
                    return _currentAssessment;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get assessment detail");
                await _errorHandling.HandleErrorAsync(ex, "Failed to retrieve assessment details");
                return null;
            }
        }

        public async Task<AssessmentDetail?> LoadAssessmentAsync(int assessmentId)
        {
            try
            {
                // Get assessment token first
                await GetAssessmentTokenAsync(assessmentId);

                // Load assessment detail
                var assessment = await GetAssessmentDetailAsync();
                if (assessment != null)
                {
                    _currentAssessment = assessment;
                    AssessmentChanged?.Invoke(_currentAssessment);
                    
                    // Clear relevant caches
                    _cache.Remove("assessments_for_user");
                    _cache.Remove("assessments_completion_for_user");
                    
                    return assessment;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load assessment {AssessmentId}", assessmentId);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to load assessment {assessmentId}");
                return null;
            }
        }

        public async Task<AssessmentDetail> CreateAssessmentAsync(CreateAssessmentRequest request)
        {
            try
            {
                var response = await _apiClient.PostAsync<AssessmentDetail>("createassessment", request);
                if (response != null)
                {
                    _currentAssessment = response;
                    _isBrandNew = true;
                    AssessmentChanged?.Invoke(_currentAssessment);
                    
                    // Clear caches
                    _cache.Remove("assessments_for_user");
                    _cache.Remove("assessments_completion_for_user");
                    
                    return response;
                }

                throw new InvalidOperationException("Failed to create assessment");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create assessment");
                await _errorHandling.HandleErrorAsync(ex, "Failed to create assessment");
                throw;
            }
        }

        public async Task<bool> UpdateAssessmentDetailsAsync(UpdateAssessmentRequest request)
        {
            try
            {
                var response = await _apiClient.PutAsync<bool>("assessmentdetail", request);
                if (response)
                {
                    // Refresh current assessment
                    await RefreshAssessmentAsync();
                }
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update assessment details");
                await _errorHandling.HandleErrorAsync(ex, "Failed to update assessment details");
                return false;
            }
        }

        public async Task<bool> DeleteAssessmentAsync(int assessmentId)
        {
            try
            {
                var response = await _apiClient.DeleteAsync<bool>($"assessment/{assessmentId}");
                if (response)
                {
                    // Clear caches
                    _cache.Remove("assessments_for_user");
                    _cache.Remove("assessments_completion_for_user");
                    
                    // If this was the current assessment, clear it
                    if (_currentAssessment?.AssessmentId == assessmentId)
                    {
                        await DropAssessmentAsync();
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete assessment {AssessmentId}", assessmentId);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to delete assessment {assessmentId}");
                return false;
            }
        }

        public async Task<bool> RefreshAssessmentAsync()
        {
            try
            {
                if (_currentAssessment == null)
                {
                    return false;
                }

                var assessment = await GetAssessmentDetailAsync();
                if (assessment != null)
                {
                    _currentAssessment = assessment;
                    AssessmentChanged?.Invoke(_currentAssessment);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh assessment");
                await _errorHandling.HandleErrorAsync(ex, "Failed to refresh assessment");
                return false;
            }
        }

        public async Task<DateTime?> GetLastModifiedAsync()
        {
            try
            {
                if (_currentAssessment?.LastModifiedDate != null)
                {
                    return _currentAssessment.LastModifiedDate;
                }

                var response = await _apiClient.GetAsync<DateTime?>("assessment/lastmodified");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get last modified date");
                await _errorHandling.HandleErrorAsync(ex, "Failed to get last modified date");
                return null;
            }
        }

        #endregion

        #region Assessment Token Management

        public async Task<string> GetAssessmentTokenAsync(int assessmentId)
        {
            try
            {
                var headers = new Dictionary<string, string>
                {
                    ["AssessmentId"] = assessmentId.ToString()
                };

                var response = await _apiClient.GetAsync<dynamic>("auth/token", headers);
                if (response != null && response.token != null)
                {
                    var token = response.token.ToString();
                    
                    // Store token in authentication service
                    await _authService.SetAssessmentTokenAsync(token, assessmentId);
                    
                    return token;
                }

                throw new InvalidOperationException("Failed to get assessment token");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get assessment token for assessment {AssessmentId}", assessmentId);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to get assessment token for assessment {assessmentId}");
                throw;
            }
        }

        public async Task<bool> DropAssessmentAsync()
        {
            try
            {
                _userRoleId = 0;
                _currentTab = string.Empty;
                _applicationMode = string.Empty;
                _currentAssessment = null;
                _isBrandNew = false;
                
                // Clear assessment token
                await _authService.ClearAssessmentTokenAsync();
                
                AssessmentChanged?.Invoke(null);
                AssessmentStateChanged?.Invoke("assessment_dropped");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to drop assessment");
                await _errorHandling.HandleErrorAsync(ex, "Failed to drop assessment");
                return false;
            }
        }

        #endregion

        #region Maturity Models

        public async Task<List<MaturityModel>> GetAllMaturityModelsAsync()
        {
            try
            {
                if (_maturityModels != null)
                {
                    return _maturityModels;
                }

                await LoadMaturityModelsAsync();
                return _maturityModels ?? new List<MaturityModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all maturity models");
                await _errorHandling.HandleErrorAsync(ex, "Failed to retrieve maturity models");
                return new List<MaturityModel>();
            }
        }

        private async Task LoadMaturityModelsAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<List<MaturityModel>>("MaturityModels");
                if (response != null)
                {
                    _maturityModels = response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load maturity models");
                await _errorHandling.HandleErrorAsync(ex, "Failed to load maturity models");
            }
        }

        public async Task<bool> UsesMaturityModelAsync(string modelName)
        {
            try
            {
                if (_currentAssessment?.MaturityModels == null)
                {
                    return false;
                }

                return _currentAssessment.MaturityModels.Any(m => 
                    string.Equals(m.ModelName, modelName, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if assessment uses maturity model {ModelName}", modelName);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to check maturity model usage: {modelName}");
                return false;
            }
        }

        public async Task<bool> UsesMaturityModelIdAsync(int modelId)
        {
            try
            {
                if (_currentAssessment?.MaturityModels == null)
                {
                    return false;
                }

                return _currentAssessment.MaturityModels.Any(m => m.ModelId == modelId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if assessment uses maturity model ID {ModelId}", modelId);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to check maturity model usage by ID: {modelId}");
                return false;
            }
        }

        public async Task<bool> SetModelAsync(string modelName)
        {
            try
            {
                if (_currentAssessment == null)
                {
                    return false;
                }

                var request = new { ModelName = modelName };
                var response = await _apiClient.PostAsync<bool>("assessment/setmodel", request);
                
                if (response)
                {
                    await RefreshAssessmentAsync();
                }
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set model {ModelName}", modelName);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to set model: {modelName}");
                return false;
            }
        }

        #endregion

        #region Standards

        public async Task<bool> UsesStandardAsync(string setName)
        {
            try
            {
                if (_currentAssessment?.Standards == null)
                {
                    return false;
                }

                return _currentAssessment.Standards.Any(s => 
                    string.Equals(s, setName, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if assessment uses standard {SetName}", setName);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to check standard usage: {setName}");
                return false;
            }
        }

        #endregion

        #region Assessment Features

        public async Task<bool> HasFeatureAsync(string feature)
        {
            try
            {
                if (_currentAssessment?.AssessmentFeatures == null)
                {
                    return false;
                }

                return _currentAssessment.AssessmentFeatures.Contains(feature);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if assessment has feature {Feature}", feature);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to check feature: {feature}");
                return false;
            }
        }

        public async Task<bool> ChangeFeatureAsync(string feature, bool state)
        {
            try
            {
                if (_currentAssessment == null)
                {
                    return false;
                }

                var request = new { Feature = feature, State = state };
                var response = await _apiClient.PostAsync<bool>("assessment/changefeature", request);
                
                if (response)
                {
                    await RefreshAssessmentAsync();
                }
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to change feature {Feature} to {State}", feature, state);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to change feature {feature} to {state}");
                return false;
            }
        }

        public async Task<bool> HasDiagramAsync()
        {
            try
            {
                if (_currentAssessment != null)
                {
                    return _currentAssessment.HasDiagram;
                }

                var response = await _apiClient.GetAsync<bool>("assessment/hasdiagram");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if assessment has diagram");
                await _errorHandling.HandleErrorAsync(ex, "Failed to check if assessment has diagram");
                return false;
            }
        }

        #endregion

        #region Assessment Contacts

        public async Task<AssessmentContactsResponse> GetAssessmentContactsAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<AssessmentContactsResponse>("contacts");
                return response ?? new AssessmentContactsResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get assessment contacts");
                await _errorHandling.HandleErrorAsync(ex, "Failed to retrieve assessment contacts");
                return new AssessmentContactsResponse();
            }
        }

        public async Task<AssessmentContactsResponse> GetAssessmentContactsByIdAsync(int[] contactIds)
        {
            try
            {
                var request = new { ContactIds = contactIds };
                var response = await _apiClient.PostAsync<AssessmentContactsResponse>("contacts/byids", request);
                return response ?? new AssessmentContactsResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get assessment contacts by IDs");
                await _errorHandling.HandleErrorAsync(ex, "Failed to retrieve assessment contacts by IDs");
                return new AssessmentContactsResponse();
            }
        }

        public async Task<AssessmentContact> CreateContactAsync(CreateContactRequest request)
        {
            try
            {
                var response = await _apiClient.PostAsync<AssessmentContact>("contacts", request);
                if (response != null)
                {
                    // Clear contacts cache
                    _cache.Remove("assessment_contacts");
                    return response;
                }

                throw new InvalidOperationException("Failed to create contact");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create contact");
                await _errorHandling.HandleErrorAsync(ex, "Failed to create contact");
                throw;
            }
        }

        public async Task<bool> UpdateContactAsync(AssessmentContact contact)
        {
            try
            {
                var response = await _apiClient.PutAsync<bool>($"contacts/{contact.AssessmentContactId}", contact);
                if (response)
                {
                    // Clear contacts cache
                    _cache.Remove("assessment_contacts");
                }
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update contact {ContactId}", contact.AssessmentContactId);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to update contact {contact.AssessmentContactId}");
                return false;
            }
        }

        public async Task<bool> RemoveContactAsync(int assessmentContactId)
        {
            try
            {
                var response = await _apiClient.DeleteAsync<bool>($"contacts/{assessmentContactId}");
                if (response)
                {
                    // Clear contacts cache
                    _cache.Remove("assessment_contacts");
                }
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove contact {ContactId}", assessmentContactId);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to remove contact {assessmentContactId}");
                return false;
            }
        }

        public async Task<bool> RemoveMyContactAsync(int assessmentId)
        {
            try
            {
                var response = await _apiClient.DeleteAsync<bool>($"contacts/mycontact/{assessmentId}");
                if (response)
                {
                    // Clear contacts cache
                    _cache.Remove("assessment_contacts");
                }
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove my contact for assessment {AssessmentId}", assessmentId);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to remove my contact for assessment {assessmentId}");
                return false;
            }
        }

        public async Task<List<AssessmentContact>> SearchContactsAsync(ContactSearchRequest request)
        {
            try
            {
                var response = await _apiClient.PostAsync<List<AssessmentContact>>("contacts/search", request);
                return response ?? new List<AssessmentContact>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search contacts");
                await _errorHandling.HandleErrorAsync(ex, "Failed to search contacts");
                return new List<AssessmentContact>();
            }
        }

        #endregion

        #region Roles

        public async Task<List<Role>> GetRolesAsync()
        {
            try
            {
                if (_roles != null)
                {
                    return _roles;
                }

                await LoadRolesAsync();
                return _roles ?? new List<Role>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get roles");
                await _errorHandling.HandleErrorAsync(ex, "Failed to retrieve roles");
                return new List<Role>();
            }
        }

        private async Task LoadRolesAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<List<Role>>("contacts/allroles");
                if (response != null)
                {
                    _roles = response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load roles");
                await _errorHandling.HandleErrorAsync(ex, "Failed to load roles");
            }
        }

        public async Task RefreshRolesAsync()
        {
            try
            {
                _roles = null;
                await LoadRolesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh roles");
                await _errorHandling.HandleErrorAsync(ex, "Failed to refresh roles");
            }
        }

        #endregion

        #region Organization Types

        public async Task<List<OrganizationType>> GetOrganizationTypesAsync()
        {
            try
            {
                if (_organizationTypes != null)
                {
                    return _organizationTypes;
                }

                await LoadOrganizationTypesAsync();
                return _organizationTypes ?? new List<OrganizationType>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get organization types");
                await _errorHandling.HandleErrorAsync(ex, "Failed to retrieve organization types");
                return new List<OrganizationType>();
            }
        }

        private async Task LoadOrganizationTypesAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<List<OrganizationType>>("organizationtypes");
                if (response != null)
                {
                    _organizationTypes = response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load organization types");
                await _errorHandling.HandleErrorAsync(ex, "Failed to load organization types");
            }
        }

        #endregion

        #region Other Remarks

        public async Task<string> GetOtherRemarksAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<string>("assessment/otherremarks");
                return response ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get other remarks");
                await _errorHandling.HandleErrorAsync(ex, "Failed to retrieve other remarks");
                return string.Empty;
            }
        }

        public async Task<bool> SaveOtherRemarksAsync(string remarks)
        {
            try
            {
                var request = new { Remarks = remarks };
                var response = await _apiClient.PostAsync<bool>("assessment/otherremarks", request);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save other remarks");
                await _errorHandling.HandleErrorAsync(ex, "Failed to save other remarks");
                return false;
            }
        }

        #endregion

        #region Assessment Creator

        public async Task<AssessmentContact?> GetCreatorAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<AssessmentContact>("assessment/creator");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get assessment creator");
                await _errorHandling.HandleErrorAsync(ex, "Failed to retrieve assessment creator");
                return null;
            }
        }

        #endregion

        #region Assessment Documents

        public async Task<List<string>> GetAssessmentDocumentsAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<List<string>>("assessment/documents");
                return response ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get assessment documents");
                await _errorHandling.HandleErrorAsync(ex, "Failed to retrieve assessment documents");
                return new List<string>();
            }
        }

        #endregion

        #region Assessment Conversion

        public async Task<bool> ConvertAssessmentAsync(int originalId)
        {
            try
            {
                var request = new { OriginalId = originalId };
                var response = await _apiClient.PostAsync<bool>("assessment/convert", request);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to convert assessment {OriginalId}", originalId);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to convert assessment {originalId}");
                return false;
            }
        }

        public async Task<bool> CheckUpgradesAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<bool>("assessment/checkupgrades");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check upgrades");
                await _errorHandling.HandleErrorAsync(ex, "Failed to check upgrades");
                return false;
            }
        }

        #endregion

        #region Assessment Settings

        public async Task<bool> SetAssessorSettingAsync(bool mode)
        {
            try
            {
                var request = new { Mode = mode };
                var response = await _apiClient.PostAsync<bool>("assessment/assessorsetting", request);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set assessor setting to {Mode}", mode);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to set assessor setting to {mode}");
                return false;
            }
        }

        public async Task<bool> IsDeletePermittedAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<bool>("assessment/isdeletepermitted");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if delete is permitted");
                await _errorHandling.HandleErrorAsync(ex, "Failed to check if delete is permitted");
                return false;
            }
        }

        public async Task<bool> IsPciiAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<bool>("assessment/ispcii");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if assessment is PCII");
                await _errorHandling.HandleErrorAsync(ex, "Failed to check if assessment is PCII");
                return false;
            }
        }

        public async Task<bool> PersistEncryptPreferenceAsync(bool status)
        {
            try
            {
                var request = new { Status = status };
                var response = await _apiClient.PostAsync<bool>("assessment/encryptpreference", request);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist encrypt preference {Status}", status);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to persist encrypt preference {status}");
                return false;
            }
        }

        public async Task<bool> GetEncryptPreferenceAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<bool>("assessment/encryptpreference");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get encrypt preference");
                await _errorHandling.HandleErrorAsync(ex, "Failed to get encrypt preference");
                return false;
            }
        }

        public async Task<bool> HasGlobalDocumentsAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<bool>("assessment/hasglobaldocuments");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if assessment has global documents");
                await _errorHandling.HandleErrorAsync(ex, "Failed to check if assessment has global documents");
                return false;
            }
        }

        #endregion

        #region Assessment Answers

        public async Task<bool> UpdateAnswerAsync(Answer answer)
        {
            try
            {
                var response = await _apiClient.PutAsync<bool>("answer", answer);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update answer for question {QuestionId}", answer.QuestionId);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to update answer for question {answer.QuestionId}");
                return false;
            }
        }

        #endregion

        #region Assessment State

        public async Task<string> GetModeAsync()
        {
            return _applicationMode;
        }

        public async Task<int> GetCurrentAssessmentIdAsync()
        {
            return _currentAssessment?.AssessmentId ?? 0;
        }

        public async Task<bool> IsBrandNewAssessmentAsync()
        {
            return _isBrandNew;
        }

        #endregion

        #region Utility Methods

        public async Task<string> FormatLinebreaksAsync(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return text.Replace("\r\n", "<br />")
                      .Replace("\r", "<br />")
                      .Replace("\n", "<br />");
        }

        public async Task<bool> ClearFirstTimeAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<bool>("clearFirstTime");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear first time");
                await _errorHandling.HandleErrorAsync(ex, "Failed to clear first time");
                return false;
            }
        }

        #endregion
    }
} 