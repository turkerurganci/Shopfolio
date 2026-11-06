using System.Text.Json;

using Shopfolio.Infrastructure.Binding;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The JSON of a machine endpoint's small body (T216 criterion 8; KK8; 07 2.9.6, 4.31.2; K-1171 (1)): a byte count is a JSON number both ways - written
/// as a number, and a number written as a text is not read - and the names are camelCase. That a machine model carries no date is the model rule
/// (<c>MachineDate</c>, <c>ModelBindingRuleTests</c>).
/// </summary>
public sealed class MachineJsonTests
{
    private sealed record UploadProgress(Guid Id, long ReceivedBytes, long TotalBytes);

    private sealed record UploadStart(string FileName, long SizeBytes);

    [Fact]
    public void Write_ByteCountsAreJsonNumbers_AndTheNamesCamelCase()
    {
        UploadProgress progress = new(Guid.Parse("0192f0c4-1111-7000-8000-000000000001"), 5_242_880L, 9_000_000_000L);

        string json = JsonSerializer.Serialize(progress, MachineJson.Options);

        using JsonDocument document = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Number, document.RootElement.GetProperty("receivedBytes").ValueKind);
        Assert.Equal(JsonValueKind.Number, document.RootElement.GetProperty("totalBytes").ValueKind);
        Assert.Equal(9_000_000_000L, document.RootElement.GetProperty("totalBytes").GetInt64());
        Assert.Contains("\"receivedBytes\":5242880", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Read_ANumber_IsRead_AndANumberWrittenAsAText_IsNot()
    {
        UploadStart start = JsonSerializer.Deserialize<UploadStart>("{\"fileName\":\"a.zip\",\"sizeBytes\":12}", MachineJson.Options)!;

        Assert.Equal(12L, start.SizeBytes);
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<UploadStart>("{\"fileName\":\"a.zip\",\"sizeBytes\":\"12\"}", MachineJson.Options));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<UploadStart>("{\"fileName\":\"a.zip\",\"sizeBytes\":12.5}", MachineJson.Options));
    }

    [Fact]
    public void Options_AreReadOnly_SoNoEndpointChangesThem()
    {
        Assert.True(MachineJson.Options.IsReadOnly);
        Assert.Throws<InvalidOperationException>(() => MachineJson.Options.WriteIndented = true);
    }
}
