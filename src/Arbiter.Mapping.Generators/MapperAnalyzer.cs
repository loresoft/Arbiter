#pragma warning disable MA0051 // Method is too long

using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Arbiter.Mapping.Generators;

/// <summary>
/// Roslyn diagnostic analyzer that validates mapper classes annotated with <c>[GenerateMapper]</c>.
/// </summary>
/// <remarks>
/// <para>
/// This analyzer runs independently of the source generator pipeline so that diagnostics
/// do not interfere with incremental generator caching. It validates:
/// </para>
/// <list type="bullet">
///   <item>The mapper class is declared <c>partial</c> (ARB0001).</item>
///   <item>The mapper class inherits from <c>MapperProfile&lt;TSource, TDestination&gt;</c> (ARB0002).</item>
///   <item>The <c>ConfigureMapping</c> body contains only recognized mapping calls (ARB0003).</item>
///   <item>Auto-matched properties have compatible types between source and destination (ARB0004).</item>
///   <item>The same destination property is not mapped more than once (ARB0005).</item>
///   <item>Invocations follow the <c>mapping.Property(...).From/Value/Ignore()</c> pattern (ARB0006).</item>
/// </list>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MapperAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(
            MapperDiagnostics.ClassMustBePartial,
            MapperDiagnostics.ClassMustInheritMapperProfile,
            MapperDiagnostics.InvalidStatementInConfigureMapping,
            MapperDiagnostics.PropertyTypeMismatch,
            MapperDiagnostics.DuplicateDestinationMapping,
            MapperDiagnostics.InvalidMappingCallPattern);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol typeSymbol)
            return;

        if (!HasGenerateMapperAttribute(typeSymbol))
            return;

        // ARB0001: must be partial
        CheckPartialDeclaration(context, typeSymbol);

        // ARB0002: must inherit MapperProfile<,>
        var baseType = FindMapperBaseType(typeSymbol);
        if (baseType == null)
        {
            foreach (var location in typeSymbol.Locations)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    MapperDiagnostics.ClassMustInheritMapperProfile,
                    location,
                    typeSymbol.Name));
            }

            return;
        }

        // Resolve TSource and TDestination
        if (baseType.TypeArguments.Length != 2
            || baseType.TypeArguments[0] is not INamedTypeSymbol sourceType
            || baseType.TypeArguments[1] is not INamedTypeSymbol destinationType)
        {
            return;
        }

        // Validate ConfigureMapping method bodies
        ValidateConfigureMappingMethods(context, typeSymbol, sourceType, destinationType);
    }

    /// <summary>
    /// Checks whether the type has the <c>[GenerateMapper]</c> attribute.
    /// </summary>
    private static bool HasGenerateMapperAttribute(INamedTypeSymbol typeSymbol)
    {
        foreach (var attribute in typeSymbol.GetAttributes())
        {
            var attrClass = attribute.AttributeClass;
            if (attrClass == null)
                continue;

            var fullName = attrClass.ToDisplayString();
            if (string.Equals(fullName, MapperConstants.GenerateMapperAttributeName, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Reports ARB0001 if the class is not declared partial.
    /// </summary>
    private static void CheckPartialDeclaration(SymbolAnalysisContext context, INamedTypeSymbol typeSymbol)
    {
        foreach (var syntaxRef in typeSymbol.DeclaringSyntaxReferences)
        {
            var syntax = syntaxRef.GetSyntax(context.CancellationToken);
            if (syntax is not ClassDeclarationSyntax classDecl)
                continue;

            var hasPartial = false;
            foreach (var modifier in classDecl.Modifiers)
            {
                if (modifier.IsKind(SyntaxKind.PartialKeyword))
                {
                    hasPartial = true;
                    break;
                }
            }

            if (!hasPartial)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    MapperDiagnostics.ClassMustBePartial,
                    classDecl.Identifier.GetLocation(),
                    typeSymbol.Name));
            }
        }
    }

    /// <summary>
    /// Walks the inheritance chain to find <c>MapperProfile&lt;TSource, TDestination&gt;</c>.
    /// </summary>
    private static INamedTypeSymbol? FindMapperBaseType(INamedTypeSymbol typeSymbol)
    {
        var current = typeSymbol.BaseType;

        while (current != null)
        {
            if (current.IsGenericType
                && string.Equals(current.Name, MapperConstants.MapperBaseClassName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(current.ContainingNamespace?.ToDisplayString(), MapperConstants.MapperBaseNamespace, StringComparison.OrdinalIgnoreCase)
                && current.TypeArguments.Length == 2)
            {
                return current;
            }

            current = current.BaseType;
        }

        return null;
    }

    /// <summary>
    /// Validates all ConfigureMapping method bodies across partial declarations and
    /// checks auto-matched properties for type compatibility.
    /// </summary>
    private static void ValidateConfigureMappingMethods(
        SymbolAnalysisContext context,
        INamedTypeSymbol typeSymbol,
        INamedTypeSymbol sourceType,
        INamedTypeSymbol destinationType)
    {
        var customMappedProperties = new HashSet<string>(StringComparer.Ordinal);
        var configuredPairs = new List<(string Source, string Destination)>();

        foreach (var syntaxRef in typeSymbol.DeclaringSyntaxReferences)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var syntax = syntaxRef.GetSyntax(context.CancellationToken);
            if (syntax is not ClassDeclarationSyntax classDecl)
                continue;

            foreach (var member in classDecl.Members)
            {
                if (member is not MethodDeclarationSyntax method)
                    continue;

                if (!string.Equals(method.Identifier.Text, MapperConstants.ConfigureMappingMethodName, StringComparison.Ordinal))
                    continue;

                if (method.Body == null)
                    continue;

                // Get the mapping parameter name for validation
                var mappingParameterName = GetMappingParameterName(method);

                ValidateMethodBody(context, method.Body, mappingParameterName, customMappedProperties, configuredPairs);
            }
        }

        // ARB0004: check auto-matched properties for type compatibility
        ValidateAutoMatchedPropertyTypes(context, typeSymbol, sourceType, destinationType, customMappedProperties, configuredPairs);
    }

    /// <summary>
    /// Gets the parameter name of the MappingBuilder parameter in ConfigureMapping.
    /// </summary>
    private static string? GetMappingParameterName(MethodDeclarationSyntax method)
    {
        if (method.ParameterList.Parameters.Count != 1)
            return null;

        return method.ParameterList.Parameters[0].Identifier.Text;
    }

    /// <summary>
    /// Validates each statement in the ConfigureMapping method body and collects
    /// the names of destination properties that have explicit custom mappings.
    /// </summary>
    private static void ValidateMethodBody(
        SymbolAnalysisContext context,
        BlockSyntax body,
        string? mappingParameterName,
        HashSet<string> customMappedProperties,
        List<(string Source, string Destination)> configuredPairs)
    {
        var seenDestinations = new Dictionary<string, Location>(StringComparer.Ordinal);

        foreach (var statement in body.Statements)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            // type-level nested configuration: mapping.Map<TSource, TDestination>(...)
            if (TryGetNestedConfiguration(statement, mappingParameterName, out var mapInvocation))
            {
                ProcessNestedConfiguration(context, mapInvocation!, configuredPairs);
                continue;
            }

            var kind = ClassifyMappingStatement(
                statement,
                mappingParameterName,
                out var propertyInvocation,
                out var outerInvocation);

            switch (kind)
            {
                case MappingStatementKind.InvalidStatement:
                    ReportInvalidStatement(context, statement);
                    continue;
                case MappingStatementKind.InvalidPattern:
                    ReportInvalidMappingPattern(context, outerInvocation!);
                    continue;
            }

            // Extract destination property name for duplicate check and custom mapping tracking
            var destName = GetDestinationPropertyName(propertyInvocation!);
            if (destName == null)
                continue;

            customMappedProperties.Add(destName);

            if (outerInvocation != null
                && outerInvocation.Expression is MemberAccessExpressionSyntax { Name.Identifier.Text: MapperConstants.MapWithMethodName })
            {
                ProcessNestedConfiguration(context, outerInvocation, configuredPairs);
            }

            // ARB0005: duplicate destination mapping
            var destLocation = propertyInvocation!.GetLocation();
            if (seenDestinations.ContainsKey(destName))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    MapperDiagnostics.DuplicateDestinationMapping,
                    destLocation,
                    destName));
            }

            seenDestinations[destName] = destLocation;
        }
    }

    /// <summary>
    /// Classifies a statement as a valid mapping call, a non-mapping statement, or
    /// a malformed mapping pattern. Returns the parsed invocations for valid calls.
    /// </summary>
    private static MappingStatementKind ClassifyMappingStatement(
        StatementSyntax statement,
        string? mappingParameterName,
        out InvocationExpressionSyntax? propertyInvocation,
        out InvocationExpressionSyntax? outerInvocation)
    {
        propertyInvocation = null;
        outerInvocation = null;

        // Must be an expression statement containing an invocation with member access
        if (statement is not ExpressionStatementSyntax expressionStatement
            || expressionStatement.Expression is not InvocationExpressionSyntax invocation
            || invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return MappingStatementKind.InvalidStatement;
        }

        outerInvocation = invocation;
        var methodName = memberAccess.Name.Identifier.Text;

        // Check if the terminal method is recognized (From, Value, Ignore)
        if (!IsRecognizedMappingMethod(methodName))
        {
            // If the receiver is mapping.Property(...), it's a malformed pattern (ARB0006);
            // otherwise it's not a mapping call at all (ARB0003)
            if (memberAccess.Expression is InvocationExpressionSyntax possiblePropertyCall
                && IsPropertyCallOnMappingParameter(possiblePropertyCall, mappingParameterName))
            {
                return MappingStatementKind.InvalidPattern;
            }

            return MappingStatementKind.InvalidStatement;
        }

        // The receiver should be the Property(...) invocation
        if (memberAccess.Expression is not InvocationExpressionSyntax propInvocation)
            return MappingStatementKind.InvalidPattern;

        // Validate that Property() is called on the mapping parameter
        if (!IsPropertyCallOnMappingParameter(propInvocation, mappingParameterName))
            return MappingStatementKind.InvalidStatement;

        propertyInvocation = propInvocation;
        return MappingStatementKind.Valid;
    }

    /// <summary>
    /// Checks whether the method name is one of the recognized mapping methods.
    /// </summary>
    private static bool IsRecognizedMappingMethod(string methodName)
    {
        return string.Equals(methodName, MapperConstants.FromMethodName, StringComparison.Ordinal)
            || string.Equals(methodName, MapperConstants.ValueMethodName, StringComparison.Ordinal)
            || string.Equals(methodName, MapperConstants.IgnoreMethodName, StringComparison.Ordinal)
            || string.Equals(methodName, MapperConstants.MapWithMethodName, StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines whether the statement is a type-level <c>mapping.Map&lt;TSource, TDestination&gt;(...)</c> call.
    /// </summary>
    private static bool TryGetNestedConfiguration(
        StatementSyntax statement,
        string? mappingParameterName,
        out InvocationExpressionSyntax? invocation)
    {
        invocation = null;

        if (mappingParameterName == null
            || statement is not ExpressionStatementSyntax { Expression: InvocationExpressionSyntax candidate }
            || candidate.Expression is not MemberAccessExpressionSyntax memberAccess
            || memberAccess.Name is not GenericNameSyntax genericName
            || !string.Equals(genericName.Identifier.Text, MapperConstants.MapMethodName, StringComparison.Ordinal)
            || memberAccess.Expression is not IdentifierNameSyntax identifier
            || !string.Equals(identifier.Identifier.Text, mappingParameterName, StringComparison.Ordinal))
        {
            return false;
        }

        invocation = candidate;
        return true;
    }

    /// <summary>
    /// Records the configured nested type pair and validates the optional nested configuration lambda body.
    /// </summary>
    private static void ProcessNestedConfiguration(
        SymbolAnalysisContext context,
        InvocationExpressionSyntax invocation,
        List<(string Source, string Destination)> configuredPairs)
    {
        if (invocation.Expression is MemberAccessExpressionSyntax { Name: GenericNameSyntax { TypeArgumentList.Arguments.Count: 2 } genericName })
        {
            var arguments = genericName.TypeArgumentList.Arguments;
            configuredPairs.Add((GetSimpleTypeName(arguments[0]), GetSimpleTypeName(arguments[1])));
        }

        if (invocation.ArgumentList.Arguments.Count != 1
            || invocation.ArgumentList.Arguments[0].Expression is not LambdaExpressionSyntax lambda)
        {
            return;
        }

        var parameterName = lambda switch
        {
            SimpleLambdaExpressionSyntax simple => simple.Parameter.Identifier.Text,
            ParenthesizedLambdaExpressionSyntax { ParameterList.Parameters.Count: 1 } parenthesized => parenthesized.ParameterList.Parameters[0].Identifier.Text,
            _ => null,
        };

        if (lambda.Block != null)
        {
            var nestedProperties = new HashSet<string>(StringComparer.Ordinal);
            ValidateMethodBody(context, lambda.Block, parameterName, nestedProperties, configuredPairs);
        }
    }

    /// <summary>
    /// Gets the simple (unqualified, non-generic) name of a type syntax, e.g. <c>Address</c> for <c>Models.Address?</c>.
    /// </summary>
    private static string GetSimpleTypeName(TypeSyntax type)
    {
        return type switch
        {
            NullableTypeSyntax nullable => GetSimpleTypeName(nullable.ElementType),
            QualifiedNameSyntax qualified => qualified.Right.Identifier.Text,
            AliasQualifiedNameSyntax alias => alias.Name.Identifier.Text,
            SimpleNameSyntax simple => simple.Identifier.Text,
            _ => type.ToString(),
        };
    }

    /// <summary>
    /// Determines whether a source/destination property pair is deep cloned through a configured nested mapping,
    /// either directly or as collection elements / dictionary values.
    /// </summary>
    private static bool IsConfiguredPair(
        ITypeSymbol source,
        ITypeSymbol destination,
        List<(string Source, string Destination)> configuredPairs)
    {
        if (configuredPairs.Count == 0)
            return false;

        var sourceElement = TypeClassifier.GetSourceDictionaryTypes(source, out _)
            ?? TypeClassifier.GetSourceElementType(source)
            ?? source;

        var destinationElement = TypeClassifier.GetDestinationDictionaryTypes(destination, out _)
            ?? TypeClassifier.GetDestinationElementType(destination, out _)
            ?? destination;

        foreach (var pair in configuredPairs)
        {
            if (string.Equals(pair.Source, sourceElement.Name, StringComparison.Ordinal)
                && string.Equals(pair.Destination, destinationElement.Name, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Validates that the Property() call is on the mapping parameter (e.g. mapping.Property(...)).
    /// </summary>
    private static bool IsPropertyCallOnMappingParameter(
        InvocationExpressionSyntax propertyInvocation,
        string? mappingParameterName)
    {
        if (mappingParameterName == null)
            return false;

        // Property() must be called as mapping.Property(...)
        if (propertyInvocation.Expression is not MemberAccessExpressionSyntax propertyAccess)
            return false;

        if (!string.Equals(propertyAccess.Name.Identifier.Text, "Property", StringComparison.Ordinal))
            return false;

        // The receiver must be the mapping parameter
        if (propertyAccess.Expression is not IdentifierNameSyntax identifier)
            return false;

        return string.Equals(identifier.Identifier.Text, mappingParameterName, StringComparison.Ordinal);
    }

    /// <summary>
    /// Extracts the destination property name from a Property(d =&gt; d.Name) invocation.
    /// </summary>
    private static string? GetDestinationPropertyName(InvocationExpressionSyntax propertyInvocation)
    {
        if (propertyInvocation.ArgumentList.Arguments.Count != 1)
            return null;

        var argument = propertyInvocation.ArgumentList.Arguments[0].Expression;
        var lambdaBody = GetLambdaBody(argument);

        if (lambdaBody is MemberAccessExpressionSyntax memberAccess)
            return memberAccess.Name.Identifier.Text;

        return null;
    }

    /// <summary>
    /// Checks auto-matched properties (matched by name, not explicitly configured) for type
    /// compatibility between source and destination. Reports ARB0004 when an implicit conversion
    /// does not exist, which would cause the generated assignment to fail to compile.
    /// </summary>
    private static void ValidateAutoMatchedPropertyTypes(
        SymbolAnalysisContext context,
        INamedTypeSymbol typeSymbol,
        INamedTypeSymbol sourceType,
        INamedTypeSymbol destinationType,
        HashSet<string> customMappedProperties,
        List<(string Source, string Destination)> configuredPairs)
    {
        var sourceProperties = GetReadableProperties(sourceType);
        var destinationProperties = GetSettableProperties(destinationType);

        foreach (var destProp in destinationProperties)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            // Skip properties with explicit custom mappings (From/Value/Ignore)
            if (customMappedProperties.Contains(destProp.Name))
                continue;

            // Find auto-matched source property by name (case-insensitive, matching generator behavior)
            if (!sourceProperties.TryGetValue(destProp.Name, out var sourceProp))
                continue;

            // Check if source type can be implicitly converted to destination type
            var sourceTypeSymbol = sourceProp.Type;
            var destTypeSymbol = destProp.Type;

            // Skip if types are the same
            if (SymbolEqualityComparer.Default.Equals(sourceTypeSymbol, destTypeSymbol))
                continue;

            // Handle nullable-to-nullable and nullable-to-non-nullable of the same underlying type
            var sourceUnderlying = GetUnderlyingType(sourceTypeSymbol);
            var destUnderlying = GetUnderlyingType(destTypeSymbol);

            if (SymbolEqualityComparer.Default.Equals(sourceUnderlying, destUnderlying))
                continue;

            if (IsConfiguredPair(sourceTypeSymbol, destTypeSymbol, configuredPairs))
                continue;

            var conversion = context.Compilation.ClassifyConversion(sourceTypeSymbol, destTypeSymbol);
            if (!conversion.Exists || conversion.IsExplicit)
            {
                // Report on the class declaration where the mapper is defined
                foreach (var location in typeSymbol.Locations)
                {
                    Diagnostic diagnostic = Diagnostic.Create(
                        MapperDiagnostics.PropertyTypeMismatch,
                        location,
                        destProp.Name,
                        sourceTypeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                        destTypeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
                    );
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    /// <summary>
    /// Gets the underlying type, unwrapping <see cref="Nullable{T}"/> if present.
    /// </summary>
    private static ITypeSymbol GetUnderlyingType(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType
            && namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T
            && namedType.TypeArguments.Length == 1)
        {
            return namedType.TypeArguments[0];
        }

        return type;
    }

    /// <summary>
    /// Collects all readable instance properties from the type and its base types,
    /// keyed by name (case-insensitive) to match the generator's auto-matching behavior.
    /// </summary>
    private static Dictionary<string, IPropertySymbol> GetReadableProperties(INamedTypeSymbol type)
    {
        var properties = new Dictionary<string, IPropertySymbol>(StringComparer.OrdinalIgnoreCase);
        var current = type;

        while (current != null && current.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in current.GetMembers())
            {
                if (member is IPropertySymbol prop
                    && !prop.IsStatic
                    && !prop.IsIndexer
                    && prop.GetMethod != null
                    && !properties.ContainsKey(prop.Name))
                {
                    properties[prop.Name] = prop;
                }
            }

            current = current.BaseType;
        }

        return properties;
    }

    /// <summary>
    /// Collects all settable instance properties from the type and its base types.
    /// </summary>
    private static List<IPropertySymbol> GetSettableProperties(INamedTypeSymbol type)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var properties = new List<IPropertySymbol>();
        var current = type;

        while (current != null && current.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in current.GetMembers())
            {
                if (member is IPropertySymbol prop
                    && !prop.IsStatic
                    && !prop.IsIndexer
                    && prop.SetMethod != null
                    && seen.Add(prop.Name))
                {
                    properties.Add(prop);
                }
            }

            current = current.BaseType;
        }

        return properties;
    }

    /// <summary>
    /// Reports ARB0006 for a statement that follows the mapping chain but uses an unrecognized method.
    /// </summary>
    private static void ReportInvalidMappingPattern(SymbolAnalysisContext context, InvocationExpressionSyntax invocation)
    {
        context.ReportDiagnostic(Diagnostic.Create(
            MapperDiagnostics.InvalidMappingCallPattern,
            invocation.GetLocation(),
            invocation.ToString()));
    }

    /// <summary>
    /// Reports ARB0003 for a statement that is not a recognized mapping call.
    /// </summary>
    private static void ReportInvalidStatement(SymbolAnalysisContext context, StatementSyntax statement)
    {
        var description = GetStatementDescription(statement);

        context.ReportDiagnostic(Diagnostic.Create(
            MapperDiagnostics.InvalidStatementInConfigureMapping,
            statement.GetLocation(),
            description));
    }

    /// <summary>
    /// Returns a human-readable description of a statement kind for diagnostic messages.
    /// </summary>
    private static string GetStatementDescription(StatementSyntax statement)
    {
        return statement switch
        {
            LocalDeclarationStatementSyntax => "variable declaration",
            IfStatementSyntax => "if statement",
            ForStatementSyntax => "for loop",
            ForEachStatementSyntax => "foreach loop",
            WhileStatementSyntax => "while loop",
            DoStatementSyntax => "do-while loop",
            SwitchStatementSyntax => "switch statement",
            ReturnStatementSyntax => "return statement",
            ThrowStatementSyntax => "throw statement",
            TryStatementSyntax => "try statement",
            LockStatementSyntax => "lock statement",
            UsingStatementSyntax => "using statement",
            ExpressionStatementSyntax expr => expr.Expression.ToString(),
            _ => statement.Kind().ToString(),
        };
    }

    /// <summary>
    /// Extracts the body expression from a simple or parenthesized lambda.
    /// </summary>
    private static ExpressionSyntax? GetLambdaBody(ExpressionSyntax expression)
    {
        if (expression is SimpleLambdaExpressionSyntax simpleLambda)
            return simpleLambda.Body as ExpressionSyntax;

        if (expression is ParenthesizedLambdaExpressionSyntax parenLambda)
            return parenLambda.Body as ExpressionSyntax;

        return null;
    }

    /// <summary>
    /// Classification result for a statement within <c>ConfigureMapping</c>.
    /// </summary>
    private enum MappingStatementKind
    {
        /// <summary>The statement is a valid <c>mapping.Property(...).From/Value/Ignore(...)</c> call.</summary>
        Valid,

        /// <summary>The statement is not a mapping call at all (ARB0003).</summary>
        InvalidStatement,

        /// <summary>The statement follows the mapping chain but uses an unrecognized method (ARB0006).</summary>
        InvalidPattern,
    }

}
