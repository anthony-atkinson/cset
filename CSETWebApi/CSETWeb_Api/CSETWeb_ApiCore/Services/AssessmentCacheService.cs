//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWeb_ApiCore.Interfaces;
using CSETWeb_ApiCore.Models.Caching;
using CSETWebCore.DataLayer.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSETWeb_ApiCore.Services
{
    /// <summary>
    /// Specialized caching service for assessment data
    /// </summary>
    public class AssessmentCacheService
    {
        private readonly ICacheService _cacheService;
        private readonly CachingConfiguration _config;
        private readonly ILogger<AssessmentCacheService> _logger;
        private readonly CSETContext _context;

        public AssessmentCacheService(
            ICacheService cacheService,
            IOptions<CachingConfiguration> config,
            ILogger<AssessmentCacheService> logger,
            CSETContext context)
        {
            _cacheService = cacheService;
            _config = config.Value;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Gets assessment metadata with caching
        /// </summary>
        public async Task<ASSESSMENTS> GetAssessmentAsync(int assessmentId)
        {
            var cacheKey = string.Format(_config.CacheKeys.Assessments, assessmentId);
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                _logger.LogDebug("Loading assessment from database: {AssessmentId}", assessmentId);
                
                return _context.ASSESSMENTS
                    .FirstOrDefault(a => a.Assessment_Id == assessmentId);
            }, 30); // Cache for 30 minutes
        }

        /// <summary>
        /// Gets assessment answers with caching
        /// </summary>
        public async Task<List<ANSWER>> GetAssessmentAnswersAsync(int assessmentId)
        {
            var cacheKey = $"answers:{assessmentId}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                _logger.LogDebug("Loading assessment answers from database: {AssessmentId}", assessmentId);
                
                return _context.ANSWER
                    .Where(a => a.Assessment_Id == assessmentId)
                    .ToList();
            }, 15); // Cache for 15 minutes (shorter due to frequent updates)
        }

        /// <summary>
        /// Gets assessment questions with caching
        /// </summary>
        public async Task<List<NEW_QUESTION>> GetAssessmentQuestionsAsync(int assessmentId)
        {
            var cacheKey = string.Format(_config.CacheKeys.Questions, assessmentId);
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                _logger.LogDebug("Loading assessment questions from database: {AssessmentId}", assessmentId);
                
                // Get questions based on selected standards
                var selectedStandards = _context.AVAILABLE_STANDARDS
                    .Where(x => x.Assessment_Id == assessmentId && x.Selected)
                    .Select(x => x.Set_Name)
                    .ToList();

                var questions = from q in _context.NEW_QUESTION
                              join qs in _context.NEW_QUESTION_SETS on q.Question_Id equals qs.Question_Id
                              where selectedStandards.Contains(qs.Set_Name)
                              select q;

                return questions.Distinct().ToList();
            }, 60); // Cache for 1 hour
        }

        /// <summary>
        /// Gets assessment demographics with caching
        /// </summary>
        public async Task<DEMOGRAPHICS> GetAssessmentDemographicsAsync(int assessmentId)
        {
            var cacheKey = $"demographics:{assessmentId}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                _logger.LogDebug("Loading assessment demographics from database: {AssessmentId}", assessmentId);
                
                return _context.DEMOGRAPHICS
                    .FirstOrDefault(d => d.Assessment_Id == assessmentId);
            }, 45); // Cache for 45 minutes
        }

        /// <summary>
        /// Gets assessment documents with caching
        /// </summary>
        public async Task<List<DOCUMENT_FILE>> GetAssessmentDocumentsAsync(int assessmentId)
        {
            var cacheKey = $"documents:{assessmentId}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                _logger.LogDebug("Loading assessment documents from database: {AssessmentId}", assessmentId);
                
                return _context.DOCUMENT_FILE
                    .Where(d => d.Assessment_Id == assessmentId)
                    .ToList();
            }, 90); // Cache for 1.5 hours
        }

        /// <summary>
        /// Gets user assessments with caching
        /// </summary>
        public async Task<List<ASSESSMENTS>> GetUserAssessmentsAsync(int userId)
        {
            var cacheKey = $"user-assessments:{userId}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                _logger.LogDebug("Loading user assessments from database: {UserId}", userId);
                
                return _context.ASSESSMENTS
                    .Where(a => a.AssessmentCreatorId == userId)
                    .OrderByDescending(a => a.AssessmentCreatedDate)
                    .ToList();
            }, 20); // Cache for 20 minutes
        }

        /// <summary>
        /// Invalidates assessment cache
        /// </summary>
        public async Task InvalidateAssessmentCacheAsync(int assessmentId)
        {
            var cacheKeys = new[]
            {
                string.Format(_config.CacheKeys.Assessments, assessmentId),
                $"answers:{assessmentId}",
                string.Format(_config.CacheKeys.Questions, assessmentId),
                $"demographics:{assessmentId}",
                $"documents:{assessmentId}"
            };

            foreach (var key in cacheKeys)
            {
                await _cacheService.RemoveAsync(key);
            }

            _logger.LogDebug("Invalidated assessment cache for assessment: {AssessmentId}", assessmentId);
        }

        /// <summary>
        /// Invalidates user assessments cache
        /// </summary>
        public async Task InvalidateUserAssessmentsCacheAsync(int userId)
        {
            var cacheKey = $"user-assessments:{userId}";
            await _cacheService.RemoveAsync(cacheKey);
            _logger.LogDebug("Invalidated user assessments cache for user: {UserId}", userId);
        }

        /// <summary>
        /// Invalidates all assessment-related cache
        /// </summary>
        public async Task InvalidateAllAssessmentCacheAsync()
        {
            await _cacheService.InvalidateByPatternAsync("assessments:*");
            await _cacheService.InvalidateByPatternAsync("answers:*");
            await _cacheService.InvalidateByPatternAsync("questions:*");
            await _cacheService.InvalidateByPatternAsync("demographics:*");
            await _cacheService.InvalidateByPatternAsync("documents:*");
            await _cacheService.InvalidateByPatternAsync("user-assessments:*");
            _logger.LogInformation("Invalidated all assessment-related cache");
        }

        /// <summary>
        /// Gets assessment completion statistics with caching
        /// </summary>
        public async Task<AssessmentCompletionStats> GetAssessmentCompletionStatsAsync(int assessmentId)
        {
            var cacheKey = $"completion-stats:{assessmentId}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                _logger.LogDebug("Calculating assessment completion stats: {AssessmentId}", assessmentId);
                
                var answers = await GetAssessmentAnswersAsync(assessmentId);
                var questions = await GetAssessmentQuestionsAsync(assessmentId);

                var totalQuestions = questions.Count;
                var answeredQuestions = answers.Count(a => a.Answer_Text != "U" && a.Answer_Text != null);
                var completionPercentage = totalQuestions > 0 ? (double)answeredQuestions / totalQuestions * 100 : 0;

                return new AssessmentCompletionStats
                {
                    AssessmentId = assessmentId,
                    TotalQuestions = totalQuestions,
                    AnsweredQuestions = answeredQuestions,
                    CompletionPercentage = completionPercentage,
                    LastCalculated = DateTime.UtcNow
                };
            }, 10); // Cache for 10 minutes
        }
    }

    /// <summary>
    /// Assessment completion statistics
    /// </summary>
    public class AssessmentCompletionStats
    {
        public int AssessmentId { get; set; }
        public int TotalQuestions { get; set; }
        public int AnsweredQuestions { get; set; }
        public double CompletionPercentage { get; set; }
        public DateTime LastCalculated { get; set; }
    }
} 