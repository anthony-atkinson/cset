using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CSETWebCore.PlaywrightTests.Infrastructure
{
    /// <summary>
    /// Base test fixture for visual regression testing with Playwright
    /// </summary>
    [TestFixture]
    public abstract class VisualRegressionTestFixture : CrossBrowserTestFixture
    {
        protected string BaselineImagesPath { get; private set; } = string.Empty;
        protected string CurrentImagesPath { get; private set; } = string.Empty;
        protected string DiffImagesPath { get; private set; } = string.Empty;
        protected double VisualThreshold { get; set; } = 0.02; // 2% difference threshold
        protected bool UpdateBaselines { get; set; } = false;

        protected override async Task BrowserSpecificSetupAsync()
        {
            await base.BrowserSpecificSetupAsync();
            
            // Setup visual regression paths
            var testResultsPath = Path.Combine(Directory.GetCurrentDirectory(), "test-results");
            BaselineImagesPath = Path.Combine(testResultsPath, "visual-baselines", BrowserName.ToLower());
            CurrentImagesPath = Path.Combine(testResultsPath, "visual-current", BrowserName.ToLower());
            DiffImagesPath = Path.Combine(testResultsPath, "visual-diffs", BrowserName.ToLower());
            
            // Create directories
            Directory.CreateDirectory(BaselineImagesPath);
            Directory.CreateDirectory(CurrentImagesPath);
            Directory.CreateDirectory(DiffImagesPath);
            
            // Set viewport to consistent size for visual testing
            await Page.SetViewportSizeAsync(1920, 1080);
        }

        /// <summary>
        /// Take a screenshot and compare with baseline
        /// </summary>
        protected async Task<VisualComparisonResult> CompareScreenshotAsync(string testName, string elementSelector = "body", bool fullPage = false)
        {
            var baselinePath = Path.Combine(BaselineImagesPath, $"{testName}.png");
            var currentPath = Path.Combine(CurrentImagesPath, $"{testName}.png");
            var diffPath = Path.Combine(DiffImagesPath, $"{testName}_diff.png");

            // Take current screenshot
            var screenshotOptions = new PageScreenshotOptions
            {
                Path = currentPath,
                FullPage = fullPage
            };

            if (!string.IsNullOrEmpty(elementSelector) && elementSelector != "body")
            {
                var element = Page.Locator(elementSelector);
                await element.ScreenshotAsync(new LocatorScreenshotOptions { Path = currentPath });
            }
            else
            {
                await Page.ScreenshotAsync(screenshotOptions);
            }

            // Check if baseline exists
            if (!File.Exists(baselinePath))
            {
                if (UpdateBaselines)
                {
                    // Copy current to baseline
                    File.Copy(currentPath, baselinePath, true);
                    return new VisualComparisonResult
                    {
                        TestName = testName,
                        BaselineCreated = true,
                        IsMatch = true,
                        DifferencePercentage = 0.0
                    };
                }
                else
                {
                    throw new FileNotFoundException($"Baseline image not found: {baselinePath}. Set UpdateBaselines=true to create baseline.");
                }
            }

            // Compare images
            var comparisonResult = await CompareImagesAsync(baselinePath, currentPath, diffPath);
            comparisonResult.TestName = testName;
            comparisonResult.BaselinePath = baselinePath;
            comparisonResult.CurrentPath = currentPath;
            comparisonResult.DiffPath = diffPath;

            return comparisonResult;
        }

        /// <summary>
        /// Compare two images and generate diff
        /// </summary>
        private async Task<VisualComparisonResult> CompareImagesAsync(string baselinePath, string currentPath, string diffPath)
        {
            try
            {
                using var baselineImage = await Image.LoadAsync(baselinePath);
                using var currentImage = await Image.LoadAsync(currentPath);

                // Ensure images are the same size
                if (baselineImage.Width != currentImage.Width || baselineImage.Height != currentImage.Height)
                {
                    return new VisualComparisonResult
                    {
                        IsMatch = false,
                        DifferencePercentage = 100.0,
                        ErrorMessage = "Images have different dimensions"
                    };
                }

                // Convert to grayscale for comparison
                using var baselineGray = baselineImage.CloneAs<L8>();
                using var currentGray = currentImage.CloneAs<L8>();

                var totalPixels = baselineGray.Width * baselineGray.Height;
                var differentPixels = 0;

                // Create diff image
                using var diffImage = new Image<L8>(baselineGray.Width, baselineGray.Height);

                for (int y = 0; y < baselineGray.Height; y++)
                {
                    for (int x = 0; x < baselineGray.Width; x++)
                    {
                        var baselinePixel = baselineGray[x, y];
                        var currentPixel = currentGray[x, y];
                        var diff = Math.Abs(baselinePixel.PackedValue - currentPixel.PackedValue);

                        if (diff > 30) // Threshold for pixel difference
                        {
                            differentPixels++;
                            diffImage[x, y] = new L8(255); // White for differences
                        }
                        else
                        {
                            diffImage[x, y] = new L8(0); // Black for matches
                        }
                    }
                }

                var differencePercentage = (double)differentPixels / totalPixels * 100.0;
                var isMatch = differencePercentage <= VisualThreshold * 100.0;

                // Save diff image if there are differences
                if (!isMatch)
                {
                    await diffImage.SaveAsync(diffPath);
                }

                return new VisualComparisonResult
                {
                    IsMatch = isMatch,
                    DifferencePercentage = differencePercentage,
                    DifferentPixels = differentPixels,
                    TotalPixels = totalPixels
                };
            }
            catch (Exception ex)
            {
                return new VisualComparisonResult
                {
                    IsMatch = false,
                    DifferencePercentage = 100.0,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Wait for visual stability before taking screenshot
        /// </summary>
        protected async Task WaitForVisualStabilityAsync(int timeoutMs = 5000)
        {
            var startTime = DateTime.Now;
            var lastScreenshot = await Page.ScreenshotAsync();
            var stableCount = 0;
            const int requiredStableCount = 3;

            while (DateTime.Now - startTime < TimeSpan.FromMilliseconds(timeoutMs))
            {
                await Task.Delay(500);
                var currentScreenshot = await Page.ScreenshotAsync();

                if (AreScreenshotsEqual(lastScreenshot, currentScreenshot))
                {
                    stableCount++;
                    if (stableCount >= requiredStableCount)
                    {
                        break;
                    }
                }
                else
                {
                    stableCount = 0;
                }

                lastScreenshot = currentScreenshot;
            }
        }

        /// <summary>
        /// Simple screenshot comparison for stability detection
        /// </summary>
        private bool AreScreenshotsEqual(byte[] screenshot1, byte[] screenshot2)
        {
            if (screenshot1.Length != screenshot2.Length)
                return false;

            // Simple hash comparison for performance
            var hash1 = System.Security.Cryptography.SHA256.HashData(screenshot1);
            var hash2 = System.Security.Cryptography.SHA256.HashData(screenshot2);

            return Convert.ToBase64String(hash1) == Convert.ToBase64String(hash2);
        }

        /// <summary>
        /// Take screenshot of specific element
        /// </summary>
        protected async Task<string> TakeElementScreenshotAsync(string elementSelector, string testName)
        {
            var screenshotPath = Path.Combine(CurrentImagesPath, $"{testName}_{elementSelector.Replace(" ", "_")}.png");
            
            var element = Page.Locator(elementSelector);
            await element.ScreenshotAsync(new LocatorScreenshotOptions { Path = screenshotPath });
            
            return screenshotPath;
        }

        /// <summary>
        /// Take screenshot of viewport
        /// </summary>
        protected async Task<string> TakeViewportScreenshotAsync(string testName)
        {
            var screenshotPath = Path.Combine(CurrentImagesPath, $"{testName}_viewport.png");
            
            await Page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath, FullPage = false });
            
            return screenshotPath;
        }

        /// <summary>
        /// Take full page screenshot
        /// </summary>
        protected async Task<string> TakeFullPageScreenshotAsync(string testName)
        {
            var screenshotPath = Path.Combine(CurrentImagesPath, $"{testName}_fullpage.png");
            
            await Page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath, FullPage = true });
            
            return screenshotPath;
        }

        /// <summary>
        /// Update baseline images for all tests
        /// </summary>
        protected void EnableBaselineUpdate()
        {
            UpdateBaselines = true;
        }

        /// <summary>
        /// Set visual comparison threshold
        /// </summary>
        protected void SetVisualThreshold(double threshold)
        {
            VisualThreshold = Math.Max(0.0, Math.Min(1.0, threshold));
        }

        /// <summary>
        /// Get visual test category
        /// </summary>
        protected string GetVisualTestCategory()
        {
            return $"Visual-{BrowserName}";
        }
    }

    /// <summary>
    /// Result of visual comparison
    /// </summary>
    public class VisualComparisonResult
    {
        public string TestName { get; set; } = string.Empty;
        public bool IsMatch { get; set; }
        public double DifferencePercentage { get; set; }
        public int DifferentPixels { get; set; }
        public int TotalPixels { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public bool BaselineCreated { get; set; }
        public string BaselinePath { get; set; } = string.Empty;
        public string CurrentPath { get; set; } = string.Empty;
        public string DiffPath { get; set; } = string.Empty;

        public string GetSummary()
        {
            if (BaselineCreated)
            {
                return $"Baseline created for {TestName}";
            }

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                return $"Visual comparison failed for {TestName}: {ErrorMessage}";
            }

            return $"Visual comparison for {TestName}: {(IsMatch ? "PASS" : "FAIL")} " +
                   $"(Difference: {DifferencePercentage:F2}%, Threshold: {(DifferencePercentage <= 2.0 ? "OK" : "EXCEEDED")})";
        }
    }
} 