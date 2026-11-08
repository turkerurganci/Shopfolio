namespace Shopfolio.Infrastructure.Imaging;

/// <summary>Registers the image processor (09 15.6): the one registration the web host and the command mode share, so that a module that stands on it resolves in both.</summary>
internal static class ImagingRegistration
{
    public static IServiceCollection AddImageProcessing(this IServiceCollection services)
    {
        services.AddSingleton<ImageProcessor>();
        return services;
    }
}
