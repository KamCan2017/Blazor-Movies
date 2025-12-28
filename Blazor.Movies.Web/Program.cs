using Blazor.Movies.Web.Components;
using Blazor.Movies.Web.Services;
using Blazor.Movies.Web.StateObservers;
using Blazor.Movies.Web.ViewModels.Scenario2;
using Radzen;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IEventAggregator, EventAggregator>();
builder.Services.AddScoped<IStateObserver, StateObserver>();
builder.Services.AddScoped<IStateManager, StateManager>();
builder.Services.AddScoped<IMovieProvider, MovieProvider>();
builder.Services.AddScoped<IWatchlistProvider, WatchlistProvider>();
/*builder.Services.AddScoped<MoviesGridViewModel>();
builder.Services.AddTransient<MovieCardViewModel>();
builder.Services.AddScoped<WatchListViewModel>();*/

builder.Services.AddScoped<MoviesGridViewModel2>();
builder.Services.AddTransient<MovieCardViewModel2>();
builder.Services.AddScoped<WatchListViewModel2>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRadzenComponents();

builder.Host.UseSerilog((context, services, configuration) => configuration
    // Read configuration from appsettings.json (including Serilog section)
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    // Add default enrichers
    .Enrich.FromLogContext()
    .WriteTo.File(
        path: Path.Combine(AppContext.BaseDirectory, "logs", "log-.txt"),
        rollingInterval: RollingInterval.Day,
        outputTemplate:"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Debug()
    // Fallback to console if no sinks are configured
    .WriteTo.Console());


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Use Serilog's DiagnosticContext for richer logging of requests in development
    app.UseDeveloperExceptionPage();
    app.UseSerilogRequestLogging();
}

else // Configure the HTTP request pipeline.
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();



app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();