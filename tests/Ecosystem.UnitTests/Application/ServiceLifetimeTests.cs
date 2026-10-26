using Ecosystem.Application.Interfaces;
using Ecosystem.WebAPI.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ecosystem.UnitTests.Application;

public class ServiceLifetimeTests
{
    private readonly IServiceProvider _serviceProvider;

    public ServiceLifetimeTests()
    {
        var services = new ServiceCollection();
        services.AddLifetimeDemonstrationServices();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void SingletonService_AcrossMultipleResolves_ShouldReturnSameInstance()
    {
        // Act
        var instance1 = _serviceProvider.GetRequiredService<ISingletonService>();
        var instance2 = _serviceProvider.GetRequiredService<ISingletonService>();

        // Assert
        Assert.Equal(instance1.InstanceId, instance2.InstanceId);
        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void TransientService_AcrossMultipleResolves_ShouldReturnDifferentInstances()
    {
        // Act
        var instance1 = _serviceProvider.GetRequiredService<ITransientService>();
        var instance2 = _serviceProvider.GetRequiredService<ITransientService>();

        // Assert
        Assert.NotEqual(instance1.InstanceId, instance2.InstanceId);
        Assert.NotSame(instance1, instance2);
    }

    [Fact]
    public void ScopedService_WithinSameScope_ShouldReturnSameInstance()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();

        // Act
        var instance1 = scope.ServiceProvider.GetRequiredService<IScopedService>();
        var instance2 = scope.ServiceProvider.GetRequiredService<IScopedService>();

        // Assert
        Assert.Equal(instance1.InstanceId, instance2.InstanceId);
        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void ScopedService_AcrossDifferentScopes_ShouldReturnDifferentInstances()
    {
        // Arrange
        using var scope1 = _serviceProvider.CreateScope();
        using var scope2 = _serviceProvider.CreateScope();

        // Act
        var instance1 = scope1.ServiceProvider.GetRequiredService<IScopedService>();
        var instance2 = scope2.ServiceProvider.GetRequiredService<IScopedService>();

        // Assert
        Assert.NotEqual(instance1.InstanceId, instance2.InstanceId);
        Assert.NotSame(instance1, instance2);
    }
}
