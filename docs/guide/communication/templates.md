# Template Best Practices

This guide covers best practices for creating and managing templates in Arbiter.Communication, including design patterns, performance considerations, and maintenance strategies.

## Template Structure

### YAML Template Format

Templates use YAML format with specific fields for email and SMS:

**Email Template Structure:**

```yaml
subject: Your subject with {{ variables }}

textBody: |
    Plain text version of your email.
    Supports multiple lines and {{ variables }}.

htmlBody: |
    <!DOCTYPE html>
    <html>
    <head>
        <title>{{ Subject }}</title>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
    </head>
    <body>
        <!-- Your HTML content with {{ variables }} -->
    </body>
    </html>
```

**SMS Template Structure:**

```yaml
message: Your SMS message with {{ variables }}. Keep it concise!
```

## Template Organization

### File Naming Convention

Use descriptive, kebab-case names for your templates:

```text
user-welcome.yaml
password-reset.yaml
order-confirmation.yaml
payment-failed.yaml
account-verification.yaml
marketing-newsletter.yaml
support-ticket-created.yaml
```

### Directory Structure

Organize templates by category:

```text
Templates/
├── Authentication/
│   ├── welcome-email.yaml
│   ├── password-reset.yaml
│   └── account-verification.yaml
├── Orders/
│   ├── order-confirmation.yaml
│   ├── order-shipped.yaml
│   └── order-delivered.yaml
├── Marketing/
│   ├── newsletter.yaml
│   ├── product-announcement.yaml
│   └── special-offer.yaml
└── Support/
    ├── ticket-created.yaml
    ├── ticket-resolved.yaml
    └── satisfaction-survey.yaml
```

### Resource Embedding

Configure multiple template resolvers for different categories:

```csharp
services.AddTemplateResourceResolver<Program>("MyApp.Templates.Auth.{0}.yaml", priority: 100);
services.AddTemplateResourceResolver<Program>("MyApp.Templates.Orders.{0}.yaml", priority: 200);
services.AddTemplateResourceResolver<Program>("MyApp.Templates.Marketing.{0}.yaml", priority: 300);
```

## Template Design Guidelines

### Email Templates

#### HTML Structure

Always include proper HTML structure:

```yaml
htmlBody: |
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>{{ Subject }}</title>
        <style>
            /* Inline CSS for better compatibility */
            body { font-family: Arial, sans-serif; margin: 0; padding: 20px; }
            .container { max-width: 600px; margin: 0 auto; }
            .header { background-color: #f8f9fa; padding: 20px; }
            .content { padding: 20px; }
            .footer { background-color: #f8f9fa; padding: 10px; font-size: 12px; }
        </style>
    </head>
    <body>
        <div class="container">
            <div class="header">
                <h1>{{ CompanyName }}</h1>
            </div>
            <div class="content">
                <!-- Your content here -->
            </div>
            <div class="footer">
                <p>&copy; {{ CurrentYear }} {{ CompanyName }}. All rights reserved.</p>
            </div>
        </div>
    </body>
    </html>
```

#### Responsive Design

Use responsive techniques for mobile compatibility:

```yaml
htmlBody: |
    <!DOCTYPE html>
    <html>
    <head>
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <style>
            @media only screen and (max-width: 600px) {
                .container { width: 100% !important; }
                .content { padding: 10px !important; }
                h1 { font-size: 24px !important; }
            }
        </style>
    </head>
    <body>
        <!-- Content -->
    </body>
    </html>
```

#### Text Fallback

Always provide a text version:

```yaml
subject: Welcome to {{ ProductName }}!

textBody: |
    Hello {{ UserName }},
    
    Welcome to {{ ProductName }}! We're excited to have you on board.
    
    To get started, please verify your account by clicking this link:
    {{ VerificationLink }}
    
    If you have any questions, feel free to contact our support team.
    
    Best regards,
    The {{ ProductName }} Team

htmlBody: |
    <!-- Rich HTML version -->
```

### SMS Templates

#### Keep It Concise

SMS messages should be under 160 characters when possible:

```yaml
# Good - Concise and clear
message: Your {{ ProductName }} verification code is {{ Code }}. Valid for 5 minutes.

# Avoid - Too verbose
message: Hello {{ UserName }}, thank you for signing up for {{ ProductName }}. Your verification code is {{ Code }}. Please enter this code to verify your account. This code will expire in 5 minutes.
```

#### Include Essential Information

Prioritize the most important information:

```yaml
message: |
    Order #{{ OrderNumber }} shipped! 
    Track: {{ TrackingLink }}
    ETA: {{ EstimatedDelivery }}
```

#### Brand Identification

Include your brand name, especially for important messages:

```yaml
message: "{{ CompanyName }}: Your password was reset. If this wasn't you, contact support immediately."
```

## Fluid Templating Best Practices

### Variable Handling

#### Safe Property Access

Use safe navigation to handle null values:

```yaml
subject: Welcome{% if ProductName != blank %} to {{ ProductName }}{% endif %}

textBody: |
    Hello {{ UserName | default: "Customer" }},
    
    {% if CompanyName != blank -%}
    Welcome to {{ CompanyName }}!
    {% else -%}
    Welcome!
    {% endif %}
```

#### Conditional Content

Use conditionals for optional content:

```yaml
htmlBody: |
    <h1>Order Confirmation</h1>
    <p>Thank you for your order, {{ CustomerName }}!</p>
    
    {% if DiscountApplied -%}
    <div class="discount-notice">
        <strong>Discount Applied:</strong> You saved {{ DiscountAmount }}!
    </div>
    {% endif -%}
    
    {% if ShippingNotes != blank -%}
    <div class="shipping-notes">
        <strong>Special Instructions:</strong> {{ ShippingNotes }}
    </div>
    {% endif -%}
```

#### Loops and Collections

Handle collections safely:

```yaml
htmlBody: |
    <h2>Order Items</h2>
    {% if Items and Items.size > 0 -%}
    <ul>
        {% for item in Items -%}
        <li>
            <strong>{{ item.Name }}</strong> - 
            Qty: {{ item.Quantity }} - 
            Price: {{ item.Price | currency }}
        </li>
        {% endfor -%}
    </ul>
    {% else -%}
    <p>No items found.</p>
    {% endif -%}
```

### Custom Filters

Define custom filters for common formatting:

```csharp
services.AddSingleton<FluidParser>(sp =>
{
    var parser = new FluidParser();
    
    // Add custom filters
    TemplateContext.GlobalFilters.AddFilter("currency", (input, arguments, context) =>
    {
        if (input.ToNumberValue() is var number && !double.IsNaN(number))
        {
            return new StringValue(number.ToString("C"));
        }
        return StringValue.Empty;
    });
    
    TemplateContext.GlobalFilters.AddFilter("shortdate", (input, arguments, context) =>
    {
        if (DateTime.TryParse(input.ToStringValue(), out var date))
        {
            return new StringValue(date.ToString("M/d/yyyy"));
        }
        return StringValue.Empty;
    });
    
    return parser;
});
```

Use custom filters in templates:

```yaml
htmlBody: |
    <p>Order Total: {{ OrderTotal | currency }}</p>
    <p>Order Date: {{ OrderDate | shortdate }}</p>
```

## Performance Optimization

### Model Optimization

Keep template models lightweight:

```csharp
// Good - Specific model for template
public class WelcomeEmailModel
{
    public string UserName { get; set; }
    public string ProductName { get; set; }
    public string VerificationLink { get; set; }
}

// Avoid - Passing entire entities
public class User
{
    // 50+ properties that won't be used in template
}
```

## Testing Strategies

### Template Validation

Create tests to validate template rendering:

```csharp
[Test]
public void ValidateWelcomeEmailTemplate()
{
    // Arrange
    var templateService = _serviceProvider.GetRequiredService<ITemplateService>();
    var model = new WelcomeEmailModel
    {
        UserName = "John Doe",
        ProductName = "TestApp",
        VerificationLink = "https://test.com/verify/123"
    };
    
    // Act
    var success = templateService.TryGetTemplate<EmailTemplate>("welcome-email", out var template);
    Assert.True(success);
    
    var renderedSubject = templateService.RenderTemplate(template.Subject, model);
    var renderedHtml = templateService.RenderTemplate(template.HtmlBody, model);
    var renderedText = templateService.RenderTemplate(template.TextBody, model);
    
    // Assert
    Assert.Contains("John Doe", renderedSubject);
    Assert.Contains("TestApp", renderedSubject);
    Assert.Contains("verify/123", renderedHtml);
    Assert.Contains("verify/123", renderedText);
    Assert.DoesNotContain("{{", renderedHtml); // No unrendered variables
}
```

### Integration Testing

Test the complete email flow:

```csharp
[Test]
public async Task TestCompleteEmailFlow()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddEmailDelivery<MemoryEmailDeliverService>();
    services.AddTemplateResourceResolver<TestClass>("TestApp.Templates.{0}.yaml");
    
    var provider = services.BuildServiceProvider();
    var emailService = provider.GetRequiredService<IEmailTemplateService>();
    var memoryService = provider.GetRequiredService<IEmailDeliveryService>() as MemoryEmailDeliverService;
    
    var model = new { UserName = "Test User", ProductName = "Test App" };
    var recipients = new EmailRecipients(["test@example.com"]);
    
    // Act
    var result = await emailService.Send("welcome-email", model, recipients);
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.Single(memoryService.Messages);
    
    var sentMessage = memoryService.Messages.First();
    Assert.Equal("Welcome to Test App!", sentMessage.Content.Subject);
    Assert.Contains("Test User", sentMessage.Content.HtmlBody);
}
```

This comprehensive guide should help you create maintainable, performant, and secure templates for your Arbiter.Communication implementation.
