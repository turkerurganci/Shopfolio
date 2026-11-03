using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.FileSecrets;

namespace Shopfolio.IntegrationTests.FileSecrets;

public sealed class SecretFilesTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "shopfolio-secrets-" + Guid.NewGuid().ToString("N"));

    public SecretFilesTests()
    {
        Directory.CreateDirectory(_directory);
    }

    public void Dispose()
    {
        Directory.Delete(_directory, recursive: true);
    }

    private static NpgsqlConnectionStringBuilder Parse(string text) => new(text);

    [Theory]
    [InlineData("value\n", "value")]
    [InlineData("value\r\n", "value")]
    [InlineData("value", "value")]
    [InlineData("  spaced value \n", "  spaced value ")]
    [InlineData("two\n\n", "two\n")]
    public void Read_ReturnsTheContentWithoutItsClosingLineBreak(string content, string expected)
    {
        File.WriteAllText(Path.Combine(_directory, "a_secret"), content);

        Assert.Equal(expected, new SecretFiles(_directory).Read("a_secret"));
    }

    [Fact]
    public void Read_OfAFileThatIsNotMounted_ReturnsNull()
    {
        Assert.Null(new SecretFiles(_directory).Read("missing"));
    }

    [Fact]
    public void Read_OfAMissingDirectory_ReturnsNull()
    {
        Assert.Null(new SecretFiles(Path.Combine(_directory, "nowhere")).Read("a_secret"));
    }

    [Theory]
    [InlineData("../outside")]
    [InlineData("sub/file")]
    [InlineData("..")]
    [InlineData("")]
    public void Read_OfANameThatIsAPath_Throws(string name)
    {
        Assert.ThrowsAny<ArgumentException>(() => new SecretFiles(_directory).Read(name));
    }


    private IConfigurationRoot Configure(IEnumerable<KeyValuePair<string, string?>>? other = null)
    {
        ConfigurationBuilder builder = new();
        builder.AddInMemoryCollection(other ?? []);
        builder.Add(new SecretFileConfigurationSource(new SecretFiles(_directory)));
        return builder.Build();
    }

    [Fact]
    public void ConfigurationSource_WithAppConnectionFile_GivesTheWholeFileAsTheConnectionString()
    {
        File.WriteAllText(Path.Combine(_directory, SecretNames.DatabaseAppConnection), "Host=database;Database=shopfolio;Username=shopfolio_app;Password=made-up-pass-word\n");

        IConfigurationRoot configuration = Configure();

        string? value = configuration[DatabaseOptions.ConnectionStringKey];
        Assert.Equal("Host=database;Database=shopfolio;Username=shopfolio_app;Password=made-up-pass-word", value);
        Assert.Equal("shopfolio_app", Parse(value!).Username);
    }

    [Fact]
    public void ConfigurationSource_WithReaderConnectionFile_GivesTheReaderString()
    {
        File.WriteAllText(Path.Combine(_directory, SecretNames.DatabaseReaderConnection), "Host=database;Username=shopfolio_reader;Password=reader-made-up\n");

        Assert.Equal("Host=database;Username=shopfolio_reader;Password=reader-made-up", Configure()[DatabaseOptions.ConnectionStringKey]);
    }

    [Fact]
    public void ConfigurationSource_WithMountedFile_WinsOverTheOtherSources()
    {
        File.WriteAllText(Path.Combine(_directory, SecretNames.DatabaseAppConnection), "Host=from-the-file\n");

        IConfigurationRoot configuration = Configure([new(DatabaseOptions.ConnectionStringKey, "Host=from-the-setting")]);

        Assert.Equal("Host=from-the-file", configuration[DatabaseOptions.ConnectionStringKey]);
    }

    [Fact]
    public void ConfigurationSource_WithoutMountedFile_LeavesTheSettingToTheOtherSources()
    {
        IConfigurationRoot configuration = Configure([new(DatabaseOptions.ConnectionStringKey, "Host=localhost;Database=x;Username=somebody;Password=made-up")]);

        Assert.Equal("Host=localhost;Database=x;Username=somebody;Password=made-up", configuration[DatabaseOptions.ConnectionStringKey]);
        Assert.Null(Configure()[DatabaseOptions.ConnectionStringKey]);
    }

    [Fact]
    public void ConnectionStringFromTheFile_BindsIntoTheOptionsAndNoPasswordReachesTheValidatorMessage()
    {
        File.WriteAllText(Path.Combine(_directory, SecretNames.DatabaseAppConnection), "Host=database;Username=shopfolio_app;Password=made-up-never-print\n");
        DatabaseOptions options = new();
        Configure().GetSection(DatabaseOptions.SectionName).Bind(options);
        options.LivenessTimeoutSeconds = 0;

        ValidateOptionsResult result = new DatabaseOptionsValidator().Validate(null, options);

        Assert.Equal("shopfolio_app", Parse(options.ConnectionString).Username);
        Assert.True(result.Failed);
        Assert.DoesNotContain("made-up-never-print", string.Join(' ', result.Failures), StringComparison.Ordinal);
    }
}
