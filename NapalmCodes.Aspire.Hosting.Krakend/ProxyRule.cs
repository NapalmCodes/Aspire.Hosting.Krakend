using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Yarp.ReverseProxy.LoadBalancing;

namespace NapalmCodes.Aspire.Hosting.Krakend;

/// <summary>
/// Rules for forwarding to a destination.
/// </summary>
public class ProxyRule
{
    /// <summary>
    /// Gets or sets the incoming path to match for
    /// forwarding to a set of destinations.
    /// </summary>
    public required string Path { get; set; }
    
    /// <summary>
    /// Gets or sets the service discovery aware destination to forward to.
    /// </summary>
    public required IResourceBuilder<IResourceWithServiceDiscovery> Destination { get; set; }

    /// <summary>
    /// Gets or sets the load balancing policy to use.
    /// Defaults to round-robin.
    /// </summary>
    public string LoadBalancingPolicy { get; set; } = LoadBalancingPolicies.RoundRobin;
}