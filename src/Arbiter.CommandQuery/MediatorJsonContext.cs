using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Dispatcher;
using Arbiter.CommandQuery.Models;

namespace Arbiter.CommandQuery;

[JsonSerializable(typeof(DispatchRequest))]
[JsonSerializable(typeof(ProblemDetails))]
public partial class MediatorJsonContext : JsonSerializerContext;
