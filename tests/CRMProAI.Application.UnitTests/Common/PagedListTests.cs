using CRMProAI.Contracts.Common;
using FluentAssertions;
using Xunit;

namespace CRMProAI.Application.UnitTests.Common;

public class PagedListTests
{
    [Fact]
    public void PagedList_ShouldCalculateTotalPagesCorrectly()
    {
        var items = new List<string> { "Item 1", "Item 2" };
        var pagedList = new PagedList<string>(items, TotalCount: 50, Page: 1, PageSize: 10);

        pagedList.TotalPages.Should().Be(5);
        pagedList.HasNextPage.Should().BeTrue();
        pagedList.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void PagedList_OnMiddlePage_ShouldIndicateNextAndPreviousPages()
    {
        var items = new List<string> { "Item 11", "Item 12" };
        var pagedList = new PagedList<string>(items, TotalCount: 50, Page: 2, PageSize: 10);

        pagedList.HasNextPage.Should().BeTrue();
        pagedList.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void PagedList_OnLastPage_ShouldNotHaveNextPage()
    {
        var items = new List<string> { "Item 41" };
        var pagedList = new PagedList<string>(items, TotalCount: 45, Page: 5, PageSize: 10);

        pagedList.TotalPages.Should().Be(5);
        pagedList.HasNextPage.Should().BeFalse();
        pagedList.HasPreviousPage.Should().BeTrue();
    }
}
