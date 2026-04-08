#pragma warning disable MA0051 // Method is too long

using Arbiter.Mapping.Generators.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Arbiter.Mapping.Generators;

/// <summary>
/// Incremental source generator that emits mapping implementations for classes
/// annotated with <c>[GenerateMapper]</c> and derived from <c>MapperProfile&lt;TSource, TDestination&gt;</c>.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class MapperGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Display format that includes the type name and containing namespaces without global prefix or generics.
    /// </summary>
    private static readonly SymbolDisplayFormat NameAndNamespaces = new(SymbolDisplayGlobalNamespaceStyle.Omitted, SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, SymbolDisplayGenericsOptions.None);

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: MapperConstants.GenerateMapperAttributeName,
                predicate: SyntacticPredicate,
                transform: SemanticTransform
            )
            .WithTrackingName(MapperConstants.GeneratorTrackingName);

        // output code
        var mapperClasses = provider
            .Where(static item => item is not null);

        context.RegisterSourceOutput(mapperClasses, Execute);

    }

    /// <summary>
    /// Generates and emits the source file for a single mapper class.
    /// </summary>
    /// <param name="context">The source production context for emitting generated code.</param>
    /// <param name="mapperClass">The mapper class model to generate code for.</param>
    private static void Execute(SourceProductionContext context, MapperClass? mapperClass)
    {
        if (mapperClass == null)
            return;

        var source = MapperWriter.Generate(mapperClass);

        context.AddSource(mapperClass.OutputFile, source);
    }

    /// <summary>
    /// Filters syntax nodes to class declarations only.
    /// </summary>
    /// <param name="syntaxNode">The syntax node to evaluate.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation.</param>
    /// <returns><see langword="true"/> if the node is a class declaration; otherwise <see langword="false"/>.</returns>
    private static bool SyntacticPredicate(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        return syntaxNode is ClassDeclarationSyntax;
    }

    /// <summary>
    /// Transforms a syntax context into a <see cref="MapperClass"/> model by resolving source and
    /// destination types, matching common properties, and parsing custom mapping configurations.
    /// </summary>
    /// <param name="context">The generator attribute syntax context.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation.</param>
    /// <returns>A <see cref="MapperClass"/> model if valid; otherwise <see langword="null"/>.</returns>
    private static MapperClass? SemanticTransform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.TargetSymbol is not INamedTypeSymbol targetSymbol)
            return null;

        var baseType = FindMapperBaseType(targetSymbol);
        if (baseType == null
            || baseType.TypeArguments.Length != 2
            || baseType.TypeArguments[0] is not INamedTypeSymbol sourceType
            || baseType.TypeArguments[1] is not INamedTypeSymbol destinationType)
        {
            return null;
        }

        var customMappings = ParseCreateMapMethod(targetSymbol, cancellationToken);
        var constructorParameters = GetConstructorParameterNames(destinationType);
        var mappings = BuildPropertyMappings(sourceType, destinationType, customMappings, constructorParameters, cancellationToken);
        var imports = CollectImports(context.TargetNode);

        return CreateMapperClassModel(targetSymbol, sourceType, destinationType, constructorParameters, mappings, imports);
    }

    /// <summary>
    /// Creates the <see cref="MapperClass"/> model from the resolved type symbols and property mappings.
    /// </summary>
    /// <param name="targetSymbol">The mapper class symbol.</param>
    /// <param name="sourceType">The source type symbol.</param>
    /// <param name="destinationType">The destination type symbol.</param>
    /// <param name="constructorParameters">Constructor parameter names matched to property names.</param>
    /// <param name="mappings">The resolved property mappings.</param>
    /// <param name="imports">Using directives collected from the mapper's source file.</param>
    /// <returns>A new <see cref="MapperClass"/> model.</returns>
    private static MapperClass CreateMapperClassModel(
        INamedTypeSymbol targetSymbol,
        INamedTypeSymbol sourceType,
        INamedTypeSymbol destinationType,
        string[] constructorParameters,
        List<PropertyMapping> mappings,
        string[] imports)
    {
        var qualifiedName = targetSymbol.ToDisplayString(NameAndNamespaces);

        return new MapperClass
        {
            FullyQualified = targetSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            EntityNamespace = targetSymbol.ContainingNamespace.ToDisplayString(),
            EntityName = targetSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
            OutputFile = $"{qualifiedName}.g.cs",
            SourceClass = CreateMappedClass(sourceType),
            DestinationClass = CreateMappedClass(destinationType),
            ConstructorParameters = [.. constructorParameters],
            Properties = [.. mappings],
            Imports = [.. imports],
        };
    }

    /// <summary>
    /// Builds the list of property mappings by resolving destination properties (including
    /// getter-only properties backed by constructor parameters), then matching each against
    /// custom mappings or auto-matched source properties.
    /// </summary>
    /// <param name="sourceType">The source type symbol.</param>
    /// <param name="destinationType">The destination type symbol.</param>
    /// <param name="customMappings">Custom mappings parsed from <c>ConfigureMapping</c>.</param>
    /// <param name="constructorParameters">Constructor parameter names matched to property names.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation.</param>
    /// <returns>A list of resolved property mappings.</returns>
    private static List<PropertyMapping> BuildPropertyMappings(
        INamedTypeSymbol sourceType,
        INamedTypeSymbol destinationType,
        List<CustomMapping> customMappings,
        string[] constructorParameters,
        CancellationToken cancellationToken)
    {
        var customDestinationMappings = new Dictionary<string, CustomMapping>(StringComparer.Ordinal);
        foreach (var mapping in customMappings)
            customDestinationMappings[mapping.DestinationName] = mapping;

        var destinationProperties = GetDestinationProperties(destinationType, constructorParameters);
        var sourcePropertyNames = GetReadablePropertyNames(sourceType);

        var mappings = new List<PropertyMapping>();
        foreach (var property in destinationProperties)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var mapping = CreatePropertyMapping(property, sourceType, sourcePropertyNames, customDestinationMappings);
            if (mapping != null)
                mappings.Add(mapping);
        }

        return mappings;
    }

    /// <summary>
    /// Collects settable destination properties and expands the list with getter-only
    /// properties that are backed by constructor parameters.
    /// </summary>
    /// <param name="destinationType">The destination type symbol.</param>
    /// <param name="constructorParameters">Constructor parameter names matched to property names.</param>
    /// <returns>A list of destination properties including constructor-backed getter-only properties.</returns>
    private static List<IPropertySymbol> GetDestinationProperties(
        INamedTypeSymbol destinationType,
        string[] constructorParameters)
    {
        var properties = GetSettableProperties(destinationType);

        var settableNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var prop in properties)
            settableNames.Add(prop.Name);

        foreach (var ctorParam in constructorParameters)
        {
            if (settableNames.Contains(ctorParam))
                continue;

            var prop = FindProperty(destinationType, ctorParam);
            if (prop != null)
                properties.Add(prop);
        }

        return properties;
    }

    /// <summary>
    /// Creates a <see cref="PropertyMapping"/> for a single destination property by checking
    /// custom mappings first, then falling back to auto-matching by name.
    /// </summary>
    /// <param name="property">The destination property symbol.</param>
    /// <param name="sourceType">The source type symbol for resolving segment nullability.</param>
    /// <param name="sourcePropertyNames">Set of readable source property names.</param>
    /// <param name="customDestinationMappings">Custom mappings keyed by destination property name.</param>
    /// <returns>A <see cref="PropertyMapping"/>, or <see langword="null"/> if ignored or unmatched.</returns>
    private static PropertyMapping? CreatePropertyMapping(
        IPropertySymbol property,
        INamedTypeSymbol sourceType,
        HashSet<string> sourcePropertyNames,
        Dictionary<string, CustomMapping> customDestinationMappings)
    {
        if (customDestinationMappings.TryGetValue(property.Name, out var custom))
            return custom.IsIgnored ? null : CreateCustomPropertyMapping(property, sourceType, custom);

        if (sourcePropertyNames.Contains(property.Name))
            return CreateAutoPropertyMapping(property);

        return null;
    }

    /// <summary>
    /// Creates a <see cref="PropertyMapping"/> from a custom mapping configuration,
    /// using either a raw source expression or a decomposed property path.
    /// </summary>
    /// <param name="property">The destination property symbol.</param>
    /// <param name="sourceType">The source type symbol for resolving segment nullability.</param>
    /// <param name="custom">The custom mapping configuration.</param>
    /// <returns>A new <see cref="PropertyMapping"/>.</returns>
    private static PropertyMapping CreateCustomPropertyMapping(
        IPropertySymbol property,
        INamedTypeSymbol sourceType,
        CustomMapping custom)
    {
        if (!string.IsNullOrEmpty(custom.SourceExpression))
        {
            return new PropertyMapping
            {
                DestinationName = property.Name,
                SourceExpression = custom.SourceExpression,
                SourceExpressionParameter = custom.SourceExpressionParameter,
                IsDestinationNullable = IsNullable(property),
                IsDestinationString = property.Type.SpecialType == SpecialType.System_String,
                IsReadOnly = IsReadOnly(property),
            };
        }

        return new PropertyMapping
        {
            DestinationName = property.Name,
            SourcePath = [.. custom.SourcePath],
            SourceSegmentNullable = GetSegmentNullability(sourceType, custom.SourcePath),
            IsDestinationNullable = IsNullable(property),
            IsDestinationString = property.Type.SpecialType == SpecialType.System_String,
            IsReadOnly = IsReadOnly(property),
        };
    }

    /// <summary>
    /// Creates a <see cref="PropertyMapping"/> for a destination property that is
    /// auto-matched to a source property with the same name.
    /// </summary>
    /// <param name="property">The destination property symbol.</param>
    /// <returns>A new <see cref="PropertyMapping"/>.</returns>
    private static PropertyMapping CreateAutoPropertyMapping(IPropertySymbol property)
    {
        return new PropertyMapping
        {
            DestinationName = property.Name,
            SourcePath = [property.Name],
            SourceSegmentNullable = [false],
            IsDestinationNullable = IsNullable(property),
            IsDestinationString = property.Type.SpecialType == SpecialType.System_String,
            IsReadOnly = IsReadOnly(property),
        };
    }

    /// <summary>
    /// Walks the inheritance chain to find the <c>Arbiter.Mapping.MapperProfile&lt;TSource, TDestination&gt;</c> base type.
    /// </summary>
    /// <param name="targetSymbol">The mapper class symbol to inspect.</param>
    /// <returns>The generic base type symbol, or <see langword="null"/> if not found.</returns>
    private static INamedTypeSymbol? FindMapperBaseType(INamedTypeSymbol targetSymbol)
    {
        var baseType = targetSymbol.BaseType;

        while (baseType != null)
        {
            if (baseType.IsGenericType
                && string.Equals(baseType.Name, MapperConstants.MapperBaseClassName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(baseType.ContainingNamespace?.ToDisplayString(), MapperConstants.MapperBaseNamespace, StringComparison.OrdinalIgnoreCase)
                && baseType.TypeArguments.Length == 2)
            {
                return baseType;
            }

            baseType = baseType.BaseType;
        }

        return null;
    }

    /// <summary>
    /// Creates a <see cref="MappedClass"/> model from a type symbol.
    /// </summary>
    /// <param name="typeSymbol">The type symbol to convert.</param>
    /// <returns>A new <see cref="MappedClass"/> model.</returns>
    private static MappedClass CreateMappedClass(INamedTypeSymbol typeSymbol)
    {
        return new MappedClass
        {
            FullyQualified = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            EntityNamespace = typeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
            EntityName = typeSymbol.Name,
        };
    }

    /// <summary>
    /// Collects all settable instance properties from the type and its base types.
    /// </summary>
    /// <param name="type">The type symbol to inspect.</param>
    /// <returns>A list of settable instance properties.</returns>
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
    /// Collects the names of all readable instance properties from the type and its base types.
    /// </summary>
    /// <param name="type">The type symbol to inspect.</param>
    /// <returns>A case-insensitive set of readable property names.</returns>
    private static HashSet<string> GetReadablePropertyNames(INamedTypeSymbol type)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var current = type;

        while (current != null && current.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in current.GetMembers())
            {
                if (member is IPropertySymbol prop
                    && !prop.IsStatic
                    && !prop.IsIndexer
                    && prop.GetMethod != null)
                {
                    names.Add(prop.Name);
                }
            }

            current = current.BaseType;
        }

        return names;
    }

    /// <summary>
    /// Determines whether the property type has a nullable annotation.
    /// </summary>
    /// <param name="property">The property symbol to check.</param>
    /// <returns><see langword="true"/> if the property type is nullable annotated.</returns>
    private static bool IsNullable(IPropertySymbol property)
    {
        return property.Type.NullableAnnotation == NullableAnnotation.Annotated;
    }

    /// <summary>
    /// Determines whether the property is read-only.
    /// Returns <see langword="true"/> for getter-only properties (no setter) and
    /// init-only properties; returns <see langword="false"/> only for properties
    /// with a regular <c>set</c> accessor.
    /// </summary>
    /// <param name="property">The property symbol to check.</param>
    /// <returns><see langword="true"/> if the property has no setter or an init-only setter.</returns>
    private static bool IsReadOnly(IPropertySymbol property)
    {
        return property.SetMethod is null || property.SetMethod.IsInitOnly;
    }

    /// <summary>
    /// Returns the constructor parameter names (matched to property names) for the best
    /// public constructor whose parameters all correspond to readable destination properties.
    /// Works for both positional record constructors and class constructors (including
    /// primary constructors). Returns an empty array when no suitable constructor is found.
    /// </summary>
    /// <param name="type">The destination type symbol.</param>
    /// <returns>An array of property names corresponding to constructor parameters, or an empty array.</returns>
    private static string[] GetConstructorParameterNames(INamedTypeSymbol type)
    {
        // build a map of all readable properties (not just settable) so that
        // constructor parameters backed by getter-only properties are matched
        var propertyNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var current = type;
        while (current != null && current.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in current.GetMembers())
            {
                if (member is IPropertySymbol prop
                    && !prop.IsStatic
                    && !prop.IsIndexer
                    && prop.GetMethod != null
                    && !propertyNameMap.ContainsKey(prop.Name))
                {
                    propertyNameMap[prop.Name] = prop.Name;
                }
            }

            current = current.BaseType;
        }

        IMethodSymbol? bestConstructor = null;
        var bestMatchCount = 0;

        foreach (var ctor in type.InstanceConstructors)
        {
            if (ctor.DeclaredAccessibility != Accessibility.Public)
                continue;

            if (ctor.Parameters.Length == 0)
                continue;

            var allMatch = true;
            var matchCount = 0;

            foreach (var param in ctor.Parameters)
            {
                if (propertyNameMap.ContainsKey(param.Name))
                    matchCount++;
                else
                    allMatch = false;
            }

            if (allMatch && matchCount > bestMatchCount)
            {
                bestConstructor = ctor;
                bestMatchCount = matchCount;
            }
        }

        if (bestConstructor == null)
            return [];

        var result = new string[bestConstructor.Parameters.Length];
        for (var i = 0; i < bestConstructor.Parameters.Length; i++)
        {
            var paramName = bestConstructor.Parameters[i].Name;

            result[i] = propertyNameMap.TryGetValue(paramName, out var propName)
                ? propName
                : paramName;
        }

        return result;
    }

    /// <summary>
    /// Computes the nullability of each navigation segment in a property path.
    /// </summary>
    /// <param name="sourceType">The root source type symbol.</param>
    /// <param name="sourcePath">The property path segments to evaluate.</param>
    /// <returns>An array indicating whether each navigation segment is nullable.</returns>
    private static bool[] GetSegmentNullability(INamedTypeSymbol sourceType, string[] sourcePath)
    {
        var result = new bool[sourcePath.Length];
        var currentType = sourceType;

        // check each navigation segment (all except the final property)
        for (int i = 0; i < sourcePath.Length - 1; i++)
        {
            var property = FindProperty(currentType, sourcePath[i]);
            if (property == null)
            {
                // unknown property, assume nullable for safety
                result[i] = true;
                break;
            }

            result[i] = IsNullable(property);

            // resolve the next type in the chain
            if (property.Type is INamedTypeSymbol namedType)
                currentType = namedType;
            else
                result[i] = true; // can't resolve, assume nullable
        }

        // final segment is the leaf property, not a navigation
        result[sourcePath.Length - 1] = false;

        return result;
    }

    /// <summary>
    /// Finds an instance property by name in the type or its base types.
    /// </summary>
    /// <param name="type">The type symbol to search.</param>
    /// <param name="name">The property name to find.</param>
    /// <returns>The property symbol, or <see langword="null"/> if not found.</returns>
    private static IPropertySymbol? FindProperty(INamedTypeSymbol type, string name)
    {
        var current = type;

        while (current != null && current.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in current.GetMembers())
            {
                if (member is IPropertySymbol prop
                    && !prop.IsStatic
                    && string.Equals(prop.Name, name, StringComparison.Ordinal))
                {
                    return prop;
                }
            }

            current = current.BaseType;
        }

        return null;
    }

    /// <summary>
    /// Parses the <c>ConfigureMapping</c> method body across all partial declarations to extract custom mappings.
    /// </summary>
    /// <param name="targetSymbol">The mapper class symbol.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation.</param>
    /// <returns>A list of custom mapping configurations.</returns>
    private static List<CustomMapping> ParseCreateMapMethod(INamedTypeSymbol targetSymbol, CancellationToken cancellationToken)
    {
        var results = new List<CustomMapping>();

        // search all partial declarations for CreateMap method
        foreach (var syntaxRef in targetSymbol.DeclaringSyntaxReferences)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var syntax = syntaxRef.GetSyntax(cancellationToken);
            if (syntax is not ClassDeclarationSyntax classDeclaration)
                continue;

            foreach (var member in classDeclaration.Members)
            {
                if (member is not MethodDeclarationSyntax method)
                    continue;

                if (!string.Equals(method.Identifier.Text, MapperConstants.ConfigureMappingMethodName, StringComparison.Ordinal))
                    continue;

                if (method.Body == null)
                    continue;

                ParseCreateMapBody(method.Body, results);
            }
        }

        return results;
    }

    /// <summary>
    /// Extracts custom mapping configurations from <c>Property(...).From(...)</c>,
    /// <c>Property(...).Value(...)</c>, and <c>Property(...).Ignore()</c> call chains.
    /// </summary>
    /// <param name="body">The method body block syntax.</param>
    /// <param name="results">The list to append parsed custom mappings to.</param>
    private static void ParseCreateMapBody(BlockSyntax body, List<CustomMapping> results)
    {
        foreach (var statement in body.Statements)
        {
            if (statement is not ExpressionStatementSyntax expressionStatement)
                continue;

            if (expressionStatement.Expression is not InvocationExpressionSyntax outerInvocation)
                continue;

            if (outerInvocation.Expression is not MemberAccessExpressionSyntax outerMemberAccess)
                continue;

            var methodName = outerMemberAccess.Name.Identifier.Text;

            // the receiver should be the Property(...) invocation
            if (outerMemberAccess.Expression is not InvocationExpressionSyntax propertyInvocation)
                continue;

            var destName = GetDestinationPropertyName(propertyInvocation);
            if (destName == null)
                continue;

            if (string.Equals(methodName, MapperConstants.IgnoreMethodName, StringComparison.Ordinal))
            {
                results.Add(new CustomMapping
                {
                    DestinationName = destName,
                    SourcePath = [],
                    IsIgnored = true,
                });
            }
            else if (string.Equals(methodName, MapperConstants.FromMethodName, StringComparison.Ordinal))
            {
                var mapping = GetSourceMapping(outerInvocation, destName);
                if (mapping != null)
                {
                    results.Add(mapping.Value);
                }
            }
            else if (string.Equals(methodName, MapperConstants.ValueMethodName, StringComparison.Ordinal))
            {
                var valueExpression = GetValueExpression(outerInvocation);
                if (valueExpression != null)
                {
                    results.Add(new CustomMapping
                    {
                        DestinationName = destName,
                        SourcePath = [],
                        SourceExpression = valueExpression,
                        IsIgnored = false,
                    });
                }
            }
        }
    }

    /// <summary>
    /// Extracts the destination property name from a <c>Property(d =&gt; d.Name)</c> invocation.
    /// </summary>
    /// <param name="propertyInvocation">The <c>Property(...)</c> invocation syntax.</param>
    /// <returns>The destination property name, or <see langword="null"/> if it cannot be resolved.</returns>
    private static string? GetDestinationPropertyName(InvocationExpressionSyntax propertyInvocation)
    {
        if (propertyInvocation.ArgumentList.Arguments.Count != 1)
            return null;

        var lambdaBody = GetLambdaBody(propertyInvocation.ArgumentList.Arguments[0].Expression);
        if (lambdaBody is MemberAccessExpressionSyntax memberAccess)
            return memberAccess.Name.Identifier.Text;

        return null;
    }

    /// <summary>
    /// Extracts the constant value expression text from a <c>Value(...)</c> invocation.
    /// </summary>
    /// <param name="valueInvocation">The <c>Value(...)</c> invocation syntax.</param>
    /// <returns>The expression text, or <see langword="null"/> if it cannot be resolved.</returns>
    private static string? GetValueExpression(InvocationExpressionSyntax valueInvocation)
    {
        if (valueInvocation.ArgumentList.Arguments.Count != 1)
            return null;

        var argument = valueInvocation.ArgumentList.Arguments[0].Expression;
        var text = argument.ToString();

        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    /// <summary>
    /// Parses a <c>From(s =&gt; ...)</c> invocation into a <see cref="CustomMapping"/>,
    /// using a decomposed property path when possible or falling back to a raw expression.
    /// </summary>
    /// <param name="mapFromInvocation">The <c>From(...)</c> invocation syntax.</param>
    /// <param name="destName">The destination property name.</param>
    /// <returns>A <see cref="CustomMapping"/>, or <see langword="null"/> if the lambda cannot be parsed.</returns>
    private static CustomMapping? GetSourceMapping(InvocationExpressionSyntax mapFromInvocation, string destName)
    {
        if (mapFromInvocation.ArgumentList.Arguments.Count != 1)
            return null;

        var argument = mapFromInvocation.ArgumentList.Arguments[0].Expression;

        // extract lambda parameter name and body
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
            return null;
        }

        // try to decompose into property-only chain
        var path = new List<string>();
        var expression = lambdaBody;

        while (expression is MemberAccessExpressionSyntax memberAccess)
        {
            path.Insert(0, memberAccess.Name.Identifier.Text);
            expression = memberAccess.Expression;

            // unwrap null-forgiving operator (e.g. src.Priority!.Name)
            if (expression is PostfixUnaryExpressionSyntax postfix)
                expression = postfix.Operand;
        }

        // if we successfully walked to the lambda parameter, use decomposed path
        if (expression is IdentifierNameSyntax identifier
            && string.Equals(identifier.Identifier.Text, parameterName, StringComparison.Ordinal)
            && path.Count > 0)
        {
            return new CustomMapping
            {
                DestinationName = destName,
                SourcePath = [.. path],
                IsIgnored = false,
            };
        }

        // fall back to raw expression mode for complex expressions
        // (method calls, LINQ, ternary, etc.)
        var expressionText = lambdaBody.ToString();
        if (string.IsNullOrWhiteSpace(expressionText))
            return null;

        return new CustomMapping
        {
            DestinationName = destName,
            SourcePath = [],
            SourceExpression = expressionText,
            SourceExpressionParameter = parameterName,
            IsIgnored = false,
        };
    }

    /// <summary>
    /// Extracts the body expression from a simple or parenthesized lambda.
    /// </summary>
    /// <param name="expression">The expression syntax to inspect.</param>
    /// <returns>The lambda body expression, or <see langword="null"/> if not a lambda.</returns>
    private static ExpressionSyntax? GetLambdaBody(ExpressionSyntax expression)
    {
        if (expression is SimpleLambdaExpressionSyntax simpleLambda)
            return simpleLambda.Body as ExpressionSyntax;

        if (expression is ParenthesizedLambdaExpressionSyntax parenLambda)
            return parenLambda.Body as ExpressionSyntax;

        return null;
    }

    /// <summary>
    /// Walks the syntax tree from <paramref name="targetNode"/> upward and collects all
    /// <c>using</c> directives declared at the compilation-unit and namespace levels.
    /// These are forwarded to the generated file so that raw source expressions that
    /// reference types from imported namespaces continue to compile.
    /// </summary>
    /// <param name="targetNode">The mapper class syntax node.</param>
    /// <returns>A sorted, deduplicated array of using directive texts (e.g. <c>"using Domain.Constants;"</c>).</returns>
    private static string[] CollectImports(SyntaxNode targetNode)
    {
        var result = new SortedSet<string>(StringComparer.Ordinal);

        var current = targetNode.Parent;
        while (current != null)
        {
            IEnumerable<UsingDirectiveSyntax>? usings = current switch
            {
                BaseNamespaceDeclarationSyntax ns => ns.Usings,
                CompilationUnitSyntax cu => cu.Usings,
                _ => null,
            };

            if (usings != null)
            {
                foreach (var u in usings)
                {
                    var text = u.WithoutTrivia().ToString();
                    if (!string.IsNullOrWhiteSpace(text))
                        result.Add(text);
                }
            }

            current = current.Parent;
        }

        return [.. result];
    }


    /// <summary>
    /// Intermediate representation of a custom property mapping parsed from a <c>ConfigureMapping</c> method body.
    /// </summary>
    private struct CustomMapping
    {
        public string DestinationName;
        public string[] SourcePath;
        public string SourceExpression;
        public string SourceExpressionParameter;
        public bool IsIgnored;
    }
}

