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
    /// <param name="configurationPath">Path to KrakenD configuration for use in bind mount.</param>
    /// <param name="useFlexibleConfiguration">
    /// Enable flexible configuration (default true).
    /// See - https://www.krakend.io/docs/configuration/flexible-config
    /// </param>
    /// <param name="port">The host port for the KrakenD server.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/> for chaining.</returns>
    public static IResourceBuilder<KrakendResource> AddKrakend(
        this IDistributedApplicationBuilder builder,
        string name,
        string? configurationPath = null,
        bool useFlexibleConfiguration = true,
        int? port = null)
    {
        var krakendResource = new KrakendResource(name);

        var resourceBuilder = builder.AddResource(krakendResource)
            .WithHttpEndpoint(
                port: port,
                name: KrakendResource.PrimaryEndpointName,
                targetPort: 8080)
            .WithImage(KrakendContainerImageTags.Image, KrakendContainerImageTags.Tag)
            .WithImageRegistry(KrakendContainerImageTags.Registry)
            .WithOtlpExporter()
            .WithEnvironment("FC_ENABLE", useFlexibleConfiguration ? "1" : "0");

        if (!string.IsNullOrWhiteSpace(configurationPath))
        {
            resourceBuilder.WithConfigBindMount(configurationPath);
        }

        return resourceBuilder;
    }

    /// <summary>
    /// Adds a service discovery aware proxy as a sidecar to KrakenD facilitating routing
    /// to multiple replicas of an API or service.
    /// </summary>
    /// <param name="builder">The <see cref="IResourceBuilder{T}"/>.</param>
    /// <param name="name">The name to give the resource.</param>
    /// <param name="port">The host port for the KrakenD server.</param>
    /// <param name="excludeFromManifest">Excludes the proxy from being published to the manifest.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/> for chaining.</returns>
    public static IResourceBuilder<ProxyResource> WithProxy(
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
                name: ProxyResource.PrimaryEndpointName,
                targetPort: 8080
            )
            .WithImage(ProxyContainerImageTags.Image, ProxyContainerImageTags.Tag)
            // TODO: Re-enable after published to docker hub
            //.WithImageRegistry(ProxyContainerImageTags.Registry)
            .WithOtlpExporter();

        if (excludeFromManifest)
        {
            resourceBuilder.ExcludeFromManifest();
        }

        builder.WithEnvironment("KRAKEND_PROXY_URL", proxy.PrimaryEndpoint);
        
        // Service discovery (SD) is only enabled for the proxy in non-production environments. Production will likely have DNS SRV and
        // honestly probably doesn't even need the proxy. Should someone want it to deploy though we can do that and just turn SD off.
        resourceBuilder.WithEnvironment("ASPNETCORE_ENVIRONMENT",
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
        
        return resourceBuilder;
    }
    
    /// <summary>
    /// Adds a bind mount for the configuration folder to a KrakenD container resource.
    /// </summary>
    /// <param name="builder">The <see cref="IResourceBuilder{T}"/>.</param>
    /// <param name="source">The source path of the mount. This is the path to the file or directory on the host.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only mount.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/> for chaining.</returns>
    private static IResourceBuilder<KrakendResource> WithConfigBindMount(
        this IResourceBuilder<KrakendResource> builder, string source, bool isReadOnly = false) =>
        builder.WithBindMount(source, KrakendContainerConfigDirectory, isReadOnly);
}