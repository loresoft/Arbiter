using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Dispatcher;
using Arbiter.CommandQuery.Models;

namespace Arbiter.CommandQuery;

/// <summary>
/// Provides metadata about a set of types that is relevant to JSON serialization.
/// </summary>
[JsonSerializable(typeof(DispatchRequest))]
[JsonSerializable(typeof(ProblemDetails))]
public partial class MediatorJsonContext : JsonSerializerContext;
