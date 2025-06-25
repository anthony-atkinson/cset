//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWeb_ApiCore.Interfaces;
using CSETWeb_ApiCore.Models.Caching;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Model.Standards;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSETWeb_ApiCore.Services
{
    /// <summary>
    /// Specialized caching service for standards and framework data
    /// </summary>
    public class StandardsCacheService
    {
        private readonly ICacheService _cacheService;
        private readonly CachingConfiguration _config;
        private readonly ILogger<StandardsCacheService> _logger;
        private readonly CSETContext _context;

        public StandardsCacheService(
            ICacheService cacheService,
            IOptions<CachingConfiguration> config,
            ILogger<StandardsCacheService> logger,
            CSETContext context)
        {
            _cacheService = cacheService;
            _config = config.Value;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Gets standards for an assessment with caching
        /// </summary>
        public async Task<StandardsResponse> GetStandardsAsync(int assessmentId)
        {
            var cacheKey = string.Format(_config.CacheKeys.Standards, assessmentId);
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                _logger.LogDebug("Loading standards from database for assessment: {AssessmentId}", assessmentId);
                
                var response = new StandardsResponse();
                var categories = new List<StandardCategory>();

                var sets = from s in _context.SETS
                          join ss in _context.SET_CATEGORY on s.Set_Category_Id equals ss.Set_Category_Id
                          orderby ss.Set_Category_Name, s.Short_Name
                          select new { s, ss };

                var selectedSets = _context.AVAILABLE_STANDARDS
                    .Where(x => x.Assessment_Id == assessmentId && x.Selected)
                    .Select(x => x.Set_Name)
                    .ToList();

                var recommendedSets = _context.SECTOR_STANDARD_RECOMMENDATIONS
                    .Where(x => x.Sector_Id == 1) // Default sector
                    .Select(x => x.Set_Name)
                    .ToList();

                string currCategoryName = "";
                foreach (var set in sets)
                {
                    if (set.ss.Set_Category_Name != currCategoryName)
                    {
                        var cat = new StandardCategory();
                        cat.CategoryName = set.ss.Set_Category_Name;
                        categories.Add(cat);
                        currCategoryName = set.ss.Set_Category_Name;
                    }

                    var std = new Standard
                    {
                        Code = set.s.Set_Name,
                        FullName = set.s.Full_Name,
                        Description = set.s.Standard_ToolTip,
                        Selected = selectedSets.Contains(set.s.Set_Name),
                        Recommended = recommendedSets.Contains(set.s.Set_Name)
                    };
                    categories.Last().Standards.Add(std);
                }

                response.Categories = categories;
                return response;
            }, 120); // Cache for 2 hours
        }

        /// <summary>
        /// Gets framework data with caching
        /// </summary>
        public async Task<FrameworkResponse> GetFrameworksAsync(int assessmentId)
        {
            var cacheKey = string.Format(_config.CacheKeys.Frameworks, assessmentId);
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                _logger.LogDebug("Loading framework data from database for assessment: {AssessmentId}", assessmentId);
                
                var response = new FrameworkResponse();
                
                // Load framework questions and tiers
                var frameworkQuestions = _context.NEW_QUESTION
                    .Where(q => q.Is_Framework == true)
                    .ToList();

                var tiers = _context.FRAMEWORK_TIER_TYPE
                    .OrderBy(t => t.Tier_Order)
                    .ToList();

                response.Questions = frameworkQuestions;
                response.Tiers = tiers;
                
                return response;
            }, 180); // Cache for 3 hours
        }

        /// <summary>
        /// Gets maturity models with caching
        /// </summary>
        public async Task<List<MATURITY_MODELS>> GetMaturityModelsAsync()
        {
            var cacheKey = string.Format(_config.CacheKeys.MaturityModels, "all");
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                _logger.LogDebug("Loading maturity models from database");
                
                return _context.MATURITY_MODELS
                    .OrderBy(m => m.Model_Name)
                    .ToList();
            }, 240); // Cache for 4 hours
        }

        /// <summary>
        /// Gets question headings with caching
        /// </summary>
        public async Task<List<vQUESTION_HEADINGS>> GetQuestionHeadingsAsync()
        {
            var cacheKey = string.Format(_config.CacheKeys.QuestionHeadings, "all");
            
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                _logger.LogDebug("Loading question headings from database");
                
                return _context.vQUESTION_HEADINGS
                    .OrderBy(h => h.Question_Group_Heading)
                    .ThenBy(h => h.Universal_Sub_Category)
                    .ToList();
            }, 300); // Cache for 5 hours
        }

        /// <summary>
        /// Invalidates standards cache for an assessment
        /// </summary>
        public async Task InvalidateStandardsCacheAsync(int assessmentId)
        {
            var cacheKey = string.Format(_config.CacheKeys.Standards, assessmentId);
            await _cacheService.RemoveAsync(cacheKey);
            _logger.LogDebug("Invalidated standards cache for assessment: {AssessmentId}", assessmentId);
        }

        /// <summary>
        /// Invalidates framework cache for an assessment
        /// </summary>
        public async Task InvalidateFrameworkCacheAsync(int assessmentId)
        {
            var cacheKey = string.Format(_config.CacheKeys.Frameworks, assessmentId);
            await _cacheService.RemoveAsync(cacheKey);
            _logger.LogDebug("Invalidated framework cache for assessment: {AssessmentId}", assessmentId);
        }

        /// <summary>
        /// Invalidates all standards-related cache
        /// </summary>
        public async Task InvalidateAllStandardsCacheAsync()
        {
            await _cacheService.InvalidateByPatternAsync("standards:*");
            await _cacheService.InvalidateByPatternAsync("frameworks:*");
            await _cacheService.InvalidateByPatternAsync("maturity:*");
            await _cacheService.InvalidateByPatternAsync("headings:*");
            _logger.LogInformation("Invalidated all standards-related cache");
        }
    }
} 