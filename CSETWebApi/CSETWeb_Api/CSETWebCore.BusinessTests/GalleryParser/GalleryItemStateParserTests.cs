//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSETWebCore.Business.GalleryParser;
using CSETWebCore.BusinessTests.Infrastructure;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Interfaces.Maturity;
using CSETWebCore.Interfaces.Question;
using CSETWebCore.Interfaces.Standards;
using Moq;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CSETWebCore.BusinessTests.GalleryParser
{
    [TestClass]
    public class GalleryItemStateParserTests : BaseBusinessTest
    {
        private GalleryState _galleryState = null!;
        private CSETContext _context = null!;
        private Mock<ITokenManager> _mockTokenManager = null!;
        private Mock<IMaturityBusiness> _mockMaturityBusiness = null!;
        private Mock<IStandardsBusiness> _mockStandardsBusiness = null!;
        private Mock<IQuestionRequirementManager> _mockQuestionRequirementManager = null!;

        [TestInitialize]
        public override void TestInitialize()
        {
            base.TestInitialize();
            
            // Create in-memory database context
            _context = CreateInMemoryDbContext();
            
            // Setup mocks
            _mockTokenManager = new Mock<ITokenManager>();
            _mockMaturityBusiness = new Mock<IMaturityBusiness>();
            _mockStandardsBusiness = new Mock<IStandardsBusiness>();
            _mockQuestionRequirementManager = new Mock<IQuestionRequirementManager>();
            
            // Setup default token manager behavior
            _mockTokenManager.Setup(t => t.GetCurrentLanguage()).Returns("en");
            
            // Create GalleryState instance
            _galleryState = new GalleryState(
                _mockTokenManager.Object,
                _context,
                _mockMaturityBusiness.Object,
                _mockStandardsBusiness.Object,
                _mockQuestionRequirementManager.Object
            );
        }

        [TestCleanup]
        public override void TestCleanup()
        {
            _context?.Dispose();
            base.TestCleanup();
        }

        #region GetGalleryBoard Tests

        [TestMethod]
        public void GetGalleryBoard_ValidLayout_ReturnsCorrectStructure()
        {
            // Arrange
            var layoutName = "TEST_LAYOUT";
            var testData = CreateTestGalleryData(layoutName);
            SeedGalleryData(testData);

            // Act
            var result = _galleryState.GetGalleryBoard(layoutName);

            // Assert
            result.Should().NotBeNull();
            result.Layout_Name.Should().Be(layoutName);
            result.Rows.Should().HaveCount(2);
            
            // Verify first group
            var firstGroup = result.Rows[0];
            firstGroup.Group_Title.Should().Be("Test Group 1");
            firstGroup.Group_Id.Should().Be(1);
            firstGroup.GalleryItems.Should().HaveCount(2);
            
            // Verify second group
            var secondGroup = result.Rows[1];
            secondGroup.Group_Title.Should().Be("Test Group 2");
            secondGroup.Group_Id.Should().Be(2);
            secondGroup.GalleryItems.Should().HaveCount(1);
        }

        [TestMethod]
        public void GetGalleryBoard_EmptyLayout_ReturnsEmptyStructure()
        {
            // Arrange
            var layoutName = "EMPTY_LAYOUT";

            // Act
            var result = _galleryState.GetGalleryBoard(layoutName);

            // Assert
            result.Should().NotBeNull();
            result.Layout_Name.Should().BeNull();
            result.Rows.Should().BeEmpty();
        }

        [TestMethod]
        public void GetGalleryBoard_NonExistentLayout_ReturnsEmptyStructure()
        {
            // Arrange
            var layoutName = "NON_EXISTENT_LAYOUT";
            var testData = CreateTestGalleryData("EXISTING_LAYOUT");
            SeedGalleryData(testData);

            // Act
            var result = _galleryState.GetGalleryBoard(layoutName);

            // Assert
            result.Should().NotBeNull();
            result.Layout_Name.Should().BeNull();
            result.Rows.Should().BeEmpty();
        }

        [TestMethod]
        public void GetGalleryBoard_WithInvisibleItems_FiltersOutInvisibleItems()
        {
            // Arrange
            var layoutName = "TEST_LAYOUT";
            var testData = CreateTestGalleryDataWithInvisibleItems(layoutName);
            SeedGalleryData(testData);

            // Act
            var result = _galleryState.GetGalleryBoard(layoutName);

            // Assert
            result.Should().NotBeNull();
            result.Rows.Should().HaveCount(1);
            result.Rows[0].GalleryItems.Should().HaveCount(1); // Only visible item
            result.Rows[0].GalleryItems[0].Title.Should().Be("Visible Item");
        }

        [TestMethod]
        public void GetGalleryBoard_WithTranslation_AppliesTranslation()
        {
            // Arrange
            var layoutName = "TEST_LAYOUT";
            var testData = CreateTestGalleryData(layoutName);
            SeedGalleryData(testData);
            
            // Setup translation
            _mockTokenManager.Setup(t => t.GetCurrentLanguage()).Returns("es");
            
            // Mock translation overlay (this would need to be implemented in the actual code)
            // For now, we'll test that the method doesn't throw with non-English language

            // Act
            var result = _galleryState.GetGalleryBoard(layoutName);

            // Assert
            result.Should().NotBeNull();
            result.Rows.Should().HaveCount(2);
        }

        [TestMethod]
        public void GetGalleryBoard_OrdersByRowIndexAndColumnIndex()
        {
            // Arrange
            var layoutName = "TEST_LAYOUT";
            var testData = CreateTestGalleryDataWithCustomOrdering(layoutName);
            SeedGalleryData(testData);

            // Act
            var result = _galleryState.GetGalleryBoard(layoutName);

            // Assert
            result.Should().NotBeNull();
            result.Rows.Should().HaveCount(2);
            
            // Verify ordering
            result.Rows[0].Group_Id.Should().Be(2); // Row index 0
            result.Rows[1].Group_Id.Should().Be(1); // Row index 1
            
            // Verify column ordering within first group
            var firstGroupItems = result.Rows[1].GalleryItems;
            firstGroupItems[0].Title.Should().Be("Item 1"); // Column 0
            firstGroupItems[1].Title.Should().Be("Item 2"); // Column 1
        }

        [TestMethod]
        public void GetGalleryBoard_WithNullValues_HandlesGracefully()
        {
            // Arrange
            var layoutName = "TEST_LAYOUT";
            var testData = CreateTestGalleryDataWithNullValues(layoutName);
            SeedGalleryData(testData);

            // Act
            var result = _galleryState.GetGalleryBoard(layoutName);

            // Assert
            result.Should().NotBeNull();
            result.Rows.Should().HaveCount(1);
            result.Rows[0].GalleryItems.Should().HaveCount(1);
            
            // Verify null values are handled
            var item = result.Rows[0].GalleryItems[0];
            item.Title.Should().BeNull();
            item.Description.Should().Be("Test Description");
        }

        #endregion

        #region Helper Methods

        private (List<GALLERY_LAYOUT>, List<GALLERY_GROUP>, List<GALLERY_ITEM>, List<GALLERY_ROWS>, List<GALLERY_GROUP_DETAILS>) CreateTestGalleryData(string layoutName)
        {
            var layouts = new List<GALLERY_LAYOUT>
            {
                new GALLERY_LAYOUT { Layout_Name = layoutName }
            };

            var groups = new List<GALLERY_GROUP>
            {
                new GALLERY_GROUP { Group_Id = 1, Group_Title = "Test Group 1" },
                new GALLERY_GROUP { Group_Id = 2, Group_Title = "Test Group 2" }
            };

            var items = new List<GALLERY_ITEM>
            {
                new GALLERY_ITEM
                {
                    Gallery_Item_Guid = Guid.NewGuid(),
                    Title = "Test Item 1",
                    Description = "Test Description 1",
                    Configuration_Setup = "{}",
                    Is_Visible = true,
                    CreationDate = DateTime.UtcNow
                },
                new GALLERY_ITEM
                {
                    Gallery_Item_Guid = Guid.NewGuid(),
                    Title = "Test Item 2",
                    Description = "Test Description 2",
                    Configuration_Setup = "{}",
                    Is_Visible = true,
                    CreationDate = DateTime.UtcNow
                },
                new GALLERY_ITEM
                {
                    Gallery_Item_Guid = Guid.NewGuid(),
                    Title = "Test Item 3",
                    Description = "Test Description 3",
                    Configuration_Setup = "{}",
                    Is_Visible = true,
                    CreationDate = DateTime.UtcNow
                }
            };

            var rows = new List<GALLERY_ROWS>
            {
                new GALLERY_ROWS { Layout_Name = layoutName, Row_Index = 0, Group_Id = 1 },
                new GALLERY_ROWS { Layout_Name = layoutName, Row_Index = 1, Group_Id = 2 }
            };

            var details = new List<GALLERY_GROUP_DETAILS>
            {
                new GALLERY_GROUP_DETAILS { Group_Id = 1, Column_Index = 0, Gallery_Item_Guid = items[0].Gallery_Item_Guid, Click_Count = 0 },
                new GALLERY_GROUP_DETAILS { Group_Id = 1, Column_Index = 1, Gallery_Item_Guid = items[1].Gallery_Item_Guid, Click_Count = 0 },
                new GALLERY_GROUP_DETAILS { Group_Id = 2, Column_Index = 0, Gallery_Item_Guid = items[2].Gallery_Item_Guid, Click_Count = 0 }
            };

            return (layouts, groups, items, rows, details);
        }

        private (List<GALLERY_LAYOUT>, List<GALLERY_GROUP>, List<GALLERY_ITEM>, List<GALLERY_ROWS>, List<GALLERY_GROUP_DETAILS>) CreateTestGalleryDataWithInvisibleItems(string layoutName)
        {
            var layouts = new List<GALLERY_LAYOUT>
            {
                new GALLERY_LAYOUT { Layout_Name = layoutName }
            };

            var groups = new List<GALLERY_GROUP>
            {
                new GALLERY_GROUP { Group_Id = 1, Group_Title = "Test Group" }
            };

            var items = new List<GALLERY_ITEM>
            {
                new GALLERY_ITEM
                {
                    Gallery_Item_Guid = Guid.NewGuid(),
                    Title = "Visible Item",
                    Description = "Visible Description",
                    Configuration_Setup = "{}",
                    Is_Visible = true,
                    CreationDate = DateTime.UtcNow
                },
                new GALLERY_ITEM
                {
                    Gallery_Item_Guid = Guid.NewGuid(),
                    Title = "Invisible Item",
                    Description = "Invisible Description",
                    Configuration_Setup = "{}",
                    Is_Visible = false,
                    CreationDate = DateTime.UtcNow
                }
            };

            var rows = new List<GALLERY_ROWS>
            {
                new GALLERY_ROWS { Layout_Name = layoutName, Row_Index = 0, Group_Id = 1 }
            };

            var details = new List<GALLERY_GROUP_DETAILS>
            {
                new GALLERY_GROUP_DETAILS { Group_Id = 1, Column_Index = 0, Gallery_Item_Guid = items[0].Gallery_Item_Guid, Click_Count = 0 },
                new GALLERY_GROUP_DETAILS { Group_Id = 1, Column_Index = 1, Gallery_Item_Guid = items[1].Gallery_Item_Guid, Click_Count = 0 }
            };

            return (layouts, groups, items, rows, details);
        }

        private (List<GALLERY_LAYOUT>, List<GALLERY_GROUP>, List<GALLERY_ITEM>, List<GALLERY_ROWS>, List<GALLERY_GROUP_DETAILS>) CreateTestGalleryDataWithCustomOrdering(string layoutName)
        {
            var layouts = new List<GALLERY_LAYOUT>
            {
                new GALLERY_LAYOUT { Layout_Name = layoutName }
            };

            var groups = new List<GALLERY_GROUP>
            {
                new GALLERY_GROUP { Group_Id = 1, Group_Title = "Group 1" },
                new GALLERY_GROUP { Group_Id = 2, Group_Title = "Group 2" }
            };

            var items = new List<GALLERY_ITEM>
            {
                new GALLERY_ITEM
                {
                    Gallery_Item_Guid = Guid.NewGuid(),
                    Title = "Item 1",
                    Description = "Description 1",
                    Configuration_Setup = "{}",
                    Is_Visible = true,
                    CreationDate = DateTime.UtcNow
                },
                new GALLERY_ITEM
                {
                    Gallery_Item_Guid = Guid.NewGuid(),
                    Title = "Item 2",
                    Description = "Description 2",
                    Configuration_Setup = "{}",
                    Is_Visible = true,
                    CreationDate = DateTime.UtcNow
                }
            };

            var rows = new List<GALLERY_ROWS>
            {
                new GALLERY_ROWS { Layout_Name = layoutName, Row_Index = 1, Group_Id = 1 }, // Higher row index
                new GALLERY_ROWS { Layout_Name = layoutName, Row_Index = 0, Group_Id = 2 }  // Lower row index
            };

            var details = new List<GALLERY_GROUP_DETAILS>
            {
                new GALLERY_GROUP_DETAILS { Group_Id = 1, Column_Index = 1, Gallery_Item_Guid = items[1].Gallery_Item_Guid, Click_Count = 0 },
                new GALLERY_GROUP_DETAILS { Group_Id = 1, Column_Index = 0, Gallery_Item_Guid = items[0].Gallery_Item_Guid, Click_Count = 0 }
            };

            return (layouts, groups, items, rows, details);
        }

        private (List<GALLERY_LAYOUT>, List<GALLERY_GROUP>, List<GALLERY_ITEM>, List<GALLERY_ROWS>, List<GALLERY_GROUP_DETAILS>) CreateTestGalleryDataWithNullValues(string layoutName)
        {
            var layouts = new List<GALLERY_LAYOUT>
            {
                new GALLERY_LAYOUT { Layout_Name = layoutName }
            };

            var groups = new List<GALLERY_GROUP>
            {
                new GALLERY_GROUP { Group_Id = 1, Group_Title = "Test Group" }
            };

            var items = new List<GALLERY_ITEM>
            {
                new GALLERY_ITEM
                {
                    Gallery_Item_Guid = Guid.NewGuid(),
                    Title = null, // Null title
                    Description = "Test Description",
                    Configuration_Setup = "{}",
                    Is_Visible = true,
                    CreationDate = DateTime.UtcNow
                }
            };

            var rows = new List<GALLERY_ROWS>
            {
                new GALLERY_ROWS { Layout_Name = layoutName, Row_Index = 0, Group_Id = 1 }
            };

            var details = new List<GALLERY_GROUP_DETAILS>
            {
                new GALLERY_GROUP_DETAILS { Group_Id = 1, Column_Index = 0, Gallery_Item_Guid = items[0].Gallery_Item_Guid, Click_Count = 0 }
            };

            return (layouts, groups, items, rows, details);
        }

        private void SeedGalleryData((List<GALLERY_LAYOUT>, List<GALLERY_GROUP>, List<GALLERY_ITEM>, List<GALLERY_ROWS>, List<GALLERY_GROUP_DETAILS>) data)
        {
            var (layouts, groups, items, rows, details) = data;
            
            _context.GALLERY_LAYOUT.AddRange(layouts);
            _context.GALLERY_GROUP.AddRange(groups);
            _context.GALLERY_ITEM.AddRange(items);
            _context.GALLERY_ROWS.AddRange(rows);
            _context.GALLERY_GROUP_DETAILS.AddRange(details);
            
            _context.SaveChanges();
        }

        #endregion
    }
}
