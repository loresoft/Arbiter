---
title: Twilio and SendGrid Integration
description: Integration with Twilio for SMS delivery and SendGrid for email delivery
---

# Twilio and SendGrid Integration

The `Arbiter.Communication.Twilio` package provides integration with Twilio for SMS delivery and SendGrid for email delivery. These are popular third-party communication services that offer reliable global messaging capabilities.

## Features

- **SendGrid Email Service** - Send emails through SendGrid's API
- **Twilio SMS Service** - Send SMS messages through Twilio's API
- **API key authentication** - Simple configuration using API keys
- **Global delivery** - Worldwide SMS and email delivery
- **Rich features** - Advanced email templates and SMS capabilities

## Installation

```bash
dotnet add package Arbiter.Communication.Twilio
```

## Prerequisites

### For SendGrid Email

1. **SendGrid Account**: Create an account at [SendGrid](https://sendgrid.com/)
2. **API Key**: Generate an API key with Mail Send permissions
3. **Sender Authentication**: Verify your sender identity (domain or single sender)

### For Twilio SMS

1. **Twilio Account**: Create an account at [Twilio](https://www.twilio.com/)
2. **Account SID and Auth Token**: Get credentials from Twilio Console
3. **Phone Number**: Purchase a Twilio phone number for sending SMS

## Configuration

### SendGrid Email Configuration

Add your SendGrid settings to `appsettings.json`:

```json
{
  "Email": {
    "FromAddress": "noreply@yourdomain.com",
    "FromName": "Your Application",
    "ServiceKey": "SG.your-sendgrid-api-key"
  }
}
```

### Twilio SMS Configuration

Add your Twilio settings to `appsettings.json`:

```json
{
  "Sms": {
    "SenderNumber": "+15551234567",
    "UserName": "your-twilio-account-sid",
    "Password": "your-twilio-auth-token"
  }
}
```

### Combined Configuration

```json
{
  "Email": {
    "FromAddress": "noreply@yourdomain.com", 
    "FromName": "Your Application",
    "ServiceKey": "SG.your-sendgrid-api-key"
  },
  "Sms": {
    "SenderNumber": "+15551234567",
    "UserName": "your-twilio-account-sid", 
    "Password": "your-twilio-auth-token"
  }
}
```

## Service Registration

### SendGrid Email Service

```csharp
using Arbiter.Communication.Twilio;

// Using configuration
services.AddSendGridEmailDeliver();

// Using direct API key
services.AddSendGridEmailDeliver("SG.your-api-key");

// With additional configuration
services.AddSendGridEmailDeliver(options =>
{
    options.ServiceKey = "SG.your-api-key";
    options.FromAddress = "support@yourdomain.com";
    options.FromName = "Support Team";
});
```

### Twilio SMS Service

```csharp
using Arbiter.Communication.Twilio;

// Using configuration
services.AddTwilioSmsDeliver();

// Using direct credentials
services.AddTwilioSmsDeliver("your-account-sid", "your-auth-token");

// With additional configuration
services.AddTwilioSmsDeliver(options =>
{
    options.UserName = "your-account-sid";
    options.Password = "your-auth-token";
    options.SenderNumber = "+15551234567";
});
```

### Combined Setup

```csharp
services.AddSendGridEmailDeliver();
services.AddTwilioSmsDeliver();
```

## Usage Examples

### Sending Emails via SendGrid

```csharp
public class EmailNotificationService
{
    private readonly IEmailTemplateService _emailService;
    
    public EmailNotificationService(IEmailTemplateService emailService)
    {
        _emailService = emailService;
    }
    
    public async Task SendOrderConfirmation(Order order)
    {
        var model = new 
        {
            CustomerName = order.CustomerName,
            OrderNumber = order.Id,
            OrderTotal = order.Total.ToString("C"),
            Items = order.Items.Select(i => new { 
                Name = i.ProductName, 
                Quantity = i.Quantity, 
                Price = i.Price.ToString("C") 
            })
        };
        
        var recipients = new EmailRecipients([order.CustomerEmail]);
        
        var result = await _emailService.Send("order-confirmation", model, recipients);
    }
}
```

### Sending SMS via Twilio

```csharp
public class SmsNotificationService
{
    private readonly ISmsTemplateService _smsService;
    
    public SmsNotificationService(ISmsTemplateService smsService)
    {
        _smsService = smsService;
    }
    
    public async Task SendDeliveryUpdate(string phoneNumber, string status, string estimatedTime)
    {
        var model = new 
        { 
            Status = status,
            EstimatedTime = estimatedTime,
            CompanyName = "Your Company"
        };
        
        var result = await _smsService.Send("delivery-update", model, phoneNumber);
    }
}
```

### Advanced Email Features with SendGrid

```csharp
public class AdvancedEmailService
{
    private readonly IEmailDeliveryService _emailDelivery;
    
    public AdvancedEmailService(IEmailDeliveryService emailDelivery)
    {
        _emailDelivery = emailDelivery;
    }
    
    public async Task SendEmailWithAttachments(string recipient, byte[] pdfReport)
    {
        var message = new EmailMessage
        {
            Senders = new EmailSenders("reports@yourdomain.com", "Report System"),
            Recipients = new EmailRecipients([recipient]),
            Content = new EmailContent
            {
                Subject = "Monthly Report",
                HtmlBody = "<h1>Please find your monthly report attached.</h1>",
                TextBody = "Please find your monthly report attached."
            },
            Attachments = new List<EmailAttachment>
            {
                new("monthly-report.pdf", pdfReport, "application/pdf")
            }
        };
        
        var result = await _emailDelivery.Send(message);
    }
}
```

### Twilio SMS with Custom Sender

```csharp
public class CustomSmsService
{
    private readonly ISmsDeliveryService _smsDelivery;
    
    public CustomSmsService(ISmsDeliveryService smsDelivery)
    {
        _smsDelivery = smsDelivery;
    }
    
    public async Task SendFromSpecificNumber(string recipient, string message, string fromNumber)
    {
        var smsMessage = new SmsMessage
        {
            From = fromNumber,
            To = recipient,
            Message = message
        };
        
        var result = await _smsDelivery.Send(smsMessage);
    }
}
```

## Template Examples

### Email Template for SendGrid

```yaml
subject: Order Confirmation #{{ OrderNumber }}

textBody: |
    Hi {{ CustomerName }},
    
    Thank you for your order! Here are the details:
    
    Order Number: {{ OrderNumber }}
    Total: {{ OrderTotal }}
    
    Items:
    {% for item in Items -%}
    - {{ item.Name }} ({{ item.Quantity }}x) - {{ item.Price }}
    {% endfor %}
    
    We'll send you tracking information once your order ships.
    
    Thanks for shopping with us!

htmlBody: |
    <!DOCTYPE html>
    <html>
    <head>
        <title>Order Confirmation</title>
        <style>
            .order-details { border: 1px solid #ddd; padding: 20px; margin: 20px 0; }
            .item-list { list-style: none; padding: 0; }
            .item { padding: 10px; border-bottom: 1px solid #eee; }
        </style>
    </head>
    <body>
        <h1>Order Confirmation #{{ OrderNumber }}</h1>
        <p>Hi {{ CustomerName }},</p>
        <p>Thank you for your order!</p>
        
        <div class="order-details">
            <h2>Order Details</h2>
            <p><strong>Order Number:</strong> {{ OrderNumber }}</p>
            <p><strong>Total:</strong> {{ OrderTotal }}</p>
            
            <h3>Items:</h3>
            <ul class="item-list">
            {% for item in Items -%}
                <li class="item">
                    <strong>{{ item.Name }}</strong><br>
                    Quantity: {{ item.Quantity }} - Price: {{ item.Price }}
                </li>
            {% endfor %}
            </ul>
        </div>
        
        <p>We'll send you tracking information once your order ships.</p>
        <p>Thanks for shopping with us!</p>
    </body>
    </html>
```

### SMS Template for Twilio

```yaml
message: |
    {{ CompanyName }} Update: Your delivery status is {{ Status }}.
    {% if EstimatedTime != blank -%}
    Estimated delivery: {{ EstimatedTime }}.
    {% endif -%}
    Track your order at: https://yoursite.com/track
```

## Best Practices

### SendGrid Email

1. **Sender Authentication**: Always verify your sending domain
2. **List Management**: Implement proper subscription/unsubscription handling
3. **Reputation Management**: Monitor sender reputation and engagement rates
4. **Template Design**: Use responsive email templates
5. **Testing**: Test emails across different clients and devices

```csharp
// Example: Handling unsubscribe links
public async Task SendMarketingEmail(string recipient, object model)
{
    // Add unsubscribe token to model
    var enhancedModel = new 
    {
        // Copy existing model properties
        ...model,
        UnsubscribeUrl = $"https://yoursite.com/unsubscribe?token={GenerateUnsubscribeToken(recipient)}"
    };
    
    var result = await _emailService.Send("marketing-template", enhancedModel, new EmailRecipients([recipient]));
}
```

### Twilio SMS

1. **Phone Number Formatting**: Use E.164 format (+1234567890)
2. **Message Length**: Keep messages under 160 characters when possible
3. **Compliance**: Follow SMS marketing regulations (TCPA, GDPR, etc.)
4. **Opt-out Handling**: Include STOP keywords and handle opt-outs
5. **Rate Limiting**: Respect carrier rate limits

```csharp
// Example: SMS with opt-out handling
public async Task SendMarketingSms(string phoneNumber, object model)
{
    var enhancedModel = new 
    {
        ...model,
        OptOutText = "Reply STOP to opt out"
    };
    
    var result = await _smsService.Send("marketing-sms", enhancedModel, phoneNumber);
}
```

## Development and Testing

### Using Sandbox Environments

#### SendGrid Development

```json
{
  "Email": {
    "ServiceKey": "SG.sandbox-api-key",
    "FromAddress": "test@yourdomain.com"
  }
}
```

#### Twilio Development

```json
{
  "Sms": {
    "UserName": "test-account-sid",
    "Password": "test-auth-token",
    "SenderNumber": "+15005550006"
  }
}
```

### Testing with Memory Services

For unit testing, use memory services:

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
    Assert.True(result.IsSuccess);
    Assert.Single(memoryService.Messages);
    Assert.Equal("Expected Subject", memoryService.Messages.First().Content.Subject);
}
```

## Monitoring and Analytics

### SendGrid Analytics

Monitor email performance through SendGrid dashboard:

- Delivery rates
- Open rates
- Click rates
- Bounce rates
- Unsubscribe rates

### Twilio Analytics

Monitor SMS performance through Twilio console:

- Delivery rates
- Error rates
- Response rates
- Cost analysis

## Pricing Considerations

### SendGrid Pricing

- Free tier: 100 emails/day
- Paid plans: Volume-based pricing
- Additional costs: Dedicated IP, advanced features

### Twilio Pricing

- SMS: Pay per message sent (varies by destination)
- Phone numbers: Monthly rental fees
- Additional costs: MMS, international messaging

## Migration Between Providers

### From SMTP to SendGrid

```csharp
// Before
services.AddSmtpEmailDeliver();

// After  
services.AddSendGridEmailDeliver();
```

## Troubleshooting

### Common SendGrid Issues

1. **Authentication Errors**: Check API key permissions
2. **Domain Not Verified**: Complete sender authentication
3. **Rate Limiting**: Implement exponential backoff
4. **Blocked Emails**: Check sender reputation

### Common Twilio Issues

1. **Invalid Phone Numbers**: Ensure E.164 format
2. **Account Suspended**: Check account status and billing
3. **Messaging Service Errors**: Verify phone number configuration
4. **Geographic Restrictions**: Check country-specific regulations

## Related Resources

- [SendGrid Documentation](https://docs.sendgrid.com/)
- [Twilio Documentation](https://www.twilio.com/docs)
- [SendGrid API Reference](https://docs.sendgrid.com/api-reference)
- [Twilio API Reference](https://www.twilio.com/docs/api)
