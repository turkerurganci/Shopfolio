using Microsoft.Extensions.Options;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.FileSecrets;

namespace Shopfolio.UnitTests.FileSecrets;

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

    [Fact]
    public void PostConfigure_WithAppUserAndMountedFile_PutsThePasswordIntoTheConnectionString()
    {
        File.WriteAllText(Path.Combine(_directory, SecretNames.DatabaseAppPassword), "made-up;pass=word\n");
        DatabaseOptions options = new() { ConnectionString = "Host=database;Database=shopfolio;Username=shopfolio_app" };

        new DatabasePasswordPostConfigure(new SecretFiles(_directory)).PostConfigure(null, options);

        Assert.Equal("made-up;pass=word", Parse(options.ConnectionString).Password);
        Assert.Equal("shopfolio_app", Parse(options.ConnectionString).Username);
        Assert.Equal("database", Parse(options.ConnectionString).Host);
    }

    [Fact]
    public void PostConfigure_WithReaderUser_ReadsTheReaderFileNotTheAppFile()
    {
        File.WriteAllText(Path.Combine(_directory, SecretNames.DatabaseAppPassword), "app-made-up\n");
        File.WriteAllText(Path.Combine(_directory, SecretNames.DatabaseReaderPassword), "reader-made-up\n");
        DatabaseOptions options = new() { ConnectionString = "Host=database;Database=shopfolio;Username=shopfolio_reader" };

        new DatabasePasswordPostConfigure(new SecretFiles(_directory)).PostConfigure(null, options);

        Assert.Equal("reader-made-up", Parse(options.ConnectionString).Password);
    }

    [Fact]
    public void PostConfigure_WithoutMountedFile_LeavesTheConnectionStringAsItIs()
    {
        const string complete = "Host=localhost;Database=shopfolio;Username=shopfolio_app;Password=from-the-setting";
        DatabaseOptions options = new() { ConnectionString = complete };

        new DatabasePasswordPostConfigure(new SecretFiles(_directory)).PostConfigure(null, options);

        Assert.Equal(complete, options.ConnectionString);
    }

    [Fact]
    public void PostConfigure_WithOtherUser_DoesNotReadAnyFile()
    {
        File.WriteAllText(Path.Combine(_directory, SecretNames.DatabaseAppPassword), "app-made-up\n");
        DatabaseOptions options = new() { ConnectionString = "Host=localhost;Database=x;Username=somebody" };

        new DatabasePasswordPostConfigure(new SecretFiles(_directory)).PostConfigure(null, options);

        Assert.Equal("Host=localhost;Database=x;Username=somebody", options.ConnectionString);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("this is not a connection string")]
    public void PostConfigure_WithMissingOrUnparsableConnectionString_LeavesItToTheValidator(string connectionString)
    {
        File.WriteAllText(Path.Combine(_directory, SecretNames.DatabaseAppPassword), "app-made-up\n");
        DatabaseOptions options = new() { ConnectionString = connectionString };

        new DatabasePasswordPostConfigure(new SecretFiles(_directory)).PostConfigure(null, options);

        Assert.Equal(connectionString, options.ConnectionString);
        Assert.True(new DatabaseOptionsValidator().Validate(null, options).Failed);
    }

    [Fact]
    public void ValidatorFailure_NeverPrintsThePasswordFromTheFile()
    {
        File.WriteAllText(Path.Combine(_directory, SecretNames.DatabaseAppPassword), "made-up-never-print\n");
        DatabaseOptions options = new() { ConnectionString = "Host=database;Username=shopfolio_app", LivenessTimeoutSeconds = 0 };

        new DatabasePasswordPostConfigure(new SecretFiles(_directory)).PostConfigure(null, options);
        ValidateOptionsResult result = new DatabaseOptionsValidator().Validate(null, options);

        Assert.True(result.Failed);
        Assert.DoesNotContain("made-up-never-print", string.Join(' ', result.Failures), StringComparison.Ordinal);
    }
}
