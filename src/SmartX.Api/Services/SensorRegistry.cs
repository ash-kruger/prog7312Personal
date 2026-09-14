using System.Collections.Concurrent;
using SmartX.Api.Models;

namespace SmartX.Api.Services;

/// <summary>
/// In-memory registry of every sensor known to the gateway, keyed by
/// device MAC address in a dictionary so a telemetry packet arriving for
/// an already-registered device resolves in O(1) rather than scanning a
/// list.
/// </summary>
public sealed class SensorRegistry
{
    private readonly ConcurrentDictionary<string, SensorRegistration> _sensors =
        new(StringComparer.OrdinalIgnoreCase);

    public SensorRegistration Register(SensorRegistration registration)
    {
        _sensors[registration.DeviceMac] = registration;
        return registration;
    }

    public bool TryGet(string deviceMac, out SensorRegistration? registration) =>
        _sensors.TryGetValue(deviceMac, out registration);

    public IEnumerable<SensorRegistration> GetAll() =>
        _sensors.Values.OrderBy(s => s.DeploymentLocation, StringComparer.OrdinalIgnoreCase);

    public bool Exists(string deviceMac) => _sensors.ContainsKey(deviceMac);
}
