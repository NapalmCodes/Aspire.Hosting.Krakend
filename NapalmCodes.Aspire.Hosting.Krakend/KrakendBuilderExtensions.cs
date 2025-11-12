using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Yarp;

namespace NapalmCodes.Aspire.Hosting.Krakend;

/// <summary>
/// KrakenD Builder Extensions.
/// </summary>
public static class KrakendBuilderExtensions
{
    // The path within the container in which KrakenD stores the configuration
    // file(s).
    private const string KrakendContainerConfigDirectory = "/etc/krakend";

    /// <summary>
    /// Adds a KrakenD server to the application model.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name to give the resource.</param>
    /// <param name="configurationPath">
    /// Path to KrakenD configuration for use in bind mount.
    /// Expecting `krakend.json` as the filename.
    /// </param>
    /// <param name="useFlexibleConfiguration">
    /// Enable flexible configuration (default true).
    /// See - https://www.krakend.io/docs/configuration/flexible-config
    /// </param>
    /// <param name="trustDeveloperCertificates">Whether to trust developer certificates (default true).</param>
    /// <param name="port">The host port for the KrakenD server.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/> for chaining.</returns>
    public static IResourceBuilder<KrakendResource> AddKrakend(
        this IDistributedApplicationBuilder builder,
        string name,
        string? configurationPath = null,
        bool useFlexibleConfiguration = true,
        bool trustDeveloperCertificates = true,
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
            .WithEnvironment("FC_ENABLE", useFlexibleConfiguration ? "1" : "0")
            .WithDeveloperCertificateTrust(trust: trustDeveloperCertificates);

        if (!string.IsNullOrWhiteSpace(configurationPath))
        {
            resourceBuilder.WithConfigBindMount(configurationPath);
        }

        return resourceBuilder;
    }

    /// <summary>
    /// Adds a service discovery aware proxy as a "sidecar" to KrakenD facilitating routing
    /// to service discovery aware destinations.
    /// </summary>
    /// <param name="builder">The <see cref="IResourceBuilder{T}"/>.</param>
    /// <param name="rules">The rules for configuring proxy forwarding to various destinations.</param>
    /// <param name="name">The name to give the resource.</param>
    /// <param name="excludeFromManifest">Excludes the proxy from being published to the manifest.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/> for chaining.</returns>
    public static IResourceBuilder<YarpResource> WithProxy(
        this IResourceBuilder<KrakendResource> builder,
        IEnumerable<ProxyRule> rules,
        string? name = null,
        bool excludeFromManifest = false)
    {
        name ??= $"{builder.Resource.Name}-proxy";

        var proxy = builder.ApplicationBuilder.AddYarp(name)
            .WithConfiguration(yarpBuilder =>
            {
                foreach (var rule in rules)
                {
                    var cluster = yarpBuilder.AddCluster(rule.Destination)
                        .WithLoadBalancingPolicy(rule.LoadBalancingPolicy);
                    
                    yarpBuilder.AddRoute(rule.Path, cluster);    
                }
            })
            .WithParentRelationship(builder)
            .WithOtlpExporter();
        
        if(excludeFromManifest)
        {
            proxy.ExcludeFromManifest();   
        }
        
        builder.WithEnvironment("KRAKEND_PROXY_URL", proxy.GetEndpoint("http"));
        
        return proxy;
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
