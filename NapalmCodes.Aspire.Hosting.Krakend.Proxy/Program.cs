var builder = WebApplication.CreateBuilder(args);

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
