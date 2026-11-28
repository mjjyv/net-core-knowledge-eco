using Ecosystem.Application.Common;

namespace Ecosystem.UnitTests.Application;

public class PaginationModelsTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(1, 1)]
    [InlineData(4, 4)]
    public void PaginationParams_ClampsPageNumberToAtLeastOne(int input, int expected)
    {
        var query = new PaginationParams { PageNumber = input };
        Assert.Equal(expected, query.PageNumber);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-10, 1)]
    [InlineData(1, 1)]
    [InlineData(25, 25)]
    [InlineData(50, 50)]
    [InlineData(500, 50)]
    public void PaginationParams_ClampsPageSizeBetweenOneAndMax(int input, int expected)
    {
        var query = new PaginationParams { PageSize = input };
        Assert.Equal(expected, query.PageSize);
    }

    [Theory]
    [InlineData(0, 1, 10, 0, false, false)]
    [InlineData(5, 1, 2, 3, false, true)]
    [InlineData(5, 2, 2, 3, true, true)]
    [InlineData(5, 3, 2, 3, true, false)]
    [InlineData(6, 3, 2, 3, true, false)]
    public void PagedResult_ComputesNavigationMetadataAccurately(
        int totalCount,
        int pageNumber,
        int pageSize,
        int expectedTotalPages,
        bool expectedHasPrev,
        bool expectedHasNext)
    {
        var paged = new PagedResult<string>(Array.Empty<string>(), totalCount, pageNumber, pageSize);

        Assert.Equal(expectedTotalPages, paged.TotalPages);
        Assert.Equal(expectedHasPrev, paged.HasPreviousPage);
        Assert.Equal(expectedHasNext, paged.HasNextPage);
    }
}
