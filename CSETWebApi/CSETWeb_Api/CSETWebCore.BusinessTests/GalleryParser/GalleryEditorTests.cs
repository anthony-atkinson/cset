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
    public class GalleryEditorTests : BaseBusinessTest
    {
        private GalleryEditor _galleryEditor = null!;
        private CSETContext _context = null!;
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
            _mockMaturityBusiness = new Mock<IMaturityBusiness>();
            _mockStandardsBusiness = new Mock<IStandardsBusiness>();
            _mockQuestionRequirementManager = new Mock<IQuestionRequirementManager>();
            
            // Create GalleryEditor instance
            _galleryEditor = new GalleryEditor(
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

        #region GetLayout Tests

        [TestMethod]
        public void GetLayout_WithExistingLayouts_ReturnsAllLayouts()
        {
            // Arrange
            var layouts = new List<GALLERY_LAYOUT>
            {
                new GALLERY_LAYOUT { Layout_Name = "LAYOUT_1" },
                new GALLERY_LAYOUT { Layout_Name = "LAYOUT_2" },
                new GALLERY_LAYOUT { Layout_Name = "LAYOUT_3" }
            };
            _context.GALLERY_LAYOUT.AddRange(layouts);
            _context.SaveChanges();

            // Act
            var result = _galleryEditor.GetLayout();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().Contain("LAYOUT_1");
            result.Should().Contain("LAYOUT_2");
            result.Should().Contain("LAYOUT_3");
        }

        [TestMethod]
        public void GetLayout_EmptyDatabase_ReturnsEmptyList()
        {
            // Act
            var result = _galleryEditor.GetLayout();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region AddGalleryItem Tests

        [TestMethod]
        public void AddGalleryItem_ValidData_CreatesItemAndDetails()
        {
            // Arrange
            var groupId = 1;
            var columnId = 0;
            var title = "Test Item";
            var description = "Test Description";
            var configSetup = "{}";
            var iconSmall = "icon_small.png";
            var iconLarge = "icon_large.png";

            // Create group first
            var group = new GALLERY_GROUP { Group_Id = groupId, Group_Title = "Test Group" };
            _context.GALLERY_GROUP.Add(group);
            _context.SaveChanges();

            // Act
            _galleryEditor.AddGalleryItem(iconSmall, iconLarge, description, title, configSetup, groupId, columnId);

            // Assert
            var createdItem = _context.GALLERY_ITEM.FirstOrDefault();
            createdItem.Should().NotBeNull();
            createdItem!.Title.Should().Be(title);
            createdItem.Description.Should().Be(description);
            createdItem.Configuration_Setup.Should().Be(configSetup);
            createdItem.Icon_File_Name_Small.Should().Be(iconSmall);
            createdItem.Icon_File_Name_Large.Should().Be(iconLarge);
            createdItem.Is_Visible.Should().BeTrue();
            createdItem.CreationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            var createdDetail = _context.GALLERY_GROUP_DETAILS.FirstOrDefault();
            createdDetail.Should().NotBeNull();
            createdDetail!.Group_Id.Should().Be(groupId);
            createdDetail.Column_Index.Should().Be(columnId);
            createdDetail.Gallery_Item_Guid.Should().Be(createdItem.Gallery_Item_Guid);
            createdDetail.Click_Count.Should().Be(0);
        }

        [TestMethod]
        public void AddGalleryItem_WithNullValues_HandlesGracefully()
        {
            // Arrange
            var groupId = 1;
            var columnId = 0;
            var group = new GALLERY_GROUP { Group_Id = groupId, Group_Title = "Test Group" };
            _context.GALLERY_GROUP.Add(group);
            _context.SaveChanges();

            // Act
            _galleryEditor.AddGalleryItem(null, null, null, null, null, groupId, columnId);

            // Assert
            var createdItem = _context.GALLERY_ITEM.FirstOrDefault();
            createdItem.Should().NotBeNull();
            createdItem!.Title.Should().BeNull();
            createdItem.Description.Should().BeNull();
            createdItem.Configuration_Setup.Should().BeNull();
            createdItem.Icon_File_Name_Small.Should().BeNull();
            createdItem.Icon_File_Name_Large.Should().BeNull();
        }

        #endregion

        #region AddGalleryGroup Tests

        [TestMethod]
        public void AddGalleryGroup_ValidData_CreatesGroupAndRow()
        {
            // Arrange
            var groupTitle = "Test Group";
            var layoutName = "TEST_LAYOUT";

            // Create layout first
            var layout = new GALLERY_LAYOUT { Layout_Name = layoutName };
            _context.GALLERY_LAYOUT.Add(layout);
            _context.SaveChanges();

            // Act
            var result = _galleryEditor.AddGalleryGroup(groupTitle, layoutName);

            // Assert
            result.Should().BeGreaterThan(0);
            
            var createdGroup = _context.GALLERY_GROUP.FirstOrDefault(g => g.Group_Id == result);
            createdGroup.Should().NotBeNull();
            createdGroup!.Group_Title.Should().Be(groupTitle);

            var createdRow = _context.GALLERY_ROWS.FirstOrDefault(r => r.Group_Id == result);
            createdRow.Should().NotBeNull();
            createdRow!.Layout_Name.Should().Be(layoutName);
            createdRow.Row_Index.Should().Be(0); // First row
        }

        [TestMethod]
        public void AddGalleryGroup_WithExistingRows_SetsCorrectRowIndex()
        {
            // Arrange
            var layoutName = "TEST_LAYOUT";
            var layout = new GALLERY_LAYOUT { Layout_Name = layoutName };
            _context.GALLERY_LAYOUT.Add(layout);
            
            // Add existing rows
            var existingGroup = new GALLERY_GROUP { Group_Title = "Existing Group" };
            _context.GALLERY_GROUP.Add(existingGroup);
            _context.SaveChanges();
            
            var existingRow = new GALLERY_ROWS { Layout_Name = layoutName, Row_Index = 0, Group_Id = existingGroup.Group_Id };
            _context.GALLERY_ROWS.Add(existingRow);
            _context.SaveChanges();

            // Act
            var result = _galleryEditor.AddGalleryGroup("New Group", layoutName);

            // Assert
            var newRow = _context.GALLERY_ROWS.FirstOrDefault(r => r.Group_Id == result);
            newRow.Should().NotBeNull();
            newRow!.Row_Index.Should().Be(1); // Next row index
        }

        #endregion

        #region AddCustomGalleryGroup Tests

        [TestMethod]
        public void AddCustomGalleryGroup_ValidData_CreatesGroupAtTop()
        {
            // Arrange
            var groupTitle = "Custom Group";
            var layoutName = "TEST_LAYOUT";
            var layout = new GALLERY_LAYOUT { Layout_Name = layoutName };
            _context.GALLERY_LAYOUT.Add(layout);
            
            // Add existing rows
            var existingGroup = new GALLERY_GROUP { Group_Title = "Existing Group" };
            _context.GALLERY_GROUP.Add(existingGroup);
            _context.SaveChanges();
            
            var existingRow = new GALLERY_ROWS { Layout_Name = layoutName, Row_Index = 0, Group_Id = existingGroup.Group_Id };
            _context.GALLERY_ROWS.Add(existingRow);
            _context.SaveChanges();

            // Act
            var result = _galleryEditor.AddCustomGalleryGroup(groupTitle, layoutName);

            // Assert
            var newRow = _context.GALLERY_ROWS.FirstOrDefault(r => r.Group_Id == result);
            newRow.Should().NotBeNull();
            newRow!.Row_Index.Should().Be(0); // At top
            
            // Existing row should be moved down
            var existingRowUpdated = _context.GALLERY_ROWS.FirstOrDefault(r => r.Group_Id == existingGroup.Group_Id);
            existingRowUpdated.Should().NotBeNull();
            existingRowUpdated!.Row_Index.Should().Be(1);
        }

        #endregion

        #region CloneGalleryItem Tests

        [TestMethod]
        public void CloneGalleryItem_WithNewId_CreatesNewItem()
        {
            // Arrange
            var originalItem = CreateTestGalleryItem();
            var groupId = 1;
            var columnId = 0;
            
            var group = new GALLERY_GROUP { Group_Id = groupId, Group_Title = "Test Group" };
            _context.GALLERY_GROUP.Add(group);
            _context.GALLERY_ITEM.Add(originalItem);
            _context.SaveChanges();
            
            var detail = new GALLERY_GROUP_DETAILS
            {
                Group_Id = groupId,
                Column_Index = columnId,
                Gallery_Item_Guid = originalItem.Gallery_Item_Guid,
                Click_Count = 5
            };
            _context.GALLERY_GROUP_DETAILS.Add(detail);
            _context.SaveChanges();

            // Act
            _galleryEditor.CloneGalleryItem(originalItem.Gallery_Item_Guid, groupId, true);

            // Assert
            var clonedItem = _context.GALLERY_ITEM.FirstOrDefault(i => i.Gallery_Item_Guid != originalItem.Gallery_Item_Guid);
            clonedItem.Should().NotBeNull();
            clonedItem!.Title.Should().Be(originalItem.Title);
            clonedItem.Description.Should().Be(originalItem.Description);
            clonedItem.Configuration_Setup.Should().Be(originalItem.Configuration_Setup);
            clonedItem.Is_Visible.Should().BeTrue();
            clonedItem.CreationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            var clonedDetail = _context.GALLERY_GROUP_DETAILS.FirstOrDefault(d => d.Gallery_Item_Guid == clonedItem.Gallery_Item_Guid);
            clonedDetail.Should().NotBeNull();
            clonedDetail!.Group_Id.Should().Be(groupId);
            clonedDetail.Column_Index.Should().Be(1); // Next column
            clonedDetail.Click_Count.Should().Be(5); // Preserved click count
        }

        [TestMethod]
        public void CloneGalleryItem_WithoutNewId_ReusesExistingItem()
        {
            // Arrange
            var originalItem = CreateTestGalleryItem();
            var groupId = 1;
            var columnId = 0;
            
            var group = new GALLERY_GROUP { Group_Id = groupId, Group_Title = "Test Group" };
            _context.GALLERY_GROUP.Add(group);
            _context.GALLERY_ITEM.Add(originalItem);
            _context.SaveChanges();
            
            var detail = new GALLERY_GROUP_DETAILS
            {
                Group_Id = groupId,
                Column_Index = columnId,
                Gallery_Item_Guid = originalItem.Gallery_Item_Guid,
                Click_Count = 5
            };
            _context.GALLERY_GROUP_DETAILS.Add(detail);
            _context.SaveChanges();

            var originalItemCount = _context.GALLERY_ITEM.Count();

            // Act
            _galleryEditor.CloneGalleryItem(originalItem.Gallery_Item_Guid, groupId, false);

            // Assert
            _context.GALLERY_ITEM.Count().Should().Be(originalItemCount); // No new item created
            
            var newDetail = _context.GALLERY_GROUP_DETAILS.FirstOrDefault(d => d.Gallery_Item_Guid == originalItem.Gallery_Item_Guid && d.Column_Index == 1);
            newDetail.Should().NotBeNull();
            newDetail!.Group_Id.Should().Be(groupId);
        }

        [TestMethod]
        public void CloneGalleryItem_NonExistentItem_DoesNothing()
        {
            // Arrange
            var nonExistentGuid = Guid.NewGuid();
            var groupId = 1;
            
            var group = new GALLERY_GROUP { Group_Id = groupId, Group_Title = "Test Group" };
            _context.GALLERY_GROUP.Add(group);
            _context.SaveChanges();

            var originalDetailCount = _context.GALLERY_GROUP_DETAILS.Count();

            // Act
            _galleryEditor.CloneGalleryItem(nonExistentGuid, groupId, true);

            // Assert
            _context.GALLERY_GROUP_DETAILS.Count().Should().Be(originalDetailCount);
        }

        #endregion

        #region CloneGalleryGroup Tests

        [TestMethod]
        public void CloneGalleryGroup_ValidGroup_ClonesGroupAndDetails()
        {
            // Arrange
            var layoutName = "TEST_LAYOUT";
            var layout = new GALLERY_LAYOUT { Layout_Name = layoutName };
            _context.GALLERY_LAYOUT.Add(layout);
            
            var originalGroup = new GALLERY_GROUP { Group_Title = "Original Group" };
            _context.GALLERY_GROUP.Add(originalGroup);
            _context.SaveChanges();
            
            var originalRow = new GALLERY_ROWS { Layout_Name = layoutName, Row_Index = 0, Group_Id = originalGroup.Group_Id };
            _context.GALLERY_ROWS.Add(originalRow);
            
            var item = CreateTestGalleryItem();
            _context.GALLERY_ITEM.Add(item);
            _context.SaveChanges();
            
            var detail = new GALLERY_GROUP_DETAILS
            {
                Group_Id = originalGroup.Group_Id,
                Column_Index = 0,
                Gallery_Item_Guid = item.Gallery_Item_Guid,
                Click_Count = 5
            };
            _context.GALLERY_GROUP_DETAILS.Add(detail);
            _context.SaveChanges();

            // Act
            _galleryEditor.CloneGalleryGroup(originalGroup.Group_Id, layoutName);

            // Assert
            var clonedGroup = _context.GALLERY_GROUP.FirstOrDefault(g => g.Group_Id != originalGroup.Group_Id);
            clonedGroup.Should().NotBeNull();
            clonedGroup!.Group_Title.Should().Be(originalGroup.Group_Title);
            
            var clonedRow = _context.GALLERY_ROWS.FirstOrDefault(r => r.Group_Id == clonedGroup.Group_Id);
            clonedRow.Should().NotBeNull();
            clonedRow!.Layout_Name.Should().Be(layoutName);
            clonedRow.Row_Index.Should().Be(1); // Next row
            
            var clonedDetail = _context.GALLERY_GROUP_DETAILS.FirstOrDefault(d => d.Group_Id == clonedGroup.Group_Id);
            clonedDetail.Should().NotBeNull();
            clonedDetail!.Gallery_Item_Guid.Should().Be(item.Gallery_Item_Guid);
            clonedDetail.Column_Index.Should().Be(0);
        }

        #endregion

        #region DeleteGalleryItem Tests

        [TestMethod]
        public void DeleteGalleryItem_ExistingItem_RemovesDetail()
        {
            // Arrange
            var item = CreateTestGalleryItem();
            var groupId = 1;
            
            var group = new GALLERY_GROUP { Group_Id = groupId, Group_Title = "Test Group" };
            _context.GALLERY_GROUP.Add(group);
            _context.GALLERY_ITEM.Add(item);
            _context.SaveChanges();
            
            var detail = new GALLERY_GROUP_DETAILS
            {
                Group_Id = groupId,
                Column_Index = 0,
                Gallery_Item_Guid = item.Gallery_Item_Guid,
                Click_Count = 5
            };
            _context.GALLERY_GROUP_DETAILS.Add(detail);
            _context.SaveChanges();

            var originalDetailCount = _context.GALLERY_GROUP_DETAILS.Count();

            // Act
            _galleryEditor.DeleteGalleryItem(item.Gallery_Item_Guid, groupId);

            // Assert
            _context.GALLERY_GROUP_DETAILS.Count().Should().Be(originalDetailCount - 1);
            _context.GALLERY_GROUP_DETAILS.FirstOrDefault(d => d.Gallery_Item_Guid == item.Gallery_Item_Guid).Should().BeNull();
        }

        [TestMethod]
        public void DeleteGalleryItem_NonExistentItem_DoesNothing()
        {
            // Arrange
            var nonExistentGuid = Guid.NewGuid();
            var groupId = 1;
            
            var group = new GALLERY_GROUP { Group_Id = groupId, Group_Title = "Test Group" };
            _context.GALLERY_GROUP.Add(group);
            _context.SaveChanges();

            var originalDetailCount = _context.GALLERY_GROUP_DETAILS.Count();

            // Act
            _galleryEditor.DeleteGalleryItem(nonExistentGuid, groupId);

            // Assert
            _context.GALLERY_GROUP_DETAILS.Count().Should().Be(originalDetailCount);
        }

        #endregion

        #region DeleteGalleryGroup Tests

        [TestMethod]
        public void DeleteGalleryGroup_ExistingGroup_RemovesRow()
        {
            // Arrange
            var layoutName = "TEST_LAYOUT";
            var layout = new GALLERY_LAYOUT { Layout_Name = layoutName };
            _context.GALLERY_LAYOUT.Add(layout);
            
            var group = new GALLERY_GROUP { Group_Title = "Test Group" };
            _context.GALLERY_GROUP.Add(group);
            _context.SaveChanges();
            
            var row = new GALLERY_ROWS { Layout_Name = layoutName, Row_Index = 0, Group_Id = group.Group_Id };
            _context.GALLERY_ROWS.Add(row);
            _context.SaveChanges();

            var originalRowCount = _context.GALLERY_ROWS.Count();

            // Act
            _galleryEditor.DeleteGalleryGroup(group.Group_Id, layoutName);

            // Assert
            _context.GALLERY_ROWS.Count().Should().Be(originalRowCount - 1);
            _context.GALLERY_ROWS.FirstOrDefault(r => r.Group_Id == group.Group_Id).Should().BeNull();
        }

        #endregion

        #region GetUnused Tests

        [TestMethod]
        public void GetUnused_WithUnusedItems_ReturnsUnusedItems()
        {
            // Arrange
            var layoutName = "TEST_LAYOUT";
            var layout = new GALLERY_LAYOUT { Layout_Name = layoutName };
            _context.GALLERY_LAYOUT.Add(layout);
            
            var group = new GALLERY_GROUP { Group_Title = "Test Group" };
            _context.GALLERY_GROUP.Add(group);
            _context.SaveChanges();
            
            var row = new GALLERY_ROWS { Layout_Name = layoutName, Row_Index = 0, Group_Id = group.Group_Id };
            _context.GALLERY_ROWS.Add(row);
            
            var usedItem = CreateTestGalleryItem("Used Item");
            var unusedItem = CreateTestGalleryItem("Unused Item");
            _context.GALLERY_ITEM.AddRange(usedItem, unusedItem);
            _context.SaveChanges();
            
            var detail = new GALLERY_GROUP_DETAILS
            {
                Group_Id = group.Group_Id,
                Column_Index = 0,
                Gallery_Item_Guid = usedItem.Gallery_Item_Guid,
                Click_Count = 0
            };
            _context.GALLERY_GROUP_DETAILS.Add(detail);
            _context.SaveChanges();

            // Act
            var result = _galleryEditor.GetUnused(layoutName);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Title.Should().Be("Unused Item");
        }

        [TestMethod]
        public void GetUnused_AllItemsUsed_ReturnsEmptyArray()
        {
            // Arrange
            var layoutName = "TEST_LAYOUT";
            var layout = new GALLERY_LAYOUT { Layout_Name = layoutName };
            _context.GALLERY_LAYOUT.Add(layout);
            
            var group = new GALLERY_GROUP { Group_Title = "Test Group" };
            _context.GALLERY_GROUP.Add(group);
            _context.SaveChanges();
            
            var row = new GALLERY_ROWS { Layout_Name = layoutName, Row_Index = 0, Group_Id = group.Group_Id };
            _context.GALLERY_ROWS.Add(row);
            
            var item = CreateTestGalleryItem("Used Item");
            _context.GALLERY_ITEM.Add(item);
            _context.SaveChanges();
            
            var detail = new GALLERY_GROUP_DETAILS
            {
                Group_Id = group.Group_Id,
                Column_Index = 0,
                Gallery_Item_Guid = item.Gallery_Item_Guid,
                Click_Count = 0
            };
            _context.GALLERY_GROUP_DETAILS.Add(detail);
            _context.SaveChanges();

            // Act
            var result = _galleryEditor.GetUnused(layoutName);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetLayouts Tests

        [TestMethod]
        public void GetLayouts_WithExistingLayouts_ReturnsLayouts()
        {
            // Arrange
            var layouts = new List<GALLERY_LAYOUT>
            {
                new GALLERY_LAYOUT { Layout_Name = "LAYOUT_1" },
                new GALLERY_LAYOUT { Layout_Name = "LAYOUT_2" }
            };
            _context.GALLERY_LAYOUT.AddRange(layouts);
            _context.SaveChanges();

            // Act
            var result = _galleryEditor.GetLayouts();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].LayoutName.Should().Be("LAYOUT_1");
            result[1].LayoutName.Should().Be("LAYOUT_2");
        }

        #endregion

        #region UpdateItem Tests

        [TestMethod]
        public void UpdateItem_ExistingItem_UpdatesProperties()
        {
            // Arrange
            var originalItem = CreateTestGalleryItem("Original Title");
            _context.GALLERY_ITEM.Add(originalItem);
            _context.SaveChanges();
            
            var updatedItem = new GALLERY_ITEM
            {
                Gallery_Item_Guid = originalItem.Gallery_Item_Guid,
                Title = "Updated Title",
                Description = "Updated Description",
                Configuration_Setup = "{\"updated\": true}",
                Icon_File_Name_Small = "updated_small.png",
                Icon_File_Name_Large = "updated_large.png",
                Is_Visible = false
            };

            // Act
            _galleryEditor.UpdateItem(updatedItem);

            // Assert
            var result = _context.GALLERY_ITEM.FirstOrDefault(i => i.Gallery_Item_Guid == originalItem.Gallery_Item_Guid);
            result.Should().NotBeNull();
            result!.Title.Should().Be("Updated Title");
            result.Description.Should().Be("Updated Description");
            result.Configuration_Setup.Should().Be("{\"updated\": true}");
            result.Icon_File_Name_Small.Should().Be("updated_small.png");
            result.Icon_File_Name_Large.Should().Be("updated_large.png");
            result.Is_Visible.Should().BeFalse();
        }

        #endregion

        #region RenumberGroup Tests

        [TestMethod]
        public void RenumberGroup_WithDetails_ReordersColumnIndexes()
        {
            // Arrange
            var details = new List<GALLERY_GROUP_DETAILS>
            {
                new GALLERY_GROUP_DETAILS { Group_Id = 1, Column_Index = 5, Gallery_Item_Guid = Guid.NewGuid(), Click_Count = 0 },
                new GALLERY_GROUP_DETAILS { Group_Id = 1, Column_Index = 2, Gallery_Item_Guid = Guid.NewGuid(), Click_Count = 0 },
                new GALLERY_GROUP_DETAILS { Group_Id = 1, Column_Index = 8, Gallery_Item_Guid = Guid.NewGuid(), Click_Count = 0 }
            };

            // Act
            _galleryEditor.RenumberGroup(details);

            // Assert
            details[0].Column_Index.Should().Be(0);
            details[1].Column_Index.Should().Be(1);
            details[2].Column_Index.Should().Be(2);
        }

        [TestMethod]
        public void RenumberGroup_WithRows_ReordersRowIndexes()
        {
            // Arrange
            var rows = new List<GALLERY_ROWS>
            {
                new GALLERY_ROWS { Layout_Name = "TEST", Row_Index = 5, Group_Id = 1 },
                new GALLERY_ROWS { Layout_Name = "TEST", Row_Index = 2, Group_Id = 2 },
                new GALLERY_ROWS { Layout_Name = "TEST", Row_Index = 8, Group_Id = 3 }
            };

            // Act
            _galleryEditor.RenumberGroup(rows);

            // Assert
            rows[0].Row_Index.Should().Be(0);
            rows[1].Row_Index.Should().Be(1);
            rows[2].Row_Index.Should().Be(2);
        }

        #endregion

        #region UpdatePosition Tests

        [TestMethod]
        public void UpdatePosition_MoveItemToNewGroup_MovesItemCorrectly()
        {
            // Arrange
            var layoutName = "TEST_LAYOUT";
            var layout = new GALLERY_LAYOUT { Layout_Name = layoutName };
            _context.GALLERY_LAYOUT.Add(layout);
            
            var fromGroup = new GALLERY_GROUP { Group_Title = "From Group" };
            var toGroup = new GALLERY_GROUP { Group_Title = "To Group" };
            _context.GALLERY_GROUP.AddRange(fromGroup, toGroup);
            _context.SaveChanges();
            
            var fromRow = new GALLERY_ROWS { Layout_Name = layoutName, Row_Index = 0, Group_Id = fromGroup.Group_Id };
            var toRow = new GALLERY_ROWS { Layout_Name = layoutName, Row_Index = 1, Group_Id = toGroup.Group_Id };
            _context.GALLERY_ROWS.AddRange(fromRow, toRow);
            
            var item = CreateTestGalleryItem();
            _context.GALLERY_ITEM.Add(item);
            _context.SaveChanges();
            
            var detail = new GALLERY_GROUP_DETAILS
            {
                Group_Id = fromGroup.Group_Id,
                Column_Index = 0,
                Gallery_Item_Guid = item.Gallery_Item_Guid,
                Click_Count = 5
            };
            _context.GALLERY_GROUP_DETAILS.Add(detail);
            _context.SaveChanges();
            
            var moveItem = new MoveItem
            {
                Layout_Name = layoutName,
                fromId = fromGroup.Group_Id.ToString(),
                toId = toGroup.Group_Id.ToString(),
                oldIndex = "0",
                newIndex = "0",
                gallery_Item_Guid = item.Gallery_Item_Guid.ToString()
            };

            // Act
            _galleryEditor.UpdatePosition(moveItem);

            // Assert
            var movedDetail = _context.GALLERY_GROUP_DETAILS.FirstOrDefault(d => d.Gallery_Item_Guid == item.Gallery_Item_Guid);
            movedDetail.Should().NotBeNull();
            movedDetail!.Group_Id.Should().Be(toGroup.Group_Id);
        }

        #endregion

        #region Helper Methods

        private GALLERY_ITEM CreateTestGalleryItem(string title = "Test Item")
        {
            return new GALLERY_ITEM
            {
                Gallery_Item_Guid = Guid.NewGuid(),
                Title = title,
                Description = "Test Description",
                Configuration_Setup = "{}",
                Is_Visible = true,
                CreationDate = DateTime.UtcNow
            };
        }

        #endregion
    }
}