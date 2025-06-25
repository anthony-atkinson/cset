using Microsoft.Playwright;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace CSETWebCore.PlaywrightTests.Tests.Security
{
    [TestFixture]
    public class EnhancedSecurityFeaturesTests : PlaywrightTestBase
    {
        [Test]
        [Description("Verify enhanced authentication and session management")]
        public async Task EnhancedAuthentication_Should_WorkCorrectly()
        {
            // Navigate to login page
            await Page.GotoAsync($"{BaseUrl}/login");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify security headers are present
            await Expect(Page.Locator("meta[http-equiv='Content-Security-Policy']")).ToBeVisibleAsync();
            await Expect(Page.Locator("meta[http-equiv='X-Frame-Options']")).ToBeVisibleAsync();

            // Test login with valid credentials
            await Page.FillAsync("#username", "testuser");
            await Page.FillAsync("#password", "TestPassword123!");
            await Page.ClickAsync("#login-btn");

            // Verify successful login
            await Expect(Page).ToHaveURLAsync(new Regex(".*/dashboard"));

            // Verify session token is set
            await Expect(Page.Locator("meta[name='session-token']")).ToBeVisibleAsync();

            // Verify session timeout warning
            await Page.WaitForTimeoutAsync(30000); // Wait for session timeout warning
            await Expect(Page.Locator(".session-timeout-warning")).ToBeVisibleAsync();

            // Extend session
            await Page.ClickAsync("#extend-session-btn");
            await Expect(Page.Locator(".session-timeout-warning")).Not.ToBeVisibleAsync();
        }

        [Test]
        [Description("Verify rate limiting and brute force protection")]
        public async Task RateLimiting_Should_ProtectAgainstBruteForce()
        {
            // Navigate to login page
            await Page.GotoAsync($"{BaseUrl}/login");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Attempt multiple failed logins
            for (int i = 0; i < 5; i++)
            {
                await Page.FillAsync("#username", "testuser");
                await Page.FillAsync("#password", "WrongPassword123!");
                await Page.ClickAsync("#login-btn");

                // Wait for error message
                await Expect(Page.Locator(".login-error")).ToBeVisibleAsync();
                await Page.WaitForTimeoutAsync(1000);
            }

            // Verify rate limiting is triggered
            await Expect(Page.Locator(".rate-limit-warning")).ToBeVisibleAsync();
            await Expect(Page.Locator(".rate-limit-warning")).ToContainTextAsync("Too many failed attempts");

            // Verify login is blocked
            await Page.FillAsync("#username", "testuser");
            await Page.FillAsync("#password", "TestPassword123!");
            await Page.ClickAsync("#login-btn");

            // Verify login still blocked
            await Expect(Page).ToHaveURLAsync(new Regex(".*/login"));

            // Wait for rate limit to expire
            await Page.WaitForTimeoutAsync(30000);

            // Try login again
            await Page.FillAsync("#username", "testuser");
            await Page.FillAsync("#password", "TestPassword123!");
            await Page.ClickAsync("#login-btn");

            // Verify login succeeds after rate limit expires
            await Expect(Page).ToHaveURLAsync(new Regex(".*/dashboard"));
        }

        [Test]
        [Description("Verify input validation and XSS protection")]
        public async Task InputValidation_Should_ProtectAgainstXSS()
        {
            // Navigate to assessment creation
            await Page.GotoAsync($"{BaseUrl}/assessment");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Login first
            await Page.GotoAsync($"{BaseUrl}/login");
            await Page.FillAsync("#username", "testuser");
            await Page.FillAsync("#password", "TestPassword123!");
            await Page.ClickAsync("#login-btn");
            await Page.WaitForURLAsync("**/dashboard");

            // Navigate to assessment creation
            await Page.GotoAsync($"{BaseUrl}/assessment");
            await Page.ClickAsync("#create-assessment-btn");

            // Test XSS injection in assessment name
            const string xssPayload = "<script>alert('XSS')</script>";
            await Page.FillAsync("#assessment-name", xssPayload);

            // Submit form
            await Page.ClickAsync("#save-assessment-btn");

            // Verify XSS is sanitized
            await Expect(Page.Locator("#assessment-name")).ToHaveValueAsync("&lt;script&gt;alert('XSS')&lt;/script&gt;");

            // Test SQL injection
            const string sqlPayload = "'; DROP TABLE assessments; --";
            await Page.FillAsync("#assessment-description", sqlPayload);

            // Submit form
            await Page.ClickAsync("#save-assessment-btn");

            // Verify SQL injection is prevented
            await Expect(Page.Locator(".validation-error")).ToBeVisibleAsync();
            await Expect(Page.Locator(".validation-error")).ToContainTextAsync("Invalid characters detected");
        }

        [Test]
        [Description("Verify file upload security and validation")]
        public async Task FileUploadSecurity_Should_ValidateFiles()
        {
            // Navigate to file upload page
            await Page.GotoAsync($"{BaseUrl}/assessment/123/documents");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Login first
            await Page.GotoAsync($"{BaseUrl}/login");
            await Page.FillAsync("#username", "testuser");
            await Page.FillAsync("#password", "TestPassword123!");
            await Page.ClickAsync("#login-btn");
            await Page.WaitForURLAsync("**/dashboard");

            await Page.GotoAsync($"{BaseUrl}/assessment/123/documents");

            // Test uploading malicious file
            await Page.SetInputFilesAsync("#file-upload", new FilePayload
            {
                Name = "malicious.exe",
                MimeType = "application/x-msdownload",
                Buffer = System.Text.Encoding.UTF8.GetBytes("malicious content")
            });

            // Verify file type validation
            await Expect(Page.Locator(".file-validation-error")).ToBeVisibleAsync();
            await Expect(Page.Locator(".file-validation-error")).ToContainTextAsync("File type not allowed");

            // Test uploading oversized file
            await Page.SetInputFilesAsync("#file-upload", new FilePayload
            {
                Name = "large-file.pdf",
                MimeType = "application/pdf",
                Buffer = new byte[11 * 1024 * 1024] // 11MB file
            });

            // Verify file size validation
            await Expect(Page.Locator(".file-size-error")).ToBeVisibleAsync();
            await Expect(Page.Locator(".file-size-error")).ToContainTextAsync("File size exceeds limit");

            // Test uploading valid file
            await Page.SetInputFilesAsync("#file-upload", new FilePayload
            {
                Name = "valid-document.pdf",
                MimeType = "application/pdf",
                Buffer = System.Text.Encoding.UTF8.GetBytes("valid pdf content")
            });

            // Verify file is accepted
            await Expect(Page.Locator(".file-upload-success")).ToBeVisibleAsync();
        }

        [Test]
        [Description("Verify CSRF protection")]
        public async Task CSRFProtection_Should_WorkCorrectly()
        {
            // Navigate to assessment page
            await Page.GotoAsync($"{BaseUrl}/assessment");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Login first
            await Page.GotoAsync($"{BaseUrl}/login");
            await Page.FillAsync("#username", "testuser");
            await Page.FillAsync("#password", "TestPassword123!");
            await Page.ClickAsync("#login-btn");
            await Page.WaitForURLAsync("**/dashboard");

            await Page.GotoAsync($"{BaseUrl}/assessment");

            // Verify CSRF token is present
            await Expect(Page.Locator("input[name='__RequestVerificationToken']")).ToBeVisibleAsync();

            // Get CSRF token
            var csrfToken = await Page.Locator("input[name='__RequestVerificationToken']").GetAttributeAsync("value");

            // Test form submission without CSRF token
            await Page.EvaluateAsync(@"
                document.querySelector('input[name=""__RequestVerificationToken""]').remove();
            ");

            await Page.ClickAsync("#create-assessment-btn");
            await Page.FillAsync("#assessment-name", "CSRF Test");
            await Page.ClickAsync("#save-assessment-btn");

            // Verify CSRF validation error
            await Expect(Page.Locator(".csrf-error")).ToBeVisibleAsync();
            await Expect(Page.Locator(".csrf-error")).ToContainTextAsync("Invalid security token");
        }

        [Test]
        [Description("Verify audit logging and security events")]
        public async Task AuditLogging_Should_TrackSecurityEvents()
        {
            // Navigate to login page
            await Page.GotoAsync($"{BaseUrl}/login");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Login with valid credentials
            await Page.FillAsync("#username", "testuser");
            await Page.FillAsync("#password", "TestPassword123!");
            await Page.ClickAsync("#login-btn");

            // Verify successful login
            await Expect(Page).ToHaveURLAsync(new Regex(".*/dashboard"));

            // Navigate to audit log page
            await Page.GotoAsync($"{BaseUrl}/admin/audit-logs");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify login event is logged
            await Expect(Page.Locator(".audit-log-entry")).ToContainTextAsync("User login successful");
            await Expect(Page.Locator(".audit-log-entry")).ToContainTextAsync("testuser");

            // Perform security-sensitive action
            await Page.GotoAsync($"{BaseUrl}/assessment");
            await Page.ClickAsync("#create-assessment-btn");
            await Page.FillAsync("#assessment-name", "Audit Test Assessment");
            await Page.ClickAsync("#save-assessment-btn");

            // Navigate back to audit logs
            await Page.GotoAsync($"{BaseUrl}/admin/audit-logs");

            // Verify assessment creation is logged
            await Expect(Page.Locator(".audit-log-entry")).ToContainTextAsync("Assessment created");
            await Expect(Page.Locator(".audit-log-entry")).ToContainTextAsync("testuser");
        }

        [Test]
        [Description("Verify role-based access control")]
        public async Task RoleBasedAccess_Should_EnforcePermissions()
        {
            // Login as regular user
            await Page.GotoAsync($"{BaseUrl}/login");
            await Page.FillAsync("#username", "regularuser");
            await Page.FillAsync("#password", "TestPassword123!");
            await Page.ClickAsync("#login-btn");

            // Verify successful login
            await Expect(Page).ToHaveURLAsync(new Regex(".*/dashboard"));

            // Try to access admin page
            await Page.GotoAsync($"{BaseUrl}/admin/users");

            // Verify access denied
            await Expect(Page.Locator(".access-denied")).ToBeVisibleAsync();
            await Expect(Page.Locator(".access-denied")).ToContainTextAsync("Access denied");

            // Try to access user management
            await Page.GotoAsync($"{BaseUrl}/admin/user-management");

            // Verify access denied
            await Expect(Page.Locator(".access-denied")).ToBeVisibleAsync();

            // Login as admin user
            await Page.GotoAsync($"{BaseUrl}/login");
            await Page.FillAsync("#username", "adminuser");
            await Page.FillAsync("#password", "AdminPassword123!");
            await Page.ClickAsync("#login-btn");

            // Try to access admin page again
            await Page.GotoAsync($"{BaseUrl}/admin/users");

            // Verify access granted
            await Expect(Page).ToHaveURLAsync(new Regex(".*/admin/users"));
            await Expect(Page.Locator(".admin-panel")).ToBeVisibleAsync();
        }

        [Test]
        [Description("Verify data encryption and secure transmission")]
        public async Task DataEncryption_Should_ProtectSensitiveData()
        {
            // Navigate to assessment page
            await Page.GotoAsync($"{BaseUrl}/assessment");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Login first
            await Page.GotoAsync($"{BaseUrl}/login");
            await Page.FillAsync("#username", "testuser");
            await Page.FillAsync("#password", "TestPassword123!");
            await Page.ClickAsync("#login-btn");
            await Page.WaitForURLAsync("**/dashboard");

            await Page.GotoAsync($"{BaseUrl}/assessment");

            // Verify HTTPS is enforced
            await Expect(Page).ToHaveURLAsync(new Regex("^https://"));

            // Create assessment with sensitive data
            await Page.ClickAsync("#create-assessment-btn");
            await Page.FillAsync("#assessment-name", "Encryption Test");
            await Page.FillAsync("#assessment-description", "Contains sensitive information");

            // Submit form
            await Page.ClickAsync("#save-assessment-btn");

            // Verify data is transmitted securely
            await Expect(Page.Locator(".encryption-indicator")).ToBeVisibleAsync();
            await Expect(Page.Locator(".encryption-indicator")).ToContainTextAsync("Data encrypted");

            // Navigate to assessment details
            await Page.ClickAsync(".assessment-item:first-child");

            // Verify sensitive data is masked in UI
            await Expect(Page.Locator(".sensitive-data")).ToHaveClassAsync(new Regex("masked"));
        }

        [Test]
        [Description("Verify session hijacking protection")]
        public async Task SessionProtection_Should_PreventHijacking()
        {
            // Login from first browser context
            await Page.GotoAsync($"{BaseUrl}/login");
            await Page.FillAsync("#username", "testuser");
            await Page.FillAsync("#password", "TestPassword123!");
            await Page.ClickAsync("#login-btn");

            // Verify successful login
            await Expect(Page).ToHaveURLAsync(new Regex(".*/dashboard"));

            // Get session token
            var sessionToken = await Page.Locator("meta[name='session-token']").GetAttributeAsync("content");

            // Create new browser context
            var newContext = await Browser.NewContextAsync();
            var newPage = await newContext.NewPageAsync();

            // Try to use session token in new context
            await newPage.AddInitScriptAsync($@"
                localStorage.setItem('sessionToken', '{sessionToken}');
            ");

            await newPage.GotoAsync($"{BaseUrl}/dashboard");

            // Verify session is invalid in new context
            await Expect(newPage.Locator(".session-invalid")).ToBeVisibleAsync();
            await Expect(newPage).ToHaveURLAsync(new Regex(".*/login"));

            // Clean up
            await newContext.CloseAsync();
        }

        [Test]
        [Description("Verify security headers and content security policy")]
        public async Task SecurityHeaders_Should_BeProperlyConfigured()
        {
            // Navigate to main page
            await Page.GotoAsync($"{BaseUrl}/");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify security headers are present
            var response = await Page.WaitForResponseAsync("**/*");
            var headers = response.Headers;

            // Verify Content Security Policy
            Assert.That(headers.ContainsKey("content-security-policy"), "CSP header should be present");

            // Verify X-Frame-Options
            Assert.That(headers.ContainsKey("x-frame-options"), "X-Frame-Options header should be present");
            Assert.That(headers["x-frame-options"], Is.EqualTo("DENY"), "X-Frame-Options should be DENY");

            // Verify X-Content-Type-Options
            Assert.That(headers.ContainsKey("x-content-type-options"), "X-Content-Type-Options header should be present");
            Assert.That(headers["x-content-type-options"], Is.EqualTo("nosniff"), "X-Content-Type-Options should be nosniff");

            // Verify X-XSS-Protection
            Assert.That(headers.ContainsKey("x-xss-protection"), "X-XSS-Protection header should be present");
            Assert.That(headers["x-xss-protection"], Is.EqualTo("1; mode=block"), "X-XSS-Protection should be 1; mode=block");

            // Verify Referrer Policy
            Assert.That(headers.ContainsKey("referrer-policy"), "Referrer-Policy header should be present");
        }

        [Test]
        [Description("Verify secure logout and session cleanup")]
        public async Task SecureLogout_Should_CleanupSession()
        {
            // Login first
            await Page.GotoAsync($"{BaseUrl}/login");
            await Page.FillAsync("#username", "testuser");
            await Page.FillAsync("#password", "TestPassword123!");
            await Page.ClickAsync("#login-btn");

            // Verify successful login
            await Expect(Page).ToHaveURLAsync(new Regex(".*/dashboard"));

            // Get session token before logout
            var sessionToken = await Page.Locator("meta[name='session-token']").GetAttributeAsync("content");

            // Perform logout
            await Page.ClickAsync("#logout-btn");

            // Verify logout confirmation
            await Expect(Page.Locator(".logout-confirmation")).ToBeVisibleAsync();
            await Page.ClickAsync("#confirm-logout-btn");

            // Verify redirected to login page
            await Expect(Page).ToHaveURLAsync(new Regex(".*/login"));

            // Verify session token is cleared
            await Expect(Page.Locator("meta[name='session-token']")).Not.ToBeVisibleAsync();

            // Try to access protected page
            await Page.GotoAsync($"{BaseUrl}/dashboard");

            // Verify access denied
            await Expect(Page).ToHaveURLAsync(new Regex(".*/login"));
            await Expect(Page.Locator(".access-denied-message")).ToBeVisibleAsync();
        }
    }
} 