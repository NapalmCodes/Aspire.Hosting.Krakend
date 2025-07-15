![Aspire Krakend Logo](https://raw.githubusercontent.com/NapalmCodes/Aspire.Hosting.Krakend/main/images/aspire-krakend-logo.png) 

# Aspire.Hosting.Krakend
Aspire hosting component for the high performance KrakenD (https://www.krakend.io/) API Gateway.

## Overview
The project provides a [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/) component around the official Community Edition (OSS version) of the KrakenD API Gateway Docker [container](https://hub.docker.com/r/devopsfaith/krakend).
The current latest version of the container is found [here](https://github.com/NapalmCodes/Aspire.Hosting.Krakend/blob/main/NapalmCodes.Aspire.Hosting.Krakend/KrakendContainerImageTags.cs#L7). Utilization of the component allows developers to work with KrakenD locally and/or configure for a production deployment using [Azure Container Apps](https://learn.microsoft.com/en-us/azure/container-apps/overview) / [Azure Developer CLI](https://www.google.com/url?sa=t&source=web&rct=j&opi=89978449&url=https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/&ved=2ahUKEwjjn9iV046IAxUCMlkFHV7VCdEQFnoECCkQAQ&usg=AOvVaw0Y5N7vJDfjU4Osk3zCXMmB) or [Aspirate](https://github.com/prom3theu5/aspirational-manifests) / [Kubernetes](https://kubernetes.io/). 

## Use

In a .NET Aspire solution locate the `*.AppHost.csproj` project. Install the nuget package with the following command:

`dotnet add package NapalmCodes.Aspire.Hosting.Krakend`

In the `Program.cs` file import the package with: 

```csharp
using NapalmCodes.Aspire.Hosting.Krakend;
```

Using the provided extension methods the KrakenD component can be added to the solution as follows:

```csharp
// Configuration path is used to create a bind mount to copy local `krakend.json` config
// to the container.
// https://www.krakend.io/docs/configuration/
// https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/persist-data-volumes

var krakend = builder.AddKrakend("gateway", "./config/krakend", port: 8080) 
    .WithExternalHttpEndpoints()
    .WithEnvironment("FC_OUT", "/tmp/krakend.json"); // Optional: Helpful for troubleshooting flexible config issues
                                                     // (may want a bind mount for easy local access while debugging).
```

### Sidecar Service Discovery

The above is technically all that is required to work with the component. However, KrakenD is not Service Discovery aware by default preventing direct use of friendly identifiers (i.e.: `http://apiservice`)
as host names.

To facilitate the development experience a sidecar proxy can be enabled. The proxy is implemented with [YARP](https://microsoft.github.io/reverse-proxy/). It is designed to use a bind mount for setting the JSON-based configuration for your specific solution in the container. While there is nothing preventing you from deploying this proxy, it really is unnecessary when running in an environment where DNS-based service discovery (i.e.: Kubernetes DNS SRV) is available. The proxy only bridges a need for local service discovery done with environment variables by .NET Aspire. You can exclude the proxy from the manifest by using the
`excludeFromManifest` toggle on `.WithProxy()`. The sidecar is also instrumented with OpenTelemetry using the same extensions
found in a `*.ServiceDefaults` project within a .NET Aspire solution.

```csharp
krakend.WithProxy(configurationPath: "./config/proxy/yarp.json");
```

For example configuration, visit the YARP documentation linked above or check out the example in this repo.

### Open Telemetry

Using flexible configuration for KrakenD we are able to utilize the OTLP Collector hosted by .NET Aspire and discovered
through the use of environment variables. Please see the example project for the `krakend.json` configuration enabling OTEL metrics to be transmitted to the dashboard from KrakenD.

## Known Issues

Currently `http` is the best way to utilize this component locally due to complexity surrounding the trusting of self-signed certificate/certificate authorities in Docker containers. This situation is likely to evolve in future iterations of .NET Aspire.
A conversation started by yours truly can be found [here](https://github.com/dotnet/aspire/discussions/5221). This is brought up as the consumer might want to provide OpenTelemetry metrics from KrakenD to the Aspire dashboard. A `krakend.json` config snippet
has been provided in the example to assist with this. However, given the KrakenD container does not trust [dotnet dev-certs](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-dev-certs) you must run the Aspire dashboard/solution in `http` mode. There has been some effort by the .NET Aspire team in .NET 9 to enable trusting of dev certs in Linux containers. This will need to be explored.
