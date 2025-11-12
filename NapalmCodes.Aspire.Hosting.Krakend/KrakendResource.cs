using Aspire.Hosting.ApplicationModel;

namespace NapalmCodes.Aspire.Hosting.Krakend;

/// <summary>
/// A resource that represents a KrakenD resource independent of the hosting model.
/// </summary>
/// <param name="name">Resource name.</param>
public class KrakendResource(string name) :
    ContainerResource(name),
    IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "http";

    /// <summary>
    /// Gets the primary endpoint for the KrakenD server.
    /// </summary>
    private EndpointReference PrimaryEndpoint => field ??= new(this, PrimaryEndpointName);

    /// <summary>
    /// Gets the connection string expression for the KrakenD server.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create($"{PrimaryEndpoint.Property(EndpointProperty.Url)}");
}
