using System.Diagnostics.CodeAnalysis;

using Arbiter.Communication.Template;

namespace Arbiter.Communication.Tests;

public class MockTemplateResolver : ITemplateResolver
{
    public int Priority { get; } = 100;

    public bool TryResolveTemplate<TTemplate>(string templateName, [NotNullWhen(true)] out TTemplate? template)
    {
        template = default;
        return false;
    }
}
