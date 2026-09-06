using Arbiter.Components.Extensions;

using Microsoft.AspNetCore.Components;

namespace Arbiter.Components.Tests.Extensions;

public class StringExtensionsTests
{
    [Test]
    public async Task ToMarkupStringKeepsTheSuppliedMarkup()
    {
        var markup = "<b>PO-10432</b>".ToMarkupString();

        await Assert.That(markup.Value).IsEqualTo("<b>PO-10432</b>");
    }

    [Test]
    public async Task ToMarkupStringReturnsEmptyMarkupWhenTheValueIsNull()
    {
        string? value = null;

        var markup = value.ToMarkupString();

        await Assert.That(markup.Value).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task ToMarkupStringReturnsAMarkupString()
    {
        var markup = "text".ToMarkupString();

        await Assert.That(markup).IsTypeOf<MarkupString>();
    }
}
