var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpLogging(options =>
{
});

var revereseProxyBuilder = builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// https://github.com/dotnet/aspire/issues/4605
if (!builder.Environment.IsProduction())
{
    revereseProxyBuilder.AddServiceDiscoveryDestinationResolver();
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

app.UseHttpLogging();

app.UseRouting();

app.MapReverseProxy();

app.Run();
