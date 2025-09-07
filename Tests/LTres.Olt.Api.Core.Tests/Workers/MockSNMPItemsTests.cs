using LTres.Olt.Api.Core.Tools;

namespace LTres.Olt.Api.Core.Tests.Workers;

public class MockSNMPItemsTests
{

    [Fact]
    public void This_ShouldReturnNullIfNotExists()
    {
        var mockSNMPItems = new MockSNMPItems(null);

        var result = mockSNMPItems["123.456"];

        Assert.Null(result);
    }

    [Fact]
    public void This_ShouldReturnNotNull_WhenOidExists()
    {
        var mockSNMPItems = new MockSNMPItems(null);

        var result = mockSNMPItems["1.3.6.1.4.1.3902.1012.3.28.1.1.2.268501248.1"];

        Assert.NotNull(result);
    }

    [Fact]
    public void This_ShouldReturnNotNull_WhenOidWithDotAtStartExists()
    {
        var mockSNMPItems = new MockSNMPItems(null);

        var result = mockSNMPItems[".1.3.6.1.4.1.3902.1012.3.28.1.1.2.268501248.1"];

        Assert.NotNull(result);
    }

    [Fact]
    public void This_ShouldReturnValidStrValue_WhenOidExists()
    {
        var mockSNMPItems = new MockSNMPItems(null);

        var result = mockSNMPItems["1.3.6.1.4.1.3902.1012.3.28.1.1.2.268501248.1"];

        Assert.NotNull(result);
        Assert.Equal(0, result.Type);
        Assert.NotNull(result.ValuesStr);
        Assert.Equal("ONU NUMBER 1", result.ValuesStr.First());
    }

    [Fact]
    public void This_ShouldReturnValidIntValue_WhenOidExists()
    {
        var mockSNMPItems = new MockSNMPItems(null);

        var result = mockSNMPItems["1.3.6.1.4.1.3902.1012.3.28.2.1.4.268501248.1"];

        Assert.NotNull(result);
        Assert.Equal(1, result.Type);
        Assert.NotNull(result.ValuesInt);
        Assert.Equal(3, result.ValuesInt.First());
    }

    [Fact]
    public void This_ShouldReturnValidUIntValue_WhenOidExists()
    {
        var mockSNMPItems = new MockSNMPItems(null);

        var result = mockSNMPItems["1.3.6.1.4.1.3902.1012.3.28.2.1.5.268501248.1"];

        Assert.NotNull(result);
        Assert.Equal(2, result.Type);
        Assert.NotNull(result.ValuesUInt);
        Assert.Equal((uint)3, result.ValuesUInt.First());
    }


    [Fact]
    public void StartWithOid_ShouldReturnEmpty_WhenOidNotFound()
    {
        var mockSNMPItems = new MockSNMPItems(null);

        var result = mockSNMPItems.StartWithOid("123.456");
        
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void StartWithOid_ShouldReturnAValidList_WhenOidStartExists()
    {
        var mockSNMPItems = new MockSNMPItems(null);

        var result = mockSNMPItems.StartWithOid("1.3.6.1.4.1.3902.1012.3.28.1.1.2");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

}
