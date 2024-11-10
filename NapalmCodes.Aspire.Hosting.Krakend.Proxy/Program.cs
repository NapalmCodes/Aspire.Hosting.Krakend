var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("/etc/proxy/yarp.json");

var reverseProxyBuilder = builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddServiceDiscovery();

// https://github.com/dotnet/aspire/issues/4605
if (!builder.Environment.IsProduction())
{
    reverseProxyBuilder.AddServiceDiscoveryDestinationResolver();
}

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        cpBuilder =>
        {
            cpBuilder.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseCors();

app.MapReverseProxy();

app.MapGet("/", () => "OK!");

app.Run();

//TODO: Instrument with OTEL
//TODO: Doc updates
//TODO: Build updates (publish image) and Renovate