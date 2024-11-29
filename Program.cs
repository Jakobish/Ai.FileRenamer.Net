using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FileRenamerProject.Data;
using Microsoft.EntityFrameworkCore;
using FileRenamerProject;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register the FileDbContext as a scoped service
builder.Services.AddDbContext<FileDbContext>(options =>
    options.UseSqlite("Data Source=files.db"));

await builder.Build().RunAsync();