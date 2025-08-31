# Communication Overview

Arbiter.Communication provides a powerful and flexible messaging system for sending templated emails and SMS messages. The library consists of three main packages:

- **Arbiter.Communication** - Core functionality for templates and message delivery
- **Arbiter.Communication.Azure** - Azure Communication Services integration
- **Arbiter.Communication.Twilio** - Twilio and SendGrid integration

## Key Features

- **Template-based messaging** using Fluid templating engine
- **Email delivery** with HTML and text support
- **SMS delivery** with multiple provider support
- **Embedded resource templates** for packaging templates with your application
- **Dependency injection integration** with .NET Core
- **Multiple delivery providers** (SMTP, Azure Communication Services, Twilio, SendGrid)
- **Configuration-based setup** using `appsettings.json`

## Core Concepts

### Templates

Templates are YAML files that define the structure of your messages. They use the [Fluid](https://github.com/sebastienros/fluid) templating engine for dynamic content.

**Email Template Example:**

```yaml
subject: Welcome{% if ProductName != blank %} to {{ ProductName }}{% endif %}

textBody: |
    Hello {{ UserName }},
    
    Welcome to our platform! Your account has been successfully created.
    
    Best regards,
    The Team

htmlBody: |
    <!DOCTYPE html>
    <html>
    <head>
        <title>Welcome</title>
    </head>
    <body>
        <h1>Hello {{ UserName }},</h1>
        <p>Welcome to our platform! Your account has been successfully created.</p>
        <p>Best regards,<br>The Team</p>
    </body>
    </html>
```

**SMS Template Example:**

```yaml
message: Your verification code{% if ProductName != blank %} for {{ ProductName }}{% endif %} is {{ Code }}.
```

### Service Interfaces

The library provides clean abstractions for different messaging scenarios:

- `IEmailDeliveryService` - Send individual email messages
- `IEmailTemplateService` - Send templated emails with model binding
- `ISmsDeliveryService` - Send individual SMS messages  
- `ISmsTemplateService` - Send templated SMS messages with model binding
- `ITemplateService` - Resolve and process templates

## Installation

### Core Package

Install the base communication package:

```bash
dotnet add package Arbiter.Communication
```

### Provider Packages

Choose the appropriate provider package(s):

```bash
# For Azure Communication Services
dotnet add package Arbiter.Communication.Azure

# For Twilio SMS and SendGrid Email
dotnet add package Arbiter.Communication.Twilio
```

## Basic Configuration

### Email Configuration

Configure email settings in `appsettings.json`:

```json
{
  "Email": {
    "FromName": "My Application",
    "FromAddress": "noreply@myapp.com",
    "Server": "smtp.gmail.com",
    "Port": 587,
    "UseSSL": true,
    "UserName": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

### SMS Configuration

Configure SMS settings in `appsettings.json`:

```json
{
  "Sms": {
    "SenderNumber": "+15551234567",
    "UserName": "your-twilio-sid",
    "Password": "your-twilio-token"
  }
}
```

## Service Registration

### Basic SMTP Email

```csharp
services.AddSmtpEmailDeliver();
```

### Memory-based Testing

For development and testing:

```csharp
services.AddEmailDelivery<MemoryEmailDeliverService>();
services.AddSmsDelivery<MemorySmsDeliverService>();
```

### Template Resources

Register embedded template resources:

```csharp
services.AddTemplateResourceResolver<Program>("MyApp.Templates.{0}.yaml");
```

This will look for templates in embedded resources with names like:

- `MyApp.Templates.welcome-email.yaml`
- `MyApp.Templates.verification-sms.yaml`

## Basic Usage

### Sending Templated Emails

```csharp
public class UserService
{
    private readonly IEmailTemplateService _emailService;
    
    public UserService(IEmailTemplateService emailService)
    {
        _emailService = emailService;
    }
    
    public async Task SendWelcomeEmail(User user)
    {
        var model = new 
        {
            UserName = user.Name,
            ProductName = "My Application"
        };
        
        var recipients = new EmailRecipients([user.Email]);
        
        var result = await _emailService.Send(
            "welcome-email", 
            model, 
            recipients);
            
        if (!result.IsSuccess)
        {
            // Handle error
            throw new InvalidOperationException(result.ErrorMessage);
        }
    }
}
```

### Sending Direct Emails

```csharp
public class EmailService
{
    private readonly IEmailDeliveryService _deliveryService;
    
    public EmailService(IEmailDeliveryService deliveryService)
    {
        _deliveryService = deliveryService;
    }
    
    public async Task SendDirectEmail()
    {
        var message = new EmailMessage
        {
            Senders = new EmailSenders("noreply@myapp.com", "My App"),
            Recipients = new EmailRecipients(["user@example.com"]),
            Content = new EmailContent
            {
                Subject = "Direct Email",
                HtmlBody = "<h1>Hello World!</h1>",
                TextBody = "Hello World!"
            }
        };
        
        var result = await _deliveryService.Send(message);
    }
}
```

### Sending Templated SMS

```csharp
public class SmsService
{
    private readonly ISmsTemplateService _smsService;
    
    public SmsService(ISmsTemplateService smsService)
    {
        _smsService = smsService;
    }
    
    public async Task SendVerificationCode(string phoneNumber, string code)
    {
        var model = new 
        {
            Code = code,
            ProductName = "My Application"
        };
        
        var result = await _smsService.Send(
            "verification-code", 
            model, 
            phoneNumber);
            
        if (!result.IsSuccess)
        {
            // Handle error
            throw new InvalidOperationException(result.ErrorMessage);
        }
    }
}
```

## Advanced Configuration

### Custom Email Senders

Override default sender information per email:

```csharp
var senders = new EmailSenders(
    from: "support@myapp.com", 
    fromName: "Support Team",
    replyTo: "noreply@myapp.com");

var result = await _emailService.Send(
    "support-response", 
    model, 
    recipients, 
    senders);
```

### Email Recipients with CC and BCC

```csharp
var recipients = new EmailRecipients(
    to: ["user@example.com"],
    cc: ["manager@example.com"],
    bcc: ["audit@example.com"]);
```

### Email Attachments

```csharp
var attachments = new List<EmailAttachment>
{
    new("report.pdf", pdfBytes, "application/pdf"),
    new("data.csv", csvBytes, "text/csv")
};

var message = new EmailMessage
{
    // ... other properties
    Attachments = attachments
};
```

### Multiple Template Resolvers

Register multiple template sources with priorities:

```csharp
// Lower priority values are processed first
services.AddTemplateResourceResolver<Program>("MyApp.Templates.{0}.yaml", priority: 100);
services.AddTemplateResourceResolver<PluginAssembly>("Plugin.Templates.{0}.yaml", priority: 200);
```

### Configuration Override

Override configuration at registration time:

```csharp
services.AddSmtpEmailDeliver(options =>
{
    options.FromAddress = "override@example.com";
    options.FromName = "Override Name";
});
```

## Error Handling

All send operations return result objects that indicate success or failure:

```csharp
var result = await _emailService.Send(templateName, model, recipients);

if (result.Successful)
{
    // Email sent successfully
    Console.WriteLine($"Email sent successfully: {result.Message}");
}
else
{
    // Handle error
    Console.WriteLine($"Failed to send email: {result.Message}");
    
    if (result.Exception != null)
    {
        // Log the exception for debugging
        _logger.LogError(result.Exception, "Email send failed");
    }
}
```

## Template Development

### Creating Templates

1. Create YAML files for your templates
2. Embed them as resources in your project
3. Register the template resolver

**Project file configuration:**

```xml
<ItemGroup>
  <EmbeddedResource Include="Templates\*.yaml" />
</ItemGroup>
```

### Template Testing

Use the memory delivery services for testing:

```csharp
[Test]
public async Task TestEmailTemplate()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddEmailDelivery<MemoryEmailDeliverService>();
    services.AddTemplateResourceResolver<TestClass>("TestApp.Templates.{0}.yaml");
    
    var provider = services.BuildServiceProvider();
    var emailService = provider.GetRequiredService<IEmailTemplateService>();
    var memoryService = provider.GetRequiredService<IEmailDeliveryService>() as MemoryEmailDeliverService;
    
    // Act
    var result = await emailService.Send("test-template", model, recipients);
    
    // Assert
    Assert.True(result.Success);
    Assert.Single(memoryService.Messages);
}
```

## Next Steps

- [Azure Communication Services Setup](communication/azure.md)
- [Twilio and SendGrid Setup](communication/twilio.md)
- [Template Best Practices](communication/templates.md)
