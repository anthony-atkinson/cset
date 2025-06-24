# Gallery Parser Testing Implementation

## Overview

This document describes the comprehensive testing implementation for the CSET Gallery Parser functionality. The Gallery Parser is responsible for managing the gallery board structure, including gallery items, groups, layouts, and their relationships.

## Test Coverage

### GalleryState Tests (`GalleryItemStateParserTests.cs`)

The `GalleryState` class is responsible for retrieving and structuring gallery board data. The tests cover:

#### Core Functionality Tests
- **GetGalleryBoard_ValidLayout_ReturnsCorrectStructure**: Tests that a valid layout returns the correct gallery board structure with proper groups and items
- **GetGalleryBoard_EmptyLayout_ReturnsEmptyStructure**: Tests handling of empty layouts
- **GetGalleryBoard_NonExistentLayout_ReturnsEmptyStructure**: Tests handling of non-existent layouts

#### Data Filtering Tests
- **GetGalleryBoard_WithInvisibleItems_FiltersOutInvisibleItems**: Tests that invisible gallery items are properly filtered out
- **GetGalleryBoard_OrdersByRowIndexAndColumnIndex**: Tests that items are properly ordered by row and column indices

#### Internationalization Tests
- **GetGalleryBoard_WithTranslation_AppliesTranslation**: Tests translation functionality for non-English languages

#### Edge Case Tests
- **GetGalleryBoard_WithNullValues_HandlesGracefully**: Tests handling of null values in gallery data

### GalleryEditor Tests (`GalleryEditorTests.cs`)

The `GalleryEditor` class is responsible for managing gallery items and groups. The tests cover:

#### Layout Management Tests
- **GetLayout_WithExistingLayouts_ReturnsAllLayouts**: Tests retrieval of all available layouts
- **GetLayout_EmptyDatabase_ReturnsEmptyList**: Tests handling of empty layout database

#### Gallery Item Management Tests
- **AddGalleryItem_ValidData_CreatesItemAndDetails**: Tests creation of gallery items with valid data
- **AddGalleryItem_WithNullValues_HandlesGracefully**: Tests handling of null values during item creation
- **UpdateItem_ExistingItem_UpdatesProperties**: Tests updating of existing gallery items
- **DeleteGalleryItem_ExistingItem_RemovesDetail**: Tests deletion of gallery items
- **DeleteGalleryItem_NonExistentItem_DoesNothing**: Tests handling of non-existent item deletion

#### Gallery Group Management Tests
- **AddGalleryGroup_ValidData_CreatesGroupAndRow**: Tests creation of gallery groups
- **AddGalleryGroup_WithExistingRows_SetsCorrectRowIndex**: Tests proper row indexing when adding groups
- **AddCustomGalleryGroup_ValidData_CreatesGroupAtTop**: Tests creation of custom groups at the top
- **DeleteGalleryGroup_ExistingGroup_RemovesRow**: Tests deletion of gallery groups

#### Cloning Tests
- **CloneGalleryItem_WithNewId_CreatesNewItem**: Tests cloning of gallery items with new IDs
- **CloneGalleryItem_WithoutNewId_ReusesExistingItem**: Tests cloning without creating new items
- **CloneGalleryItem_NonExistentItem_DoesNothing**: Tests handling of non-existent item cloning
- **CloneGalleryGroup_ValidGroup_ClonesGroupAndDetails**: Tests cloning of entire gallery groups

#### Utility Tests
- **GetUnused_WithUnusedItems_ReturnsUnusedItems**: Tests retrieval of unused gallery items
- **GetUnused_AllItemsUsed_ReturnsEmptyArray**: Tests handling when all items are used
- **GetLayouts_WithExistingLayouts_ReturnsLayouts**: Tests retrieval of layout information
- **RenumberGroup_WithDetails_ReordersColumnIndexes**: Tests reordering of column indices
- **RenumberGroup_WithRows_ReordersRowIndexes**: Tests reordering of row indices

#### Position Management Tests
- **UpdatePosition_MoveItemToNewGroup_MovesItemCorrectly**: Tests moving items between groups

## Test Data Management

### In-Memory Database
All tests use Entity Framework Core's in-memory database provider for fast, isolated testing:

```csharp
protected CSETContext CreateInMemoryDbContext(string? databaseName = null)
{
    var options = new DbContextOptionsBuilder<CSETContext>()
        .UseInMemoryDatabase(databaseName ?? $"TestDb_{Guid.NewGuid()}")
        .Options;

    return new CSETContext(options);
}
```

### Test Data Creation
Comprehensive helper methods create realistic test data:

- `CreateTestGalleryData()`: Creates basic gallery structure
- `CreateTestGalleryDataWithInvisibleItems()`: Creates data with invisible items
- `CreateTestGalleryDataWithCustomOrdering()`: Creates data with specific ordering
- `CreateTestGalleryDataWithNullValues()`: Creates data with null values
- `CreateTestGalleryItem()`: Creates individual gallery items

### Mock Dependencies
Key dependencies are mocked to isolate the units under test:

- `ITokenManager`: For language and authentication management
- `IMaturityBusiness`: For maturity model operations
- `IStandardsBusiness`: For standards operations
- `IQuestionRequirementManager`: For question/requirement operations

## Test Scenarios Covered

### Gallery Board Retrieval
1. **Valid Layout Processing**: Complete gallery board structure retrieval
2. **Empty Layout Handling**: Graceful handling of empty layouts
3. **Non-existent Layout Handling**: Proper handling of missing layouts
4. **Item Visibility Filtering**: Correct filtering of invisible items
5. **Ordering Validation**: Proper row and column ordering
6. **Translation Support**: Multi-language support testing
7. **Null Value Handling**: Robust handling of null data

### Gallery Item Management
1. **Item Creation**: Complete item creation with all properties
2. **Item Updates**: Property updates and validation
3. **Item Deletion**: Safe item removal
4. **Item Cloning**: Both new ID and reuse scenarios
5. **Null Value Handling**: Graceful handling of null inputs
6. **Non-existent Item Handling**: Safe handling of missing items

### Gallery Group Management
1. **Group Creation**: Standard and custom group creation
2. **Group Deletion**: Safe group removal
3. **Group Cloning**: Complete group duplication
4. **Row Indexing**: Proper row index management
5. **Position Management**: Item movement between groups

### Layout Management
1. **Layout Retrieval**: Complete layout listing
2. **Layout Validation**: Proper layout structure validation
3. **Empty Layout Handling**: Graceful empty state handling

### Utility Operations
1. **Unused Item Detection**: Identification of unused gallery items
2. **Index Renumbering**: Proper reordering of indices
3. **Position Updates**: Complex position management scenarios

## Test Quality Metrics

### Coverage Targets
- **Line Coverage**: >90% for all gallery parser classes
- **Branch Coverage**: >85% for complex conditional logic
- **Method Coverage**: 100% for all public methods

### Performance Targets
- **Test Execution Time**: <2 seconds per test class
- **Memory Usage**: <50MB per test run
- **Database Operations**: <10 operations per test

### Reliability Targets
- **Test Stability**: >95% pass rate
- **Isolation**: Complete test isolation
- **Reproducibility**: Deterministic test results

## Implementation Details

### Test Infrastructure
- **Base Class**: Extends `BaseBusinessTest` for common functionality
- **Mocking**: Uses Moq for dependency mocking
- **Assertions**: Uses FluentAssertions for readable assertions
- **Data Generation**: Uses AutoFixture and Bogus for test data

### Database Testing
- **In-Memory Provider**: Fast, isolated database testing
- **Transaction Management**: Proper transaction handling
- **Data Seeding**: Comprehensive test data setup
- **Cleanup**: Proper resource disposal

### Error Handling
- **Exception Testing**: Comprehensive exception scenario coverage
- **Edge Case Testing**: Null values, empty collections, invalid data
- **Boundary Testing**: Min/max values, empty strings, special characters

## Usage Examples

### Running Gallery Parser Tests

```bash
# Run all gallery parser tests
dotnet test --filter "TestCategory=GalleryParser"

# Run specific test class
dotnet test --filter "ClassName=CSETWebCore.BusinessTests.GalleryParser.GalleryItemStateParserTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~GetGalleryBoard_ValidLayout_ReturnsCorrectStructure"
```

### Test Data Setup

```csharp
// Create test gallery data
var testData = CreateTestGalleryData("TEST_LAYOUT");
SeedGalleryData(testData);

// Create individual test items
var item = CreateTestGalleryItem("Test Item");
_context.GALLERY_ITEM.Add(item);
_context.SaveChanges();
```

## Future Enhancements

### Planned Test Improvements
1. **Integration Tests**: End-to-end gallery workflow testing
2. **Performance Tests**: Load testing for large gallery structures
3. **Concurrency Tests**: Multi-threaded gallery operations
4. **API Tests**: Controller-level gallery testing

### Additional Scenarios
1. **Complex Layout Testing**: Multi-level gallery structures
2. **Custom Set Testing**: Custom gallery set functionality
3. **Translation Testing**: Comprehensive multi-language support
4. **Accessibility Testing**: Gallery accessibility compliance

## Related Documentation

- [Gallery Parser Business Logic](../GalleryParser/)
- [Database Models](../../DataLayer/Model/)
- [Base Test Infrastructure](../Infrastructure/BaseBusinessTest.cs)
- [Test Execution Scripts](../../../scripts/)

## Maintenance Notes

### Test Data Updates
When gallery data structures change, update the test data creation methods to reflect new requirements.

### Mock Updates
When dependencies change, update the mock configurations in the test setup methods.

### Performance Monitoring
Regularly monitor test execution times and optimize slow tests to maintain fast feedback loops. 