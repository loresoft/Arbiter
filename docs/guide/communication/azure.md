# Azure Communication Services

The `Arbiter.Communication.Azure` package provides integration with Azure Communication Services for both email and SMS delivery. This is Microsoft's cloud-based communication platform that offers reliable, scalable messaging services.

## Features

- **Azure Email Service** - Send emails through Azure Communication Services
- **Azure SMS Service** - Send SMS messages through Azure Communication Services  
- **Connection string support** - Simple configuration using Azure connection strings
- **Configuration binding** - Automatic resolution from `appsettings.json`

## Installation

```bash
dotnet add package Arbiter.Communication.Azure
```

## Prerequisites

1. **Azure Communication Services Resource**: Create an Azure Communication Services resource in the Azure portal
2. **Email Domain**: For email services, you need to provision an email domain
3. **Phone Number**: For SMS services, you need to provision a phone number
4. **Connection String**: Get the connection string from your Azure Communication Services resource

## Configuration

### Connection String Setup

Add your Azure Communication Services connection string to `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "AzureCommunicationServices": "endpoint=https://your-resource.communication.azure.com/;accesskey=your-access-key"
  },
  "Email": {
    "FromAddress": "noreply@your-domain.azurecomm.net",
    "FromName": "Your Application"
  },
  "Sms": {
    "SenderNumber": "+1234567890"
  }
}
```

### Alternative Configuration

You can also store the connection string directly in configuration:

```json
{
  "AzureCommunicationServices": "endpoint=https://your-resource.communication.azure.com/;accesskey=your-access-key",
  "Email": {
    "FromAddress": "noreply@your-domain.azurecomm.net",
    "FromName": "Your Application"
  }
}
```

## Service Registration

### Email Service

```csharp
using Arbiter.Communication.Azure;

// Using connection string name
services.AddAzureEmailDeliver("AzureCommunicationServices");

// Using direct connection string
services.AddAzureEmailDeliver("endpoint=https://your-resource.communication.azure.com/;accesskey=your-key");

// With additional email configuration
services.AddAzureEmailDeliver("AzureCommunicationServices", options =>
{
    options.FromAddress = "support@your-domain.azurecomm.net";
    options.FromName = "Support Team";
});
```

### SMS Service

```csharp
using Arbiter.Communication.Azure;

// Using connection string name
services.AddAzureSmsDeliver("AzureCommunicationServices");

// Using direct connection string  
services.AddAzureSmsDeliver("endpoint=https://your-resource.communication.azure.com/;accesskey=your-key");

// With additional SMS configuration
services.AddAzureSmsDeliver("AzureCommunicationServices", options =>
{
    options.SenderNumber = "+1234567890";
});
```

### Combined Setup

```csharp
services.AddAzureEmailDeliver("AzureCommunicationServices");
services.AddAzureSmsDeliver("AzureCommunicationServices");
```

## Usage Examples

### Sending Emails

Once configured, use the standard email interfaces:

```csharp
public class NotificationService
{
    private readonly IEmailTemplateService _emailService;
    
    public NotificationService(IEmailTemplateService emailService)
    {
        _emailService = emailService;
    }
    
    public async Task SendWelcomeEmail(User user)
    {
        var model = new 
        {
            UserName = user.Name,
            ActivationLink = $"https://myapp.com/activate/{user.Id}"
        };
        
        var recipients = new EmailRecipients([user.Email]);
        
        var result = await _emailService.Send("welcome-email", model, recipients);
        
        if (!result.Success)
        {
            throw new InvalidOperationException($"Failed to send email: {result.ErrorMessage}");
        }
    }
}
```

### Sending SMS Messages

```csharp
public class SmsNotificationService
{
    private readonly ISmsTemplateService _smsService;
    
    public SmsNotificationService(ISmsTemplateService smsService)
    {
        _smsService = smsService;
    }
    
    public async Task SendVerificationCode(string phoneNumber, string code)
    {
        var model = new { Code = code };
        
        var result = await _smsService.Send("verification-code", model, phoneNumber);
        
        if (!result.Success)
        {
            throw new InvalidOperationException($"Failed to send SMS: {result.ErrorMessage}");
        }
    }
}
```

### Direct Message Sending

For more control, use the delivery services directly:

```csharp
public class DirectMessageService
{
    private readonly IEmailDeliveryService _emailDelivery;
    private readonly ISmsDeliveryService _smsDelivery;
    
    public DirectMessageService(
        IEmailDeliveryService emailDelivery,
        ISmsDeliveryService smsDelivery)
    {
        _emailDelivery = emailDelivery;
        _smsDelivery = smsDelivery;
    }
    
    public async Task SendDirectEmail()
    {
        var message = new EmailMessage
        {
            Senders = new EmailSenders("noreply@your-domain.azurecomm.net", "Your App"),
            Recipients = new EmailRecipients(["user@example.com"]),
            Content = new EmailContent
            {
                Subject = "Direct Email via Azure",
                HtmlBody = "<h1>Hello from Azure Communication Services!</h1>",
                TextBody = "Hello from Azure Communication Services!"
            }
        };
        
        var result = await _emailDelivery.Send(message);
    }
    
    public async Task SendDirectSms()
    {
        var message = new SmsMessage
        {
            From = "+1234567890",
            To = "+1987654321",
            Message = "Hello from Azure Communication Services!"
        };
        
        var result = await _smsDelivery.Send(message);
    }
}
```

## Azure Portal Setup

### Setting up Email Domain

1. Go to your Azure Communication Services resource
2. Navigate to **Email** > **Provision domains**
3. Add a custom domain or use the Azure-managed domain
4. Verify domain ownership (for custom domains)
5. Configure SPF, DKIM, and DMARC records (for custom domains)

### Setting up Phone Number

1. Go to your Azure Communication Services resource
2. Navigate to **Phone numbers** > **Get**
3. Select your country and phone number type
4. Choose SMS capability
5. Complete the purchase

### Getting Connection String

1. Go to your Azure Communication Services resource
2. Navigate to **Keys**
3. Copy the connection string

## Best Practices

### Security

- Store connection strings in Azure Key Vault for production
- Use managed identity when possible
- Rotate access keys regularly

```csharp
// Using Azure Key Vault
services.AddAzureEmailDeliver(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    return configuration["KeyVault:AzureCommunicationServices"];
});
```

### Performance

- Azure Communication Services handles rate limiting automatically
- Consider implementing retry policies for transient failures
- Use batch operations when sending multiple messages

### Monitoring

- Enable diagnostic logging in Azure
- Monitor delivery rates and failures
- Set up alerts for quota limits


## Troubleshooting

### Common Issues

1. **Invalid Domain Error**
   - Ensure email domain is properly configured and verified
   - Check SPF/DKIM/DMARC records for custom domains

2. **Phone Number Not Provisioned**
   - Verify phone number is purchased and SMS-enabled
   - Check regional availability

3. **Connection String Issues**
   - Verify connection string format
   - Ensure access key is not expired
   - Check resource endpoint URL

4. **Quota Exceeded**
   - Monitor usage in Azure portal
   - Request quota increases if needed
   - Implement rate limiting in your application

## Pricing Considerations

- Azure Communication Services charges per message sent
- Email: Pay per email sent
- SMS: Pay per SMS sent, varies by destination
- Inbound SMS and phone number rental have separate charges
- Monitor costs through Azure Cost Management

## Migration from Other Providers

When migrating from other email/SMS providers:

1. Update service registration
2. Verify template compatibility
3. Update configuration settings
4. Test thoroughly in development
5. Monitor delivery rates after migration

Example migration from SMTP:

```csharp
// Before - SMTP
services.AddSmtpEmailDeliver();

// After - Azure Communication Services
services.AddAzureEmailDeliver("AzureCommunicationServices");
```

## Related Resources

- [Azure Communication Services Documentation](https://docs.microsoft.com/en-us/azure/communication-services/)
- [Azure Communication Services Pricing](https://azure.microsoft.com/en-us/pricing/details/communication-services/)
- [Azure Communication Services REST API](https://docs.microsoft.com/en-us/rest/api/communication/)
