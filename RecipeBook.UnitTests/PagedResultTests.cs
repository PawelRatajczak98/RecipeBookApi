using Application;
using FluentAssertions;
using Xunit;

namespace RecipeBook.UnitTests;

/// <summary>
/// Testy jednostkowe dla PagedResult<T>.
/// Weryfikują logikę obliczania paginacji (ItemFrom, ItemTo, TotalPages).
/// </summary>
public class PagedResultTests
{
    [Fact]
    public void PagedResult_ShouldCalculateItemFrom_Correctly()
    {
        // Arrange
        var items = new List<string> { "Item1", "Item2", "Item3" };
        int totalCount = 100;
        int pageSize = 10;
        int pageNumber = 3;

        // Act
        var result = new PagedResult<string>(items, totalCount, pageSize, pageNumber);

        // Assert
        // ItemFrom = pageSize * (pageNumber - 1) + 1 = 10 * (3 - 1) + 1 = 21
        result.ItemFrom.Should().Be(21);
    }

    [Fact]
    public void PagedResult_ShouldCalculateItemTo_Correctly()
    {
        // Arrange
        var items = new List<string> { "Item1", "Item2", "Item3" };
        int totalCount = 100;
        int pageSize = 10;
        int pageNumber = 3;

        // Act
        var result = new PagedResult<string>(items, totalCount, pageSize, pageNumber);

        // Assert
        // ItemTo = ItemFrom + pageSize - 1 = 21 + 10 - 1 = 30
        result.ItemTo.Should().Be(30);
    }

    [Fact]
    public void PagedResult_ShouldCalculateTotalPages_WithExactDivision()
    {
        // Arrange
        var items = new List<string> { "Item1", "Item2" };
        int totalCount = 100;
        int pageSize = 10;
        int pageNumber = 1;

        // Act
        var result = new PagedResult<string>(items, totalCount, pageSize, pageNumber);

        // Assert
        // TotalPages = Ceiling(100 / 10) = 10
        result.TotalPages.Should().Be(10);
    }

    [Fact]
    public void PagedResult_ShouldCalculateTotalPages_WithRemainder()
    {
        // Arrange
        var items = new List<string> { "Item1", "Item2" };
        int totalCount = 103;
        int pageSize = 10;
        int pageNumber = 1;

        // Act
        var result = new PagedResult<string>(items, totalCount, pageSize, pageNumber);

        // Assert
        // TotalPages = Ceiling(103 / 10) = Ceiling(10.3) = 11
        result.TotalPages.Should().Be(11);
    }

    [Fact]
    public void PagedResult_ShouldHandleZeroItems()
    {
        // Arrange
        var items = new List<string>();
        int totalCount = 0;
        int pageSize = 10;
        int pageNumber = 1;

        // Act
        var result = new PagedResult<string>(items, totalCount, pageSize, pageNumber);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalItemsCount.Should().Be(0);
        result.TotalPages.Should().Be(0); // Ceiling(0 / 10) = 0
        result.ItemFrom.Should().Be(1);   // 10 * (1-1) + 1 = 1
        result.ItemTo.Should().Be(10);     // 1 + 10 - 1 = 10
    }

    [Fact]
    public void PagedResult_ShouldHandleSinglePage()
    {
        // Arrange
        var items = new List<string> { "Item1", "Item2", "Item3" };
        int totalCount = 3;
        int pageSize = 10;
        int pageNumber = 1;

        // Act
        var result = new PagedResult<string>(items, totalCount, pageSize, pageNumber);

        // Assert
        result.TotalPages.Should().Be(1); // Ceiling(3 / 10) = 1
        result.ItemFrom.Should().Be(1);
        result.ItemTo.Should().Be(10);
        result.TotalItemsCount.Should().Be(3);
    }

    [Fact]
    public void PagedResult_Properties_ShouldMatchConstructorParams()
    {
        // Arrange
        var items = new List<string> { "A", "B", "C", "D", "E" };
        int totalCount = 50;
        int pageSize = 5;
        int pageNumber = 2;

        // Act
        var result = new PagedResult<string>(items, totalCount, pageSize, pageNumber);

        // Assert
        result.Items.Should().BeSameAs(items);
        result.Items.Should().HaveCount(5);
        result.TotalItemsCount.Should().Be(50);
    }

    [Fact]
    public void PagedResult_FirstPage_ShouldStartFromOne()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5 };
        int totalCount = 50;
        int pageSize = 5;
        int pageNumber = 1;

        // Act
        var result = new PagedResult<int>(items, totalCount, pageSize, pageNumber);

        // Assert
        result.ItemFrom.Should().Be(1); // 5 * (1-1) + 1 = 1
        result.ItemTo.Should().Be(5);   // 1 + 5 - 1 = 5
    }

    [Fact]
    public void PagedResult_LastPage_ShouldCalculateCorrectly()
    {
        // Arrange
        var items = new List<int> { 96, 97, 98, 99, 100 };
        int totalCount = 100;
        int pageSize = 5;
        int pageNumber = 20; // Ostatnia strona

        // Act
        var result = new PagedResult<int>(items, totalCount, pageSize, pageNumber);

        // Assert
        result.ItemFrom.Should().Be(96); // 5 * (20-1) + 1 = 96
        result.ItemTo.Should().Be(100);  // 96 + 5 - 1 = 100
        result.TotalPages.Should().Be(20);
    }

    [Theory]
    [InlineData(1, 10, 100, 1, 10, 10)]
    [InlineData(2, 10, 100, 11, 20, 10)]
    [InlineData(5, 20, 100, 81, 100, 5)]
    [InlineData(1, 25, 103, 1, 25, 5)]
    public void PagedResult_WithVariousInputs_ShouldCalculateCorrectly(
        int pageNumber,
        int pageSize,
        int totalCount,
        int expectedFrom,
        int expectedTo,
        int expectedPages)
    {
        // Arrange
        var items = new List<object>();

        // Act
        var result = new PagedResult<object>(items, totalCount, pageSize, pageNumber);

        // Assert
        result.ItemFrom.Should().Be(expectedFrom);
        result.ItemTo.Should().Be(expectedTo);
        result.TotalPages.Should().Be(expectedPages);
    }

    [Fact]
    public void PagedResult_WithOneItem_ShouldCalculateCorrectly()
    {
        // Arrange
        var items = new List<string> { "SingleItem" };
        int totalCount = 1;
        int pageSize = 10;
        int pageNumber = 1;

        // Act
        var result = new PagedResult<string>(items, totalCount, pageSize, pageNumber);

        // Assert
        result.TotalPages.Should().Be(1); // Ceiling(1 / 10) = 1
        result.ItemFrom.Should().Be(1);
        result.ItemTo.Should().Be(10);
        result.TotalItemsCount.Should().Be(1);
    }

    [Fact]
    public void PagedResult_WithLargePageSize_ShouldCalculateCorrectly()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        int totalCount = 3;
        int pageSize = 100;
        int pageNumber = 1;

        // Act
        var result = new PagedResult<int>(items, totalCount, pageSize, pageNumber);

        // Assert
        result.TotalPages.Should().Be(1); // Ceiling(3 / 100) = 1
        result.ItemFrom.Should().Be(1);
        result.ItemTo.Should().Be(100);
    }

    [Fact]
    public void PagedResult_WithDifferentTypes_ShouldWork()
    {
        // Arrange - test z różnymi typami generycznymi
        var stringItems = new List<string> { "a", "b" };
        var intItems = new List<int> { 1, 2, 3 };
        var objectItems = new List<object> { new object() };

        // Act
        var stringResult = new PagedResult<string>(stringItems, 10, 5, 1);
        var intResult = new PagedResult<int>(intItems, 10, 5, 1);
        var objectResult = new PagedResult<object>(objectItems, 10, 5, 1);

        // Assert
        stringResult.Should().NotBeNull();
        intResult.Should().NotBeNull();
        objectResult.Should().NotBeNull();
    }
}
