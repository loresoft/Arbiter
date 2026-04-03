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
///   <item>Destination properties referenced in <c>Property()</c> exist on the destination type (ARB0004).</item>
///   <item>The same destination property is not mapped more than once (ARB0005).</item>
///   <item>Invocations follow the <c>mapping.Property(...).From/Value/Ignore()</c> pattern (ARB0006).</item>
///   <item>Source properties referenced in simple <c>From()</c> paths exist on the source type (ARB0007).</item>
/// </list>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MapperDiagnosticAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(
            MapperDiagnostics.ClassMustBePartial,
            MapperDiagnostics.ClassMustInheritMapperProfile,
            MapperDiagnostics.InvalidStatementInConfigureMapping,
            MapperDiagnostics.DestinationPropertyNotFound,
            MapperDiagnostics.DuplicateDestinationMapping,
            MapperDiagnostics.InvalidMappingCallPattern,
            MapperDiagnostics.SourcePropertyNotFound);

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
    /// Validates all ConfigureMapping method bodies across partial declarations.
    /// </summary>
    private static void ValidateConfigureMappingMethods(
        SymbolAnalysisContext context,
        INamedTypeSymbol typeSymbol,
        INamedTypeSymbol sourceType,
        INamedTypeSymbol destinationType)
    {
        var destinationPropertyNames = GetPropertyNames(destinationType);
        var sourcePropertyNames = GetPropertyNames(sourceType);

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

                ValidateMethodBody(
                    context,
                    method.Body,
                    mappingParameterName,
                    sourceType,
                    destinationType,
                    sourcePropertyNames,
                    destinationPropertyNames);
            }
        }
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
    /// Validates each statement in the ConfigureMapping method body.
    /// </summary>
    private static void ValidateMethodBody(
        SymbolAnalysisContext context,
        BlockSyntax body,
        string? mappingParameterName,
        INamedTypeSymbol sourceType,
        INamedTypeSymbol destinationType,
        HashSet<string> sourcePropertyNames,
        HashSet<string> destinationPropertyNames)
    {
        var seenDestinations = new Dictionary<string, Location>(StringComparer.Ordinal);

        foreach (var statement in body.Statements)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            // ARB0003: must be an expression statement
            if (statement is not ExpressionStatementSyntax expressionStatement)
            {
                ReportInvalidStatement(context, statement);
                continue;
            }

            // Must be an invocation expression
            if (expressionStatement.Expression is not InvocationExpressionSyntax outerInvocation)
            {
                ReportInvalidStatement(context, statement);
                continue;
            }

            // Must be a member access (e.g. .From(), .Value(), .Ignore())
            if (outerInvocation.Expression is not MemberAccessExpressionSyntax outerMemberAccess)
            {
                ReportInvalidStatement(context, statement);
                continue;
            }

            var methodName = outerMemberAccess.Name.Identifier.Text;

            // Validate the method name is From, Value, or Ignore
            if (!IsRecognizedMappingMethod(methodName))
            {
                // ARB0006: unrecognized method call
                context.ReportDiagnostic(Diagnostic.Create(
                    MapperDiagnostics.InvalidMappingCallPattern,
                    outerInvocation.GetLocation(),
                    outerInvocation.ToString()));
                continue;
            }

            // The receiver should be the Property(...) invocation
            if (outerMemberAccess.Expression is not InvocationExpressionSyntax propertyInvocation)
            {
                // ARB0006: not chained off Property()
                context.ReportDiagnostic(Diagnostic.Create(
                    MapperDiagnostics.InvalidMappingCallPattern,
                    outerInvocation.GetLocation(),
                    outerInvocation.ToString()));
                continue;
            }

            // Validate that Property() is called on the mapping parameter
            if (!IsPropertyCallOnMappingParameter(propertyInvocation, mappingParameterName))
            {
                // ARB0003: not a recognized mapping call
                ReportInvalidStatement(context, statement);
                continue;
            }

            // Extract and validate destination property name
            var destName = GetDestinationPropertyName(propertyInvocation);
            if (destName == null)
                continue;

            var destLocation = propertyInvocation.GetLocation();

            // ARB0004: destination property must exist
            if (!destinationPropertyNames.Contains(destName))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    MapperDiagnostics.DestinationPropertyNotFound,
                    destLocation,
                    destName,
                    destinationType.Name));
            }

            // ARB0005: duplicate destination mapping
            if (seenDestinations.TryGetValue(destName, out var previousLocation))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    MapperDiagnostics.DuplicateDestinationMapping,
                    destLocation,
                    destName));
            }

            seenDestinations[destName] = destLocation;

            // ARB0007: validate source property exists for simple From() paths
            if (string.Equals(methodName, MapperConstants.FromMethodName, StringComparison.Ordinal))
            {
                ValidateSourceProperty(context, outerInvocation, sourceType, sourcePropertyNames);
            }
        }
    }

    /// <summary>
    /// Checks whether the method name is one of the recognized mapping methods.
    /// </summary>
    private static bool IsRecognizedMappingMethod(string methodName)
    {
        return string.Equals(methodName, MapperConstants.FromMethodName, StringComparison.Ordinal)
            || string.Equals(methodName, MapperConstants.ValueMethodName, StringComparison.Ordinal)
            || string.Equals(methodName, MapperConstants.IgnoreMethodName, StringComparison.Ordinal);
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
    /// Validates that the source property in a From() call exists on the source type.
    /// Only checks simple property path expressions (e.g. p =&gt; p.Name or p =&gt; p.Department.Name).
    /// Complex expressions (method calls, arithmetic, etc.) are skipped.
    /// </summary>
    private static void ValidateSourceProperty(
        SymbolAnalysisContext context,
        InvocationExpressionSyntax fromInvocation,
        INamedTypeSymbol sourceType,
        HashSet<string> sourcePropertyNames)
    {
        if (fromInvocation.ArgumentList.Arguments.Count != 1)
            return;

        var argument = fromInvocation.ArgumentList.Arguments[0].Expression;

        // Extract lambda parameter name and body
        string parameterName;
        ExpressionSyntax lambdaBody;

        if (argument is SimpleLambdaExpressionSyntax simpleLambda
            && simpleLambda.Body is ExpressionSyntax simpleBody)
        {
            parameterName = simpleLambda.Parameter.Identifier.Text;
            lambdaBody = simpleBody;
        }
        else if (argument is ParenthesizedLambdaExpressionSyntax parenLambda
            && parenLambda.ParameterList.Parameters.Count > 0
            && parenLambda.Body is ExpressionSyntax parenBody)
        {
            parameterName = parenLambda.ParameterList.Parameters[0].Identifier.Text;
            lambdaBody = parenBody;
        }
        else
        {
            return;
        }

        // Try to decompose into a simple property path
        var path = new List<string>();
        var expression = lambdaBody;

        while (expression is MemberAccessExpressionSyntax memberAccess)
        {
            path.Insert(0, memberAccess.Name.Identifier.Text);
            expression = memberAccess.Expression;

            // Unwrap null-forgiving operator
            if (expression is PostfixUnaryExpressionSyntax postfix)
                expression = postfix.Operand;
        }

        // Only validate simple property paths (not complex expressions)
        if (expression is not IdentifierNameSyntax identifier
            || !string.Equals(identifier.Identifier.Text, parameterName, StringComparison.Ordinal)
            || path.Count == 0)
        {
            return;
        }

        // Validate the first segment exists on the source type
        var firstSegment = path[0];
        if (!sourcePropertyNames.Contains(firstSegment))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                MapperDiagnostics.SourcePropertyNotFound,
                fromInvocation.ArgumentList.GetLocation(),
                firstSegment,
                sourceType.Name));
            return;
        }

        // Validate subsequent path segments by walking the type chain
        if (path.Count > 1)
        {
            ValidateNestedSourcePath(context, fromInvocation, sourceType, path);
        }
    }

    /// <summary>
    /// Validates nested property paths (e.g. p.Department.Name) by walking the type chain.
    /// </summary>
    private static void ValidateNestedSourcePath(
        SymbolAnalysisContext context,
        InvocationExpressionSyntax fromInvocation,
        INamedTypeSymbol sourceType,
        List<string> path)
    {
        var currentType = sourceType;

        for (var i = 0; i < path.Count; i++)
        {
            var segment = path[i];
            var property = FindProperty(currentType, segment);

            if (property == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    MapperDiagnostics.SourcePropertyNotFound,
                    fromInvocation.ArgumentList.GetLocation(),
                    segment,
                    currentType.Name));
                return;
            }

            // Move to the next type in the chain for navigation properties
            if (i < path.Count - 1)
            {
                if (property.Type is INamedTypeSymbol namedType)
                    currentType = namedType;
                else
                    return; // Can't resolve further, skip remaining validation
            }
        }
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
        switch (statement)
        {
            case LocalDeclarationStatementSyntax:
                return "variable declaration";
            case IfStatementSyntax:
                return "if statement";
            case ForStatementSyntax:
                return "for loop";
            case ForEachStatementSyntax:
                return "foreach loop";
            case WhileStatementSyntax:
                return "while loop";
            case DoStatementSyntax:
                return "do-while loop";
            case SwitchStatementSyntax:
                return "switch statement";
            case ReturnStatementSyntax:
                return "return statement";
            case ThrowStatementSyntax:
                return "throw statement";
            case TryStatementSyntax:
                return "try statement";
            case LockStatementSyntax:
                return "lock statement";
            case UsingStatementSyntax:
                return "using statement";
            case ExpressionStatementSyntax expr:
                return expr.Expression.ToString();
            default:
                return statement.Kind().ToString();
        }
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
    /// Collects all instance property names from the type and its base types (case-insensitive).
    /// </summary>
    private static HashSet<string> GetPropertyNames(INamedTypeSymbol type)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var current = type;

        while (current != null && current.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in current.GetMembers())
            {
                if (member is IPropertySymbol prop
                    && !prop.IsStatic
                    && !prop.IsIndexer)
                {
                    names.Add(prop.Name);
                }
            }

            current = current.BaseType;
        }

        return names;
    }

    /// <summary>
    /// Finds an instance property by name in the type or its base types.
    /// </summary>
    private static IPropertySymbol? FindProperty(INamedTypeSymbol type, string name)
    {
        var current = type;

        while (current != null && current.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in current.GetMembers())
            {
                if (member is IPropertySymbol prop
                    && !prop.IsStatic
                    && string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return prop;
                }
            }

            current = current.BaseType;
        }

        return null;
    }
}
