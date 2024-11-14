using Aspire.Hosting.ApplicationModel;

namespace NapalmCodes.Aspire.Hosting.Krakend.Proxy;

/// <summary>
/// A resource that represents a service discovery aware proxy for KrakenD to utilize
/// independent of the hosting model.
/// </summary>
/// <param name="name">Resource name.</param>
public class ProxyResource(string name) :
    ContainerResource(name),
    IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "http";

    private EndpointReference? _primaryEndpoint;

    /// <summary>
    /// Gets the primary endpoint for the KrakenD proxy.
    /// </summary>
    public EndpointReference PrimaryEndpoint => _primaryEndpoint ??= new(this, PrimaryEndpointName);

    /// <summary>
    /// Gets the connection string expression for the KrakenD proxy.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create($"{PrimaryEndpoint.Property(EndpointProperty.Url)}");
}
