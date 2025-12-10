using BlazorApp1.Components;
using BlazorApp1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ============================================================================
// SERVICE REGISTRATION - CRITICAL FOR DATA PERSISTENCE
// ============================================================================

// FileReaderService: Reads CSV, JSON, TXT files (factory pattern)
// Singleton = One instance for entire app (doesn't matter much, stateless)
builder.Services.AddSingleton<FileReaderService>();

// HttpClient: For downloading images from URLs
// Singleton = One instance, reused for all requests (efficient)
builder.Services.AddSingleton<HttpClient>();

// ImageHandlingService: Detects image URLs and columns
// Singleton = Consistent image detection logic across app
builder.Services.AddSingleton<ImageHandlingService>();

// DataManagementService: STORES ALL YOUR DATA IN MEMORY
// ⭐ SINGLETON IS CRITICAL HERE ⭐
// Why? When you navigate from /json-edit-item back to /json-data:
//   - Scoped: New instance created = Lost data (BIG PROBLEM)
//   - Singleton: Same instance reused = Data persists ✅
// This is why your edits are saved even after page navigation!
builder.Services.AddSingleton<DataManagementService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
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
