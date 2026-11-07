namespace Shopfolio.Endpoints.Files;

/// <summary>The two file endpoint groups of 09 15.7: the download (UC-37) and the chunked upload (UC-83), mapped together.</summary>
internal static class FileEndpoints
{
    public static IEndpointRouteBuilder MapFileEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDownloadEndpoint();
        endpoints.MapUploadEndpoints();
        return endpoints;
    }
}
