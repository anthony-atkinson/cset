//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.Helpers;
using FluentAssertions;
using Bogus;

namespace CSETWebCore.BusinessTests.Infrastructure
{
    /// <summary>
    /// Base class for business logic unit tests providing common setup and utilities
    /// </summary>
    [TestClass]
    public abstract class BaseBusinessTest
    {
        protected IFixture Fixture { get; private set; } = null!;
        protected Mock<CSETContext> MockDbContext { get; private set; } = null!;
        protected Mock<IConfiguration> MockConfiguration { get; private set; } = null!;
        protected Mock<ILogger> MockLogger { get; private set; } = null!;
        protected Faker Faker { get; private set; } = null!;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            // Setup AutoFixture with AutoMoq
            Fixture = new Fixture().Customize(new AutoMoqCustomization());
            
            // Setup Bogus faker for realistic test data
            Faker = new Faker();

            // Setup common mocks
            MockDbContext = new Mock<CSETContext>();
            MockConfiguration = new Mock<IConfiguration>();
            MockLogger = new Mock<ILogger>();

            // Configure common mock behaviors
            SetupMockDefaults();

            // Allow derived classes to perform additional setup
            SetupTest();
        }

        [TestCleanup]
        public virtual void TestCleanup()
        {
            // Override in derived classes if needed
        }

        /// <summary>
        /// Override this method in derived classes for additional test setup
        /// </summary>
        protected virtual void SetupTest()
        {
            // Default implementation - override in derived classes
        }

        /// <summary>
        /// Setup default mock behaviors
        /// </summary>
        private void SetupMockDefaults()
        {
            // Setup configuration defaults
            MockConfiguration.Setup(c => c["ConnectionStrings:DefaultConnection"])
                .Returns("Data Source=:memory:");

            // Setup logger to not throw on calls
            MockLogger.Setup(l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
        }

        /// <summary>
        /// Creates an in-memory database context for testing
        /// </summary>
        protected CSETContext CreateInMemoryDbContext(string? databaseName = null)
        {
            var options = new DbContextOptionsBuilder<CSETContext>()
                .UseInMemoryDatabase(databaseName ?? $"TestDb_{Guid.NewGuid()}")
                .Options;

            return new CSETContext(options);
        }

        /// <summary>
        /// Helper method to create a valid test assessment
        /// </summary>
        protected ASSESSMENTS CreateTestAssessment(string? name = null)
        {
            return new ASSESSMENTS
            {
                Assessment_Id = Faker.Random.Int(1, 10000),
                Assessment_Name = name ?? Faker.Lorem.Sentence(3),
                Assessment_Date = Faker.Date.Recent(),
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                AssessmentCreatedDate = DateTime.UtcNow,
                LastAccessedDate = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Helper method to create a test user
        /// </summary>
        protected USERS CreateTestUser(string? email = null)
        {
            return new USERS
            {
                UserId = Faker.Random.Int(1, 10000),
                PrimaryEmail = email ?? Faker.Internet.Email(),
                FirstName = Faker.Name.FirstName(),
                LastName = Faker.Name.LastName(),
                CreatedDate = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Helper method to setup a mock DbSet for Entity Framework
        /// </summary>
        protected Mock<DbSet<T>> CreateMockDbSet<T>(IEnumerable<T> data) where T : class
        {
            var queryableData = data.AsQueryable();
            var mockDbSet = new Mock<DbSet<T>>();

            mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryableData.Provider);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryableData.Expression);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryableData.GetEnumerator());

            return mockDbSet;
        }

        /// <summary>
        /// Asserts that an action throws a specific exception with a specific message
        /// </summary>
        protected void AssertThrows<TException>(Action action, string expectedMessage = "") 
            where TException : Exception
        {
            action.Should().Throw<TException>()
                .Which.Message.Should().Contain(expectedMessage);
        }

        /// <summary>
        /// Asserts that an async action throws a specific exception with a specific message
        /// </summary>
        protected async Task AssertThrowsAsync<TException>(Func<Task> action, string expectedMessage = "") 
            where TException : Exception
        {
            var exception = await action.Should().ThrowAsync<TException>();
            if (!string.IsNullOrEmpty(expectedMessage))
            {
                exception.Which.Message.Should().Contain(expectedMessage);
            }
        }
    }
} 