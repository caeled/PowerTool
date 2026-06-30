using PowellTecRelay;
using ToolsAvaloniaApp.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Run as a Windows Service when not in development
builder.Host.UseWindowsService(o => o.ServiceName = "PowellTecRelay");

// Bind to all interfaces so LAN devices can reach it
builder.WebHost.ConfigureKestrel(o =>
{
    o.ListenAnyIP(5000);
    o.Limits.MaxRequestBodySize = null; // no size cap — LAN only, ntadmin controls access
});

var app = builder.Build();

// ── API key middleware ────────────────────────────────────────────────────────
var apiKey = app.Configuration["Relay:ApiKey"] ?? "changeme";

app.Use(async (ctx, next) =>
{
    // GET requests serve the web UI — no key needed
    if (ctx.Request.Method == "GET")
    {
        await next();
        return;
    }
    if (!ctx.Request.Headers.TryGetValue("X-Api-Key", out var key) || key != apiKey)
    {
        ctx.Response.StatusCode = 401;
        await ctx.Response.WriteAsync("Unauthorized");
        return;
    }
    await next();
});

// ── Static web UI (wwwroot/index.html) ───────────────────────────────────────
app.UseDefaultFiles();
app.UseStaticFiles();

// ── POST /run ─────────────────────────────────────────────────────────────────
app.MapPost("/run", async (RunRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Script))
        return Results.BadRequest(new RunResponse("No script provided.", false));

    try
    {
        var output = await PsRunner.RunAsync(req.Script);
        return Results.Ok(new RunResponse(output, true));
    }
    catch (Exception ex)
    {
        return Results.Ok(new RunResponse($"Relay error: {ex.Message}", false));
    }
});

// ── POST /upload — multipart/form-data: fields "destPath" + file ─────────────
app.MapPost("/upload", async (HttpContext ctx) =>
{
    var sizeFeat = ctx.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpMaxRequestBodySizeFeature>();
    if (sizeFeat != null) sizeFeat.MaxRequestBodySize = null;
    try
    {
        var form     = await ctx.Request.ReadFormAsync(new Microsoft.AspNetCore.Http.Features.FormOptions
        {
            MultipartBodyLengthLimit = long.MaxValue,
            ValueLengthLimit         = int.MaxValue,
            MultipartHeadersLengthLimit = int.MaxValue
        });
        var destPath = form["destPath"].FirstOrDefault();
        var file     = form.Files.GetFile("file");
        if (string.IsNullOrWhiteSpace(destPath) || file == null)
            return Results.BadRequest(new RunResponse("destPath and file are required.", false));
        var dir = Path.GetDirectoryName(destPath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        await using var fs = File.Create(destPath);
        await file.CopyToAsync(fs);
        var mb = Math.Round(file.Length / 1024.0 / 1024.0, 1);
        return Results.Ok(new RunResponse($"Uploaded {file.FileName} ({mb} MB) → {destPath}", true));
    }
    catch (Exception ex)
    {
        return Results.Ok(new RunResponse($"Upload error: {ex.Message}", false));
    }
});

// ── GET /ping — health check ───────────────────────────────────────────────────
app.MapGet("/ping", () => Results.Ok(new { status = "ok", version = "1.0.0" }));

app.Run();

namespace PowellTecRelay
{
    record RunRequest(string Script);
    record RunResponse(string Output, bool Success);
}
