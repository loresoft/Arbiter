---
title: Microsoft Graph Email Delivery
description: Integration with Microsoft Graph for email delivery through Microsoft Entra ID
---

# Microsoft Graph Email Delivery

The `Arbiter.Communication.Graph` package provides email delivery through Microsoft Graph. It uses Microsoft Entra ID app-only authentication to send messages from a Microsoft 365 mailbox while keeping the standard Arbiter email template and delivery interfaces.

## Features

- **Microsoft Graph email service** - Send emails through the Microsoft Graph `sendMail` API
- **Microsoft Entra ID authentication** - Authenticate with tenant ID, client ID, and client secret
- **Template support** - Use the same `IEmailTemplateService` workflow as other Arbiter email providers
- **Rich email messages** - Send HTML or text bodies with To, Cc, Bcc, reply-to, and attachments
- **Recipient override** - Redirect outbound email during development and testing
- **Configuration binding** - Resolve email settings from `appsettings.json`, user secrets, or another configuration provider

## Installation

```bash
dotnet add package Arbiter.Communication.Graph
```

## Prerequisites

1. **Microsoft 365 mailbox**: The configured `FromAddress` must be an Exchange Online mailbox that can send email.
2. **Microsoft Entra tenant**: Use the tenant where the sending mailbox is hosted.
3. **App registration**: Create an app registration for the application that sends email.
4. **Client secret**: Create a client secret for app-only authentication.
5. **Graph permission**: Grant the Microsoft Graph `Mail.Send` application permission.
6. **Admin consent**: An administrator must grant consent for the application permission.

## Microsoft Entra ID Setup

### Create the App Registration

1. Open the Microsoft Entra admin center.
2. Go to **Identity** > **Applications** > **App registrations**.
3. Select **New registration**.
4. Enter a name for the app, such as `Arbiter Email Sender`.
5. Choose the supported account type for your organization.
6. Select **Register**.
7. Copy the **Directory (tenant) ID** and **Application (client) ID** values.

### Create a Client Secret

1. Open the app registration.
2. Go to **Certificates & secrets**.
3. Select **New client secret**.
4. Choose an expiration that matches your rotation policy.
5. Copy the secret value immediately. This value is shown only once.

### Grant Microsoft Graph Permissions

1. Open the app registration.
2. Go to **API permissions**.
3. Select **Add a permission**.
4. Choose **Microsoft Graph**.
5. Choose **Application permissions**.
6. Add the `Mail.Send` permission.
7. Select **Grant admin consent** for the tenant.

### Verify the Sender Mailbox

The `FromAddress` value is used as the Microsoft Graph user when sending mail. Make sure this address belongs to an active mailbox in the tenant and can send messages.

## Configuration

Add Microsoft Graph email settings to `appsettings.json`:

```json
{
  "Email": {
    "TenantId": "00000000-0000-0000-0000-000000000000",
    "ClientId": "11111111-1111-1111-1111-111111111111",
    "ServiceKey": "your-client-secret",
    "FromAddress": "noreply@yourdomain.com",
    "FromName": "Your Application"
  }
}
```

The client secret can be configured with either `ServiceKey` or `Password`. `ServiceKey` is recommended for Microsoft Graph configuration.

### Recipient Override

Use `RecipientOverride` to redirect all outbound email to a single address during development or test runs:

```json
{
  "Email": {
    "TenantId": "00000000-0000-0000-0000-000000000000",
    "ClientId": "11111111-1111-1111-1111-111111111111",
    "ServiceKey": "your-client-secret",
    "FromAddress": "noreply@yourdomain.com",
    "RecipientOverride": "developer@yourdomain.com"
  }
}
```

### Local Development with User Secrets

For local development, store the client secret outside of source control:

```bash
dotnet user-secrets init
dotnet user-secrets set "Email:TenantId" "<tenant-id>"
dotnet user-secrets set "Email:ClientId" "<client-id>"
dotnet user-secrets set "Email:ServiceKey" "<client-secret>"
dotnet user-secrets set "Email:FromAddress" "<graph-sender-address>"
```

## Service Registration

`AddGraphEmailDeliver` registers Microsoft Graph as the email delivery service. Authentication and sender settings are read from the `Email` configuration section.

```csharp
using Arbiter.Communication.Graph;

// Using Email configuration
services.AddGraphEmailDeliver();

// With additional email configuration
services.AddGraphEmailDeliver(options =>
{
    options.FromAddress = "support@yourdomain.com";
    options.FromName = "Support Team";
});
```

Register template resources when sending templated messages:

```csharp
services
    .AddTemplateResourceResolver<Program>("MyApp.Templates.{0}.yaml")
    .AddGraphEmailDeliver();
```

## Usage Examples

### Sending Templated Emails

Once configured, use the standard email template interface:

```csharp
public class AccountNotificationService
{
    private readonly IEmailTemplateService _emailService;

    public AccountNotificationService(IEmailTemplateService emailService)
    {
        _emailService = emailService;
    }

    public async Task SendPasswordResetEmail(User user, string resetLink)
    {
        var model = new
        {
            UserName = user.Name,
            Link = resetLink,
            ProductName = "Your Application"
        };

        var recipients = new EmailRecipients([user.Email]);

        var result = await _emailService.Send("password-reset", model, recipients);
    }
}
```

### Sending Direct Emails

For more control, use `IEmailDeliveryService` directly:

```csharp
public class DirectEmailService
{
    private readonly IEmailDeliveryService _emailDelivery;

    public DirectEmailService(IEmailDeliveryService emailDelivery)
    {
        _emailDelivery = emailDelivery;
    }

    public async Task SendDirectEmail()
    {
        var message = new EmailMessage
        {
            Senders = new EmailSenders("noreply@yourdomain.com", "Your Application"),
            Recipients = new EmailRecipients(["user@example.com"]),
            Content = new EmailContent
            {
                Subject = "Hello from Microsoft Graph",
                HtmlBody = "<h1>Hello from Microsoft Graph!</h1>",
                TextBody = "Hello from Microsoft Graph!"
            }
        };

        var result = await _emailDelivery.Send(message);
    }
}
```

### Sending Attachments

```csharp
var message = new EmailMessage
{
    Senders = new EmailSenders("reports@yourdomain.com", "Report System"),
    Recipients = new EmailRecipients(["user@example.com"]),
    Content = new EmailContent
    {
        Subject = "Monthly Report",
        HtmlBody = "<p>Your monthly report is attached.</p>",
        TextBody = "Your monthly report is attached."
    },
    Attachments =
    [
        new EmailAttachment("monthly-report.pdf", "application/pdf", pdfReport)
    ]
};

var result = await emailDelivery.Send(message);
```

## Troubleshooting

- **Missing FromAddress**: Set `Email:FromAddress`. Microsoft Graph uses this value as the sender mailbox.
- **Access denied or insufficient privileges**: Confirm the app registration has the Microsoft Graph `Mail.Send` application permission and admin consent has been granted.
- **Mailbox not found**: Verify `FromAddress` is an active Exchange Online mailbox in the configured tenant.
- **Invalid client secret**: Confirm `Email:ServiceKey` or `Email:Password` contains the current client secret value.
- **Messages go to the wrong recipient**: Check whether `Email:RecipientOverride` is configured.
