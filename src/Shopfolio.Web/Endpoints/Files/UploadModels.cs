using System.Text.Json.Serialization;

using Shopfolio.Infrastructure.Binding;

namespace Shopfolio.Endpoints.Files;

/// <summary>The body of the start of a chunked upload (07 5.4.2; 07 4.31.2): the name the buyer will see and the size the whole file will have. Checked by the module, not here.</summary>
public sealed class StartUploadRequest : IMachineRequest
{
    public string? FileName { get; set; }

    public long SizeBytes { get; set; }
}

/// <summary>
/// The answer of the start (201), of a chunk (200 and, with the bytes received so far, the 409 of a chunk that does not fit) and of the status request (200)
/// (07 5.4.2). <see cref="ChunkSizeBytes"/> is written by the start only.
/// </summary>
public sealed class UploadStatusResponse : IMachineResponse
{
    public Guid Id { get; set; }

    [SpecAssignedName]
    public long ReceivedBytes { get; set; }

    [SpecAssignedName]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? ChunkSizeBytes { get; set; }

    [SpecAssignedName]
    public bool IsCompleted { get; set; }
}

/// <summary>The 422 of the start (07 5.4.2): P-26's value, so the script can tell the two reasons apart - a size above it is the file's size, otherwise the disk's threshold.</summary>
public sealed class UploadRejectedResponse : IMachineResponse
{
    [SpecAssignedName]
    public long MaxSizeBytes { get; set; }
}
