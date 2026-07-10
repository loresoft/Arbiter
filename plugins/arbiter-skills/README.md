# Arbiter Skills

A Claude Code plugin that teaches Claude how to use the [Arbiter](https://github.com/loresoft/Arbiter) family of .NET libraries.

When installed, Claude auto-loads the relevant skill based on what you're asking about — e.g. asking *"how do I register an EF Core handler for my Product entity?"* loads `arbiter-commandquery-ef` and Claude responds with the canonical `AddEntityQueries` / `AddEntityCommands` pattern.

## Install

```bash
# Add this repo as a marketplace
/plugin marketplace add loresoft/arbiter

# Install the skills bundle
/plugin install arbiter-skills@arbiter
```

## Skills included

| Skill | When it loads |
| --- | --- |
| `arbiter-overview` | Routing skill — explains the Arbiter package landscape and points to the right specialist skill |
| `arbiter-mediation` | `IMediator`, `IRequest`, `IRequestHandler`, `INotification`, `IPipelineBehavior`, `AddMediator` |
| `arbiter-commandquery` | `EntityQuery`, `EntityFilter`, `FilterOperators`, `EntityPagedQuery`, `EntityIdentifierQuery`, behaviors |
| `arbiter-commandquery-ef` | Entity Framework Core handler registration (`AddEntityQueries`, `AddEntityCommands`) |
| `arbiter-commandquery-mongo` | MongoDB handler registration |
| `arbiter-endpoints` | Minimal API integration (`AddEndpointRoutes`, `MapEndpointRoutes`, `EntityCommandEndpointBase`) |
| `arbiter-mvc` | MVC controller integration |
| `arbiter-mapping` | Source-generated mapper (`[GenerateMapper]`, `MapperProfile`, `ProjectTo`) |
| `arbiter-dispatcher` | Blazor `IDispatcher`, WASM/Server/Auto setup, `ModelStateEditor` |
| `arbiter-communication` | Email/SMS templates + Azure / Graph / Twilio / SendGrid / SMTP providers |
| `arbiter-services` | CSV, AES encryption, cache tagging, token service, URL builder |
| `arbiter-opentelemetry` | Tracing/metrics with `MediatorTelemetry`, server + monitor packages |
| `arbiter-messaging-servicebus` | Azure Service Bus integration |

## Authoring conventions

Each skill is cheat-sheet style: a `description` frontmatter line that drives auto-loading, then a tight body with **When to use → Install → Register → Canonical pattern → Variations → Reference**. Keep skills under ~200 lines; link to `docs/guide/*.md` in the Arbiter repo for depth.
