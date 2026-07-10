---
name: arbiter-opentelemetry
description: Use when wiring OpenTelemetry tracing/metrics for Arbiter — adding the mediator ActivitySource and Meter (MediatorTelemetry.SourceName / MediatorTelemetry.MeterName), calling Arbiter.OpenTelemetry.Server.AddOpenTelemetry on a HostApplicationBuilder, or exposing log queries via Arbiter.OpenTelemetry.Monitor.AddLogQuery.
---

# Arbiter OpenTelemetry

Three packages, layered:

| Package | Purpose |
| --- | --- |
| `Arbiter.OpenTelemetry`        | Shared models + the `MediatorTelemetry` ActivitySource/Meter names |
| `Arbiter.OpenTelemetry.Server` | One-call `AddOpenTelemetry` extension for ASP.NET Core hosts, pre-wires the mediator source |
| `Arbiter.OpenTelemetry.Monitor`| `AddLogQuery` — exposes telemetry log queries via the CQRS pipeline |

## Manual wiring (any host)

```bash
dotnet add package Arbiter.Mediation
dotnet add package OpenTelemetry.Extensions.Hosting
```

```csharp
using Arbiter.Mediation;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

services.AddOpenTelemetry()
    .WithTracing(t => t
        .AddSource(MediatorTelemetry.SourceName)
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(m => m
        .AddMeter(MediatorTelemetry.MeterName)
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter());
```

`MediatorTelemetry.SourceName` and `MediatorTelemetry.MeterName` are the only names you need — every Arbiter mediator activity/metric is emitted on them.

## One-call wiring (ASP.NET Core)

```bash
dotnet add package Arbiter.OpenTelemetry.Server
```

```csharp
using Arbiter.OpenTelemetry.Server;

var builder = WebApplication.CreateBuilder(args);

builder.AddOpenTelemetry(
    configureOptions:   opts    => opts.ServiceName = "MyService",
    configureMetrics:   metrics => metrics.AddRuntimeInstrumentation(),
    configureTracing:   tracing => tracing.AddHttpClientInstrumentation(),
    configureOpenTelemetry: ot  => ot.UseAzureMonitor() // optional
);
```

The extension auto-adds the mediator source/meter, ASP.NET Core instrumentation, and an exporter (OTLP by default; honor `OTEL_*` env vars).

## Log query API

```bash
dotnet add package Arbiter.OpenTelemetry.Monitor
```

```csharp
using Arbiter.OpenTelemetry.Monitor;

services.AddCommandQuery();
services.AddLogQuery();      // registers handlers for the log-query requests
```

Then send the log queries through `IMediator` / `IDispatcher` just like any other Arbiter query — useful for surfacing observability data inside an admin UI.

## Notes

- Don't add `MediatorTelemetry.SourceName` twice — the server extension already wires it.
- If you only need metrics or only traces, pass `null` to the unused configurator delegate.

## Reference

- Source: https://github.com/loresoft/Arbiter/tree/main/src/Arbiter.OpenTelemetry
- Server extension: https://github.com/loresoft/Arbiter/blob/main/src/Arbiter.OpenTelemetry.Server/HostApplicationBuilderExtensions.cs
