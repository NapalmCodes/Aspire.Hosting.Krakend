using NapalmCodes.Aspire.Hosting.Krakend;

var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.BasicApiGateway_ApiService>("apiservice")
    .WithReplicas(2);

var krakend = builder.AddKrakend("gateway", "./config/krakend", port: 8080)
    .WithExternalHttpEndpoints()
    .WithEnvironment("FC_OUT", "/etc/krakend/krakend_results.json"); // Optional: Helpful for troubleshooting flexible config issues
    
krakend.WithProxy(configurationPath: "./config/proxy", port: 8081)
    .WithReference(apiService);

builder.AddProject<Projects.BasicApiGateway_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(krakend);

builder.Build().Run();
