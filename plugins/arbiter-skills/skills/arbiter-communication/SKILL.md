---
name: arbiter-communication
description: Use when sending templated email or SMS via Arbiter.Communication — AddEmailServices / AddSmsServices, IEmailDeliveryService / ISmsDeliveryService, template resolvers, or wiring a delivery provider like Azure Communication Services, Microsoft Graph, SendGrid, Twilio, or SMTP.
---

# Arbiter.Communication

Templated email + SMS abstraction with pluggable delivery providers. Templates resolve from embedded resources (or any custom `ITemplateResolver`), then are rendered with model substitution and handed off to the configured delivery service.

## Install

```bash
dotnet add package Arbiter.Communication              # core (templates + abstractions)
```

Then one or more providers:

```bash
dotnet add package Arbiter.Communication.Azure        # Azure Communication Services (email + SMS)
dotnet add package Arbiter.Communication.Graph        # Microsoft Graph (email)
dotnet add package Arbiter.Communication.Twilio       # SendGrid (email) + Twilio (SMS)
```

SMTP delivery is built into the core package.

## Register — email

```csharp
using Arbiter.Communication;
using Arbiter.Communication.Azure;
using Arbiter.Communication.Twilio;
using Arbiter.Communication.Graph;

services.AddTemplateServices();
services.AddTemplateResourceResolver<MyAssemblyMarker>(
    resourceNameFormat: "MyApp.Templates.{0}.html");

services.AddEmailServices(options =>
{
    options.FromAddress = "noreply@example.com";
    options.FromName    = "Example App";
});

// Pick ONE delivery provider:
services.AddSmtpEmailDeliver();                                         // built-in SMTP
services.AddAzureEmailDeliver("AzureCommunicationConnectionString");    // Azure Communication
services.AddGraphEmailDeliver();                                        // Microsoft Graph
services.AddSendGridEmailDeliver("SendGrid:ApiKey");                    // SendGrid
```

## Register — SMS

```csharp
services.AddSmsServices(options =>
{
    options.FromNumber = "+15555550100";
});

// Pick ONE provider:
services.AddAzureSmsDeliver("AzureCommunicationConnectionString");
services.AddTwilioSmsDeliver(accountSID: "AC…", authenticationToken: "…");
```

## Sending

```csharp
public class WelcomeEmailSender
{
    private readonly IEmailTemplateService _email;
    public WelcomeEmailSender(IEmailTemplateService email) => _email = email;

    public Task SendAsync(User user, CancellationToken ct) =>
        _email.SendAsync(
            templateName: "Welcome",
            model: new { user.FirstName, user.Email },
            to: user.Email,
            cancellationToken: ct);
}
```

```csharp
public class OtpSender
{
    private readonly ISmsTemplateService _sms;
    public OtpSender(ISmsTemplateService sms) => _sms = sms;

    public Task SendAsync(string phone, string code, CancellationToken ct) =>
        _sms.SendAsync(templateName: "Otp", model: new { Code = code }, to: phone, cancellationToken: ct);
}
```

## Template resolution

Templates are arbitrary strings (HTML for email, text for SMS) located by an `ITemplateResolver`. The built-in `EmbeddedResourceTemplateResolver` is wired via:

```csharp
services.AddTemplateResourceResolver<MyAssemblyMarker>("MyApp.Templates.{0}.html");
// {0} is the template name; mark the file as <EmbeddedResource> in the .csproj
```

Multiple resolvers can be registered; lower `priority` wins. Implement `ITemplateResolver` for filesystem, DB, or remote sources.

## Notes

- All `AddXxxDeliver` overloads accept an `Action<EmailConfiguration>` / `Action<SmsConfiguration>` to override the from address/number per provider.
- For testing, register `services.AddEmailDelivery<NullEmailDeliveryService>()` (or your own fake).

## Reference

- Overview: https://github.com/loresoft/Arbiter/blob/main/docs/guide/communication/overview.md
- Templates: https://github.com/loresoft/Arbiter/blob/main/docs/guide/communication/templates.md
- Azure: https://github.com/loresoft/Arbiter/blob/main/docs/guide/communication/azure.md
- Graph: https://github.com/loresoft/Arbiter/blob/main/docs/guide/communication/graph.md
- Twilio/SendGrid: https://github.com/loresoft/Arbiter/blob/main/docs/guide/communication/twilio.md
