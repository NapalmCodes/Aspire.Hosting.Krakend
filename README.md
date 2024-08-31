![Aspire Krakend Logo](https://raw.githubusercontent.com/NapalmCodes/Aspire.Hosting.Krakend/main/images/aspire-krakend-logo.png) 

# Aspire.Hosting.Krakend
Aspire hosting component for the high performance KrakenD (https://www.krakend.io/) API Gateway.

## Overview
The project provides a lightweight [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/) component around the official Community Edition (OSS version) of the KrakenD API Gateway Docker [container](https://hub.docker.com/r/devopsfaith/krakend).
The current latest version of the container is found [here](https://github.com/NapalmCodes/Aspire.Hosting.Krakend/blob/c4266b82e968de8ef3aedb05d347cfab74a68e8b/NapalmCodes.Aspire.Hosting.Krakend/KrakendContainerImageTags.cs#L7). Utilization of the component allows developers to work with KrakenD locally and/or configure for a production deployment using [Azure Container Apps](https://learn.microsoft.com/en-us/azure/container-apps/overview) and [Azure Developer CLI](https://www.google.com/url?sa=t&source=web&rct=j&opi=89978449&url=https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/&ved=2ahUKEwjjn9iV046IAxUCMlkFHV7VCdEQFnoECCkQAQ&usg=AOvVaw0Y5N7vJDfjU4Osk3zCXMmB) or [Aspirate](https://github.com/prom3theu5/aspirational-manifests) and [Kubernetes](https://kubernetes.io/). 

## Use

In a .NET Aspire solution locate the `*.AppHost.csproj` project. Install the nuget package with the following command:

`dotnet add package NapalmCodes.Aspire.Hosting.Krakend`

In the `Program.cs` file import the package with: 

```csharp
using NapalmCodes.Aspire.Hosting.Krakend;
```

Using the provided extension methods the KrakenD component can be added to the solution as follows:

```csharp
var krakend = builder.AddKrakend("gateway", port: 61373)
    .WithExternalHttpEndpoints() // Optional: external endpoint if desired
    .WithConfigBindMount("./config/krakend")
    .WithEnvironment("FC_ENABLE", "1") // Optional: enable Flexible Configuration (https://www.krakend.io/docs/configuration/flexible-config/)
    .WithEnvironment("FC_OUT", "/etc/krakend/result.json") // Optional: Location to output flexible configuration results (i.e.: what is actually used to run KrakenD);
```

*Note*: The `.WithConfigBindMount()` invocation should provide the path to the `krakend.json` file used for configuring the API Gateway. Instructions on configuring the API Gateway
can be found at the official documentation page [here](https://www.krakend.io/docs/configuration/).

## Known Issues

Currently `http` is the best way to utilize this component locally due to complexity surrounding the trusting of self-signed certificate/certificate authorities in Docker containers. This situation is likely to evolve in future iterations of .NET Aspire.
A conversation started by yours truly can be found [here](https://github.com/dotnet/aspire/discussions/5221). This is brought up as the consumer might want to provide OpenTelemetry metrics from KrakenD to the Aspire dashboard. A `krakend.json` config snippet
has been provided below to assist in this. However, given the KrakenD container does not trust [dotnet dev-certs](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-dev-certs) you must run the Aspire dashboard/solution in `http` mode.

The configuration snippet below will configure KrakenD to use Aspire environment variables
for providing OpenTelemetry traces and metrics to the Aspire Dashboard. `FC_ENABLE` environment variable set to `1`
is required to insert environment variable values into configuration. Details can be found at the official documentation
for this configuration [here](https://www.krakend.io/docs/telemetry/opentelemetry/).

```json
  "extra_config": {
    "telemetry/opentelemetry": {
      "service_name": "gateway",
      "service_version": "0.1",
      "skip_paths": [""],
      "metric_reporting_period": 30,
      "exporters": {
        "otlp": [
          {
            "name": "aspire_dashboard",
            "host": "{{ (split ":" (splitList "://" (env "OTEL_EXPORTER_OTLP_ENDPOINT") | last))._0 }}",
            "port": {{ int ((split ":" (splitList "://" (env "OTEL_EXPORTER_OTLP_ENDPOINT") | last))._1) }},
            "use_http": false,
            "disable_metrics": false,
            "disable_traces": false
          }
        ]
      },
      "layers": {
        "global": {
          "disable_metrics": false,
          "disable_traces": false,
          "disable_propagation": false
        },
        "proxy": {
          "disable_metrics": false,
          "disable_traces": false
        },
        "backend": {
          "metrics": {
            "disable_stage": false,
            "round_trip": false,
            "read_payload": false,
            "detailed_connection": false,
            "static_attributes": []
          },
          "traces": {
            "disable_stage": false,
            "round_trip": false,
            "read_payload": false,
            "detailed_connection": false,
            "static_attributes": []
          }
        }
      }
    }
```
