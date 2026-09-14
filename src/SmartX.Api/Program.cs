using SmartX.Api.Models;
using SmartX.Api.Services;
using SmartX.Shared;
using SmartX.Shared.Dtos;

var builder = WebApplication.CreateBuilder(args);

// ---- Services ----
builder.Services.AddSingleton<SensorRegistry>();
builder.Services.AddSingleton<TelemetryIngestionService>();
builder.Services.AddSingleton<DeviceHierarchyValidator>();
builder.Services.AddSingleton<FileStorageService>();

builder.Services.AddCors(options =>
{
    // Permissive for local development only: the Blazor WebAssembly client
    // runs on a different port to the API, so it needs an explicit CORS
    // policy to call it at all.
    options.AddPolicy("SmartXClient", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("SmartXClient");

app.MapGet("/", () => Results.Ok(new { service = "Smart-X Ingestion Gateway", status = "online" }));

// =========================================================================
// Sensor registration and payload management
// =========================================================================

app.MapPost("/api/sensors/register", (SensorRegistrationRequest request, SensorRegistry registry) =>
{
    if (string.IsNullOrWhiteSpace(request.DeviceMac))
    {
        return Results.BadRequest(new { error = "DeviceMac is required." });
    }

    if (string.IsNullOrWhiteSpace(request.DeploymentLocation))
    {
        return Results.BadRequest(new { error = "DeploymentLocation is required." });
    }

    var registration = registry.Register(new SensorRegistration
    {
        DeviceMac = request.DeviceMac,
        DeploymentLocation = request.DeploymentLocation,
        Category = request.Category
    });

    return Results.Created($"/api/sensors/{registration.DeviceMac}", ToResponse(registration));
});

app.MapGet("/api/sensors", (SensorRegistry registry) =>
    Results.Ok(registry.GetAll().Select(ToResponse)));

app.MapGet("/api/sensors/{mac}", (string mac, SensorRegistry registry) =>
    registry.TryGet(mac, out var reg) && reg is not null
        ? Results.Ok(ToResponse(reg))
        : Results.NotFound(new { error = $"No sensor registered with MAC '{mac}'." }));

// Media / log attachment: multipart file upload against a registered sensor.
app.MapPost("/api/sensors/{mac}/upload", async (string mac, IFormFile file, SensorRegistry registry, FileStorageService storage) =>
{
    if (!registry.TryGet(mac, out var registration) || registration is null)
    {
        return Results.NotFound(new { error = $"No sensor registered with MAC '{mac}'." });
    }

    if (file.Length == 0)
    {
        return Results.BadRequest(new { error = "The uploaded file is empty." });
    }

    var relativePath = await storage.SaveAsync(mac, file);
    registration.AttachedFiles.Add(relativePath);

    return Results.Ok(new { savedAs = relativePath });
});

// =========================================================================
// Telemetry ingestion (Generics: TelemetryPacket<T>) and history queries
// =========================================================================

app.MapPost("/api/telemetry/ingest", (TelemetryIngestRequest request, TelemetryIngestionService ingestion, SensorRegistry registry) =>
{
    if (!registry.Exists(request.DeviceMac))
    {
        return Results.NotFound(new { error = $"Device '{request.DeviceMac}' is not registered." });
    }

    switch (request.ValueType.Trim().ToLowerInvariant())
    {
        case "float":
            if (request.FloatValue is null)
            {
                return Results.BadRequest(new { error = "FloatValue is required when ValueType is 'float'." });
            }
            ingestion.Ingest(new TelemetryPacket<float>(request.DeviceMac, SensorCategory.Environmental, request.FloatValue.Value));
            break;

        case "int":
            if (request.IntValue is null)
            {
                return Results.BadRequest(new { error = "IntValue is required when ValueType is 'int'." });
            }
            ingestion.Ingest(new TelemetryPacket<int>(request.DeviceMac, SensorCategory.PowerConsumption, request.IntValue.Value));
            break;

        case "bool":
            if (request.BoolValue is null)
            {
                return Results.BadRequest(new { error = "BoolValue is required when ValueType is 'bool'." });
            }
            ingestion.Ingest(new TelemetryPacket<bool>(request.DeviceMac, SensorCategory.Actuator, request.BoolValue.Value));
            break;

        default:
            return Results.BadRequest(new { error = "ValueType must be one of 'float', 'int' or 'bool'." });
    }

    return Results.Ok(new { status = "ingested" });
});

app.MapGet("/api/telemetry/{mac}/history", (string mac, TelemetryIngestionService ingestion) =>
{
    var response = new TelemetryHistoryResponse(
        Floats: ingestion.GetFloatHistory(mac).Select(p => new TelemetrySample(p.Value, p.Timestamp)).ToList(),
        Ints: ingestion.GetIntHistory(mac).Select(p => new TelemetrySample(p.Value, p.Timestamp)).ToList(),
        Bools: ingestion.GetBoolHistory(mac).Select(p => new TelemetrySample(p.Value ? 1 : 0, p.Timestamp)).ToList(),
        OptimisedHistory: ingestion.GetBatchBuffer(mac)?.OptimisedHistory.ToList() ?? new List<double>());

    return Results.Ok(response);
});

// =========================================================================
// Operator overloading demo: aggregate / delta-compare two meters
// =========================================================================

app.MapPost("/api/telemetry/aggregate", (MeterAggregationRequest request) =>
{
    var meterA = new MeterReading(request.DeviceMacA, request.WattsA);
    var meterB = new MeterReading(request.DeviceMacB, request.WattsB);

    var aggregate = meterA + meterB; // operator+ : combined virtual load
    var delta = meterA - meterB;     // operator- : delta for anomaly checks
    var aIsHigher = meterA > meterB; // operator> : direct comparison

    return Results.Ok(new MeterAggregationResponse(aggregate.Watts, delta.Watts, aIsHigher));
});

// =========================================================================
// Recursion demo: nested device deployment hierarchy validation
// =========================================================================

app.MapPost("/api/devices/validate-hierarchy", (HierarchyValidationRequest request, DeviceHierarchyValidator validator) =>
{
    var (isValid, errors) = validator.Validate(request.Root);
    return Results.Ok(new HierarchyValidationResponse(isValid, errors));
});

app.Run();

static SensorRegistrationResponse ToResponse(SensorRegistration registration) => new(
    registration.DeviceMac,
    registration.DeploymentLocation,
    registration.Category,
    registration.RegisteredAt,
    registration.AttachedFiles);
