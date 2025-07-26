using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Dispatcher;
using Arbiter.CommandQuery.Models;

namespace Arbiter.CommandQuery;

/// <summary>
/// Provides a <see cref="JsonSerializerContext"/> for source generation of JSON serialization metadata
/// for types used in the Arbiter Command/Query pipeline.
/// </summary>
/// <remarks>
/// This context enables efficient serialization and deserialization of <see cref="DispatchRequest"/>
/// and <see cref="ProblemDetails"/> types in Blazor WebAssembly and other .NET applications.
/// </remarks>
[JsonSerializable(typeof(DispatchRequest))]
[JsonSerializable(typeof(ProblemDetails))]
public partial class MediatorJsonContext : JsonSerializerContext;
