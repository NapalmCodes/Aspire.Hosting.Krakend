using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;

namespace NapalmCodes.Aspire.Hosting.Krakend.Tests;

public class AddKrakendTests
{
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
