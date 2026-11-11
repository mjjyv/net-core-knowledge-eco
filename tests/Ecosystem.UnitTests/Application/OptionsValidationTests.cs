using Ecosystem.Application.Common;
using Ecosystem.WebAPI.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ecosystem.UnitTests.Application;

public class OptionsValidationTests
{
    [Fact]
    public void FileStorageOptions_WhenInvalidMaxFileSize_ThrowsOptionsValidationException()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FileStorage:MaxFileSizeBytes"] = "-10",
                ["FileStorage:UploadFolder"] = "wwwroot/uploads"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddConfigurationAndOptions(config);
        using var provider = services.BuildServiceProvider();

        // Act & Assert
        var ex = Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<FileStorageOptions>>().Value);
        Assert.Contains("MaxFileSizeBytes", ex.Message);
    }

    [Fact]
    public void FileStorageOptions_WhenExtensionMissingLeadingDot_ThrowsOptionsValidationException()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FileStorage:MaxFileSizeBytes"] = "5242880",
                ["FileStorage:MaxRequestTotalBytes"] = "15728640",
                ["FileStorage:AllowedExtensions:0"] = "png"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddConfigurationAndOptions(config);
        using var provider = services.BuildServiceProvider();

        // Act & Assert
        var ex = Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<FileStorageOptions>>().Value);
        Assert.Contains("AllowedExtensions", ex.Message);
    }

    [Fact]
    public void ConfigurationHierarchy_EnvironmentOverridesBaseAndEnvVarOverridesAll()
    {
        // Arrange
        var baseSettings = new Dictionary<string, string?>
        {
            ["FileStorage:MaxFileSizeBytes"] = "5242880",
            ["FileStorage:MaxFilesPerRequest"] = "5",
            ["EcosystemFeatures:EnableSwagger"] = "false",
            ["EcosystemFeatures:EnvironmentBanner"] = "Base"
        };

        var devOverride = new Dictionary<string, string?>
        {
            ["FileStorage:MaxFilesPerRequest"] = "10",
            ["EcosystemFeatures:EnableSwagger"] = "true",
            ["EcosystemFeatures:EnvironmentBanner"] = "Development"
        };

        var envVarOverride = new Dictionary<string, string?>
        {
            ["FileStorage:MaxFileSizeBytes"] = "3145728"
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(baseSettings)
            .AddInMemoryCollection(devOverride)
            .AddInMemoryCollection(envVarOverride)
            .Build();

        var services = new ServiceCollection();
        services.AddConfigurationAndOptions(config);
        using var provider = services.BuildServiceProvider();

        // Act
        var fileOpts = provider.GetRequiredService<IOptions<FileStorageOptions>>().Value;
        var featureOpts = provider.GetRequiredService<IOptions<EcosystemFeaturesOptions>>().Value;

        // Assert
        Assert.Equal(3_145_728, fileOpts.MaxFileSizeBytes);
        Assert.Equal(10, fileOpts.MaxFilesPerRequest);
        Assert.True(featureOpts.EnableSwagger);
        Assert.Equal("Development", featureOpts.EnvironmentBanner);
    }
}
