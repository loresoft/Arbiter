using System.Text.Json.Serialization;

namespace Arbiter.Functions;

/// <summary>
/// Provides source-generated JSON serialization metadata for Azure Functions client models.
/// </summary>
[JsonSerializable(typeof(FunctionStatus))]
[JsonSerializable(typeof(FunctionDefinition))]
[JsonSerializable(typeof(IReadOnlyList<FunctionDefinition>))]
[JsonSerializable(typeof(FunctionBinding))]
[JsonSerializable(typeof(FunctionConfiguration))]
[JsonSerializable(typeof(FunctionTrigger))]
public partial class FunctionContext : JsonSerializerContext;
