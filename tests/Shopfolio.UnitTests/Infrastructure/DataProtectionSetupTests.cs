using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The ring setup (T232 KK1; 09 15.5 item 1): the options it produces are the fixed application name, the database key repository, no key
/// encryptor and the framework's default key lifetime. The services are built without a database: the repository is created but not read.
/// </summary>
public sealed class DataProtectionSetupTests
{
    private static ServiceProvider Build()
    {
        ServiceCollection services = new();
        services.AddLogging();
        DataProtectionSetup.Add(services);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void Add_FixesTheApplicationName()
    {
        using ServiceProvider provider = Build();

        Assert.Equal(DataProtectionSetup.ApplicationName, provider.GetRequiredService<IOptions<DataProtectionOptions>>().Value.ApplicationDiscriminator);
        Assert.Equal("Shopfolio", DataProtectionSetup.ApplicationName);
    }

    [Fact]
    public void Add_KeepsTheKeysInTheDatabaseInClearWithTheDefaultLifetime()
    {
        using ServiceProvider provider = Build();

        KeyManagementOptions options = provider.GetRequiredService<IOptions<KeyManagementOptions>>().Value;

        Assert.IsType<EntityFrameworkCoreXmlRepository<AppDbContext>>(options.XmlRepository);
        Assert.Null(options.XmlEncryptor);
        Assert.Equal(new KeyManagementOptions().NewKeyLifetime, options.NewKeyLifetime);
        Assert.Empty(options.KeyEscrowSinks);
        Assert.True(options.AutoGenerateKeys);
    }

    [Fact]
    public void Add_RegistersTheProtectedValuesOnceAsASingleton()
    {
        using ServiceProvider provider = Build();

        Assert.Same(provider.GetRequiredService<ProtectedValues>(), provider.GetRequiredService<ProtectedValues>());
    }
}
