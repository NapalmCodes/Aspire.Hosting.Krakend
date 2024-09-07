using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Sockets;

namespace NapalmCodes.Aspire.Hosting.Krakend.Tests;

public class AddKrakendTests
{
    [Fact]
    public void AddKrakendContainerAddsAnnotationMetadata()
    {
        var appBuilder = DistributedApplication.CreateBuilder();

        appBuilder.AddKrakend("krakend");

        using var app = appBuilder.Build();

        var appModel = app.Services.GetRequiredService<DistributedApplicationModel>();

        var containerResource = Assert.Single(appModel.Resources.OfType<KrakendResource>());
        Assert.Equal("krakend", containerResource.Name);

        var endpoints = containerResource.Annotations.OfType<EndpointAnnotation>();
        Assert.Single(endpoints);

        var primaryEndpoint = Assert.Single(endpoints, e => e.Name == "http");
        Assert.Equal(8080, primaryEndpoint.TargetPort);
        Assert.False(primaryEndpoint.IsExternal);
        Assert.Equal("http", primaryEndpoint.Name);
        Assert.Null(primaryEndpoint.Port);
        Assert.Equal(ProtocolType.Tcp, primaryEndpoint.Protocol);
        Assert.Equal("http", primaryEndpoint.Transport);
        Assert.Equal("http", primaryEndpoint.UriScheme);

        var containerAnnotation = Assert.Single(containerResource.Annotations.OfType<ContainerImageAnnotation>());
        Assert.Equal(KrakendContainerImageTags.Tag, containerAnnotation.Tag);
        Assert.Equal(KrakendContainerImageTags.Image, containerAnnotation.Image);
        Assert.Equal(KrakendContainerImageTags.Registry, containerAnnotation.Registry);
    }

    [Fact]
    public async Task KrakendCreatesConnectionString()
    {
        var appBuilder = DistributedApplication.CreateBuilder();
        var krakend = appBuilder
            .AddKrakend("krakend")
            .WithEndpoint("http", e => e.AllocatedEndpoint = new AllocatedEndpoint(e, "localhost", 27000));

        using var app = appBuilder.Build();

        var appModel = app.Services.GetRequiredService<DistributedApplicationModel>();

        var connectionStringResource = Assert.Single(appModel.Resources.OfType<KrakendResource>()) as IResourceWithConnectionString;
        var connectionString = await connectionStringResource.GetConnectionStringAsync();

        Assert.Equal($"http://localhost:27000", connectionString);
        Assert.Equal("{krakend.bindings.http.url}", connectionStringResource.ConnectionStringExpression.ValueExpression);
    }
}
