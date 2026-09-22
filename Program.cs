using StromligningApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<StromligningService>(client =>
{
    client.BaseAddress = new Uri("https://stromligning.dk/");
});

var app = builder.Build();

Console.WriteLine("=== APPLICATION STARTED ===");
app.Logger.LogInformation("StromligningApp starting in {EnvironmentName} environment.", app.Environment.EnvironmentName);

app.Use(async (context, next) =>
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    Console.WriteLine($"=== CONSOLE REQUEST: {context.Request.Method} {context.Request.Path} ===");
    app.Logger.LogInformation("Request started: {Method} {Path}.", context.Request.Method, context.Request.Path);

    try
    {
        await next();
    }
    catch (Exception exception)
    {
        app.Logger.LogError(exception, "Request failed: {Method} {Path}.", context.Request.Method, context.Request.Path);
        throw;
    }
    finally
    {
        stopwatch.Stop();
        app.Logger.LogInformation(
            "Request finished: {Method} {Path} returned {StatusCode} in {ElapsedMilliseconds} ms.",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
});

app.UseStaticFiles();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
