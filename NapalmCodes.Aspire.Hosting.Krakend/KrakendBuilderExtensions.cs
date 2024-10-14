using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using NapalmCodes.Aspire.Hosting.Krakend.Proxy;

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
    /// <returns>The <see cref="IResourceBuilder{T}"/> for chaining.</returns>
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
    /// <param name="builder">The <see cref="IResourceBuilder{T}"/>.</param>
    /// <param name="source">The source path of the mount. This is the path to the file or directory on the host.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only mount.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/> for chaining.</returns>
    public static IResourceBuilder<KrakendResource> WithConfigBindMount(
        this IResourceBuilder<KrakendResource> builder, string source, bool isReadOnly = false) =>
            builder.WithBindMount(source, KrakendContainerConfigDirectory, isReadOnly);

    /// <summary>
    /// Adds a service discovery aware proxy as a sidecar to KrakenD facilitating routing
    /// to multiple replicas of an API or service.
    /// </summary>
    /// <param name="builder">The <see cref="IResourceBuilder{T}"/>.</param>
    /// <param name="name">The name to give the resource.</param>
    /// <param name="port">The host port for the KrakenD server.</param>
    /// <param name="excludeFromManifest">Excludes the proxy from being published to the manifest.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/> for chaining.</returns>
    public static IResourceBuilder<KrakendResource> WithProxy(
        this IResourceBuilder<KrakendResource> builder,
        string? name = null,
        int? port = null,
        bool excludeFromManifest = false)
    {
        name ??= $"{builder.Resource.Name}-proxy";

        var proxy = new ProxyResource(name);

        var resourceBuilder = builder.ApplicationBuilder.AddResource(proxy)
            .WithHttpEndpoint(
                port: port,
                name: "http",
                targetPort: 8081
            )
            .WithImage(ProxyContainerImageTags.Image, ProxyContainerImageTags.Tag)
            .WithImageRegistry(ProxyContainerImageTags.Registry)
            .WithOtlpExporter();

        if (excludeFromManifest)
        {
            resourceBuilder.ExcludeFromManifest();
        }

        return builder.WithReference(resourceBuilder);
    }
}