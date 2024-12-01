using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FileRenamerProject;
using FileRenamerProject.Services;
using FileRenamerProject.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Load configuration from multiple sources
var environment = builder.HostEnvironment.Environment;
var configFiles = new[] 
{
    "appsettings.json", 
    $"appsettings.{environment}.json"
};

foreach (var configFile in configFiles)
{
    try 
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
        using var configStream = await httpClient.GetStreamAsync(configFile);
        builder.Configuration.AddJsonStream(configStream);
    }
    catch 
    {
        // Ignore if file not found
        Console.WriteLine($"Configuration file {configFile} not found.");
    }
}

// Add custom configuration from environment-like sources if needed
var customConfig = new Dictionary<string, string>
{
    // Add any environment-like configuration here
    // {"FILERENAMER_SOME_SETTING", "SomeValue"}
};

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddSingleton<INameSuggestionCache, NameSuggestionCache>();

// Configure in-memory database for Blazor WebAssembly
builder.Services.AddDbContext<FileDbContext>(options =>
    options.UseInMemoryDatabase("FileRenamerDb"));

await builder.Build().RunAsync();