using LTres.Olt.Api.Core.Workers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LTres.Olt.Api.Core.Tests.Workers;

public class WorkProbeCacheTests
{

    
    [Fact]
    public async Task TryToPutIntoCache_ShouldAddItemToCacheAndReturnsTrue_WhenGuidNotExists()
    {
        var logCounter = new MockLogCounter();
        var workProbeCache = new WorkProbeCache(logCounter);
        var idWork = Guid.NewGuid();
        var requestedIn = DateTime.Now;
        
        var result = await workProbeCache.TryToPutIntoCache(idWork, requestedIn);
        
        Assert.True(result);
    }

    [Fact]
    public async Task TryToPutIntoCache_ShouldAddItemToCacheAndValidateAsThreadSafe()
    {
        var logCounter = new MockLogCounter();
        var workProbeCache = new WorkProbeCache(logCounter);
        var idWork = Guid.NewGuid();
        var requestedIn = DateTime.Now;
        bool result1 = false;
        bool result2 = false;

        var task1 = Task.Run(async () => result1 = await workProbeCache.TryToPutIntoCache(idWork, requestedIn));
        var task2 = Task.Run(async () => result2 = await workProbeCache.TryToPutIntoCache(idWork, requestedIn));

        await Task.WhenAll(task1, task2);

        Assert.True(result1 != result2);        
    }

    [Fact]
    public async Task TryToPutIntoCache_ShouldNotAddItemToCacheAndReturnsFalse_WhenGuidAlreadyExists()
    {
        var logCounter = new MockLogCounter();
        var workProbeCache = new WorkProbeCache(logCounter);
        var idWork = Guid.NewGuid();
        var requestedIn = DateTime.Now;
        
        await workProbeCache.TryToPutIntoCache(idWork, requestedIn);
        var result = await workProbeCache.TryToPutIntoCache(idWork, requestedIn);
        
        Assert.False(result);
    }

    [Fact]
    public async Task TryToPutIntoCache_ShouldRemoveOldItemsFromCache()
    {
        var logCounter = new MockLogCounter();
        var workProbeCache = new WorkProbeCache(logCounter);
        var idWork1 = Guid.NewGuid();
        var idWork2 = Guid.NewGuid();
        var requestedIn1 = DateTime.Now.Subtract(TimeSpan.FromMinutes(4));
        var requestedIn2 = DateTime.Now;

        await workProbeCache.TryToPutIntoCache(idWork1, requestedIn1);
        await workProbeCache.TryToPutIntoCache(idWork2, requestedIn2);

        var result = await workProbeCache.TryToPutIntoCache(idWork1, requestedIn1);

        Assert.True(result);
    }

    [Fact]
    public async Task TryToRemoveFromCache_ShouldRemoveItemFromCacheAndReturnsTrue_WhenGuidExists()
    {
        var logCounter = new MockLogCounter();
        var workProbeCache = new WorkProbeCache(logCounter);
        var idWork = Guid.NewGuid();
        var requestedIn = DateTime.Now;
        
        await workProbeCache.TryToPutIntoCache(idWork, requestedIn);
        var result = await workProbeCache.TryToRemoveFromCache(idWork);
        
        Assert.True(result);
    }

    [Fact]
    public async Task TryToRemoveFromCache_ShouldNotRemoveItemFromCacheAndReturnsFalse_WhenGuidDoesNotExist()
    {
        var logCounter = new MockLogCounter();
        var workProbeCache = new WorkProbeCache(logCounter);
        var idWork = Guid.NewGuid();

        var result = await workProbeCache.TryToRemoveFromCache(idWork);

        Assert.False(result);
    }
        
}
