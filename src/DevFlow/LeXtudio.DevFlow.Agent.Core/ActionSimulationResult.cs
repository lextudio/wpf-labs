using System.Text.Json.Serialization;

namespace LeXtudio.DevFlow.Agent.Core;

public static class SimulationModes
{
    public const string Native = "native";
    public const string Semantic = "semantic";
    public const string Reflection = "reflection";
    public const string PropertyMutation = "property-mutation";
}

public sealed class ActionSimulationResult
{
    [JsonPropertyName("success")]
    public bool Success { get; init; } = true;

    [JsonPropertyName("simulationMode")]
    public string? SimulationMode { get; init; }

    [JsonPropertyName("elementId")]
    public string? ElementId { get; init; }

    [JsonPropertyName("key")]
    public string? Key { get; init; }

    [JsonPropertyName("text")]
    public string? Text { get; init; }

    [JsonPropertyName("deltaX")]
    public double? DeltaX { get; init; }

    [JsonPropertyName("deltaY")]
    public double? DeltaY { get; init; }
}
