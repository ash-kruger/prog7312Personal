using System.Net.Http.Json;
using SmartX.Shared.Dtos;

namespace SmartX.Client.Services;

/// <summary>
/// Thin wrapper around <see cref="HttpClient"/> that talks to the Smart-X
/// ingestion API. Keeping every endpoint call in one place means the
/// Razor pages stay focused on presentation rather than HTTP plumbing.
/// </summary>
public sealed class SmartXApiClient
{
    private readonly HttpClient _http;

    public SmartXApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<SensorRegistrationResponse>> GetSensorsAsync()
    {
        var result = await _http.GetFromJsonAsync<List<SensorRegistrationResponse>>("api/sensors");
        return result ?? new List<SensorRegistrationResponse>();
    }

    public async Task<(bool Success, string? Error)> RegisterSensorAsync(SensorRegistrationRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/sensors/register", request);
        return await ReadOutcomeAsync(response);
    }

    public async Task<(bool Success, string? Error)> IngestTelemetryAsync(TelemetryIngestRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/telemetry/ingest", request);
        return await ReadOutcomeAsync(response);
    }

    public async Task<TelemetryHistoryResponse?> GetHistoryAsync(string deviceMac) =>
        await _http.GetFromJsonAsync<TelemetryHistoryResponse>($"api/telemetry/{Uri.EscapeDataString(deviceMac)}/history");

    public async Task<(bool Success, string? Error)> UploadFileAsync(string deviceMac, MultipartFormDataContent content)
    {
        var response = await _http.PostAsync($"api/sensors/{Uri.EscapeDataString(deviceMac)}/upload", content);
        return await ReadOutcomeAsync(response);
    }

    public async Task<MeterAggregationResponse?> AggregateAsync(MeterAggregationRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/telemetry/aggregate", request);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<MeterAggregationResponse>()
            : null;
    }

    public async Task<HierarchyValidationResponse?> ValidateHierarchyAsync(HierarchyValidationRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/devices/validate-hierarchy", request);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<HierarchyValidationResponse>()
            : null;
    }

    private static async Task<(bool Success, string? Error)> ReadOutcomeAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var body = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrWhiteSpace(body) ? response.ReasonPhrase : body);
    }
}
