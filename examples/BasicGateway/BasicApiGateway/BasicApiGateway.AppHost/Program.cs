using NapalmCodes.Aspire.Hosting.Krakend;

// Note: if you need more than one replica
// this doesn't work. Since KrakenD is not service
// discovery aware (via .NET HTTP Client) it cannot send traffic to
// the "apiservice" domain directly. A work around would be to use YARP. It
// is service discovery aware and can act as a proxy to multiple API Instances
// (ex: Gateway -> Proxy -> API Instances). I think this story
// in Aspire is evolving.

const int API_REPLICAS = 1;

var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.BasicApiGateway_ApiService>("apiservice")
    .WithReplicas(API_REPLICAS);

var krakend = builder.AddKrakend("gateway", port: 8080)
    .WithExternalHttpEndpoints()
    .WithConfigBindMount("./config/krakend")
    .WithEnvironment("FC_ENABLE", "1")
    .WithEnvironment("FC_OUT", "/etc/krakend/result.json")
    .WithEnvironment("APISERVICE_COUNT", API_REPLICAS.ToString())
    .WithReference(apiService);

builder.AddProject<Projects.BasicApiGateway_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(krakend);

builder.Build().Run();
