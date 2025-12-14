namespace Arbiter.Services.GlobMatching;

/// <summary>
/// Builder for creating PatternInfo instances with fluent API.
/// </summary>
internal sealed class PatternInfoBuilder
{
    private MatchMode _mode = MatchMode.Complex;
    private string? _literalValue;
    private bool _hasDoubleStar;
    private bool _hasCharClass;
    private bool _hasBrace;
    private char? _firstLiteralChar;
    private char _firstLiteralCharUpper;

    private PatternInfoBuilder() { }

    public static PatternInfoBuilder Create() => new();

    public PatternInfoBuilder WithMode(MatchMode mode)
    {
        _mode = mode;
        return this;
    }

    public PatternInfoBuilder WithLiteral(string value)
    {
        _literalValue = value;
        return this;
    }

    public PatternInfoBuilder WithCharacteristics(PatternCharacteristics characteristics)
    {
        _hasDoubleStar = characteristics.HasDoubleStar;
        _hasCharClass = characteristics.HasCharClass;
        _hasBrace = characteristics.HasBrace;
        return this;
    }

    public PatternInfoBuilder WithFirstLiteral(FirstLiteralChar firstLiteral)
    {
        _firstLiteralChar = firstLiteral.Char;
        _firstLiteralCharUpper = firstLiteral.Upper;
        return this;
    }

    public PatternInfoBuilder AsSimple()
    {
        _hasDoubleStar = false;
        _hasCharClass = false;
        _hasBrace = false;
        return this;
    }

    public PatternInfoBuilder AsExact(string value)
    {
        return WithMode(MatchMode.Exact)
            .WithLiteral(value)
            .AsSimple();
    }

    public PatternInfoBuilder AsStartsWith(string prefix)
    {
        return WithMode(MatchMode.StartsWith)
            .WithLiteral(prefix)
            .AsSimple();
    }

    public PatternInfoBuilder AsEndsWith(string suffix)
    {
        return WithMode(MatchMode.EndsWith)
            .WithLiteral(suffix)
            .AsSimple();
    }

    public PatternInfoBuilder AsComplex()
    {
        return WithMode(MatchMode.Complex);
    }

    public PatternInfo Build()
    {
        return new PatternInfo
        {
            MatchMode = _mode,
            LiteralValue = _literalValue,
            HasDoubleStar = _hasDoubleStar,
            HasCharacterClass = _hasCharClass,
            HasBraceExpansion = _hasBrace,
            FirstLiteralChar = _firstLiteralChar,
            FirstLiteralCharUpper = _firstLiteralCharUpper
        };
    }
}
