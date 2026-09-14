namespace SmartX.Shared.Dtos;

/// <summary>Wraps the root of a device deployment tree for recursive validation.</summary>
public record HierarchyValidationRequest(DeviceNode Root);

public record HierarchyValidationResponse(bool IsValid, List<string> Errors);
