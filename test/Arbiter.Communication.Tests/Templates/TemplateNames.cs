using System.Reflection;

namespace Arbiter.Communication.Tests.Templates;

public static class TemplateNames
{
    public static readonly Assembly TemplateAssembly = typeof(TemplateNames).Assembly;
    public static readonly string TemplateResourceFormat = $"{typeof(TemplateNames).Namespace}.{{0}}.yaml";

    public const string ResetPasswordEmail = "reset-password";
    public const string VerificationCode = "verification-code";

    public static string GetResourceName(string templateName)
    {
        return string.Format(TemplateResourceFormat, templateName);
    }
}
