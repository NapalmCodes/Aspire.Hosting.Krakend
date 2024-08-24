using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace NapalmCodes.Aspire.Hosting.Krakend;

/// <summary>
/// KrakenD Builder Extensions.
/// </summary>
public static class KrakendBuilderExtensions
{
    // The path within the container in which KrakenD stores the configuration
    // file(s).
    const string KrakendContainerConfigDirectory = "/etc/krakend";

    /// <summary>
    /// Adds a KrakenD server to the application model. A container is used for local development.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name to give the resource.</param>
    /// <param name="port">The host port for the KrakenD server.</param>
    /// <returns></returns>
    public static IResourceBuilder<KrakendResource> AddKrakend(
        this IDistributedApplicationBuilder builder,
        string name,
        int? port = null)
    {
        var krakendResource = new KrakendResource(name);

        var resourceBuilder = builder.AddResource(krakendResource)
            .WithHttpEndpoint(
                port: port,
                name: KrakendResource.PrimaryEndpointName,
                targetPort: 8080)
            .WithImage(KrakendContainerImageTags.Image, Krakend.KrakendContainerImageTags.Tag)
            .WithImageRegistry(KrakendContainerImageTags.Registry)
            .WithOtlpExporter();

        return resourceBuilder;
    }

    /// <summary>
    /// Adds a bind mount for the configuration folder to a KrakenD container resource.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="source"></param>
    /// <param name="isReadOnly"></param>
    /// <returns></returns>
    public static IResourceBuilder<KrakendResource> WithConfigBindMount(
        this IResourceBuilder<KrakendResource> builder, string source, bool isReadOnly = false) =>
            builder.WithBindMount(source, KrakendContainerConfigDirectory, isReadOnly);
}