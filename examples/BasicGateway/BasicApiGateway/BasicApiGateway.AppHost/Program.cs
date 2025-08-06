using NapalmCodes.Aspire.Hosting.Krakend;

var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.BasicApiGateway_ApiService>("apiservice")
    .WithReplicas(2);

var krakend = builder.AddKrakend("gateway", "./config/krakend", port: 8080)
    .WithExternalHttpEndpoints()
    .WithEnvironment("FC_OUT", "/tmp/krakend.json"); // Optional: Helpful for troubleshooting flexible config issues
                                                          // Look at it in the running container or if you want to create a bind mount.
    
krakend.WithProxy([new ProxyRule{ Path = "/{**catch-all}", Destination = apiService }]);

builder.AddProject<Projects.BasicApiGateway_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(krakend);

builder.Build().Run();
