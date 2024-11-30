using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FileRenamerProject;
using FileRenamerProject.Services;
using FileRenamerProject.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IPdfService, PdfService>();

// Configure in-memory database for Blazor WebAssembly
builder.Services.AddDbContext<FileDbContext>(options =>
    options.UseInMemoryDatabase("FileRenamerDb"));

await builder.Build().RunAsync();