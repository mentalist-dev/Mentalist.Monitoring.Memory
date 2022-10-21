using Mentalist.Monitoring.Memory;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHostedService<MetricsHostedService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapGet("/status", () =>
    MetricsHostedServiceState.IsAlive() ? Results.StatusCode(200) : Results.StatusCode(500)
);

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapMetrics();
});

app.Run();
