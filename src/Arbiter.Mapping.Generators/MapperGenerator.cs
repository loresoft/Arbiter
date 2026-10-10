#pragma warning disable MA0051 // Method is too long

using Arbiter.Mapping.Generators.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

        foreach (var diagnosticInfo in mapperClass.Diagnostics)
        {
            var arguments = diagnosticInfo.Arguments.AsArray();
            var diagnostic = Diagnostic.Create(MapperDiagnostics.NestedMappingCycle, diagnosticInfo.ToLocation(), arguments);
            context.ReportDiagnostic(diagnostic);
        }

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

        var rootScope = ParseCreateMapMethod(targetSymbol, context.SemanticModel.Compilation, cancellationToken);
        var constructorParameters = GetConstructorParameterNames(destinationType);
        var buildContext = new BuildContext(targetSymbol, cancellationToken);
        buildContext.InProgress.Add(GetPairKey(sourceType, destinationType));

        var mappings = BuildPropertyMappings(sourceType, destinationType, rootScope, constructorParameters, buildContext);
        var imports = CollectImports(context.TargetNode);

        var mapperClass = CreateMapperClassModel(targetSymbol, sourceType, destinationType, constructorParameters, mappings, imports);

        return mapperClass with
        {
            NestedMappings = [.. buildContext.Helpers],
            Diagnostics = [.. buildContext.Diagnostics],
        };
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
    /// <param name="scope">The configuration scope parsed from <c>ConfigureMapping</c> or a nested configuration.</param>
    /// <param name="constructorParameters">Constructor parameter names matched to property names.</param>
    /// <param name="buildContext">The shared build state for nested helpers and diagnostics.</param>
    /// <returns>A list of resolved property mappings.</returns>
    private static List<PropertyMapping> BuildPropertyMappings(
        INamedTypeSymbol sourceType,
        INamedTypeSymbol destinationType,
        MappingScope scope,
        string[] constructorParameters,
        BuildContext buildContext)
    {
        var customDestinationMappings = new Dictionary<string, CustomMapping>(StringComparer.Ordinal);
        foreach (var mapping in scope.CustomMappings)
            customDestinationMappings[mapping.DestinationName] = mapping;

        var destinationProperties = GetDestinationProperties(destinationType, constructorParameters);
        var sourcePropertyNames = GetReadablePropertyNames(sourceType);

        var mappings = new List<PropertyMapping>();
        foreach (var property in destinationProperties)
        {
            buildContext.CancellationToken.ThrowIfCancellationRequested();

            var mapping = CreatePropertyMapping(property, sourceType, sourcePropertyNames, customDestinationMappings, scope, buildContext);
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
    /// <param name="scope">The configuration scope used to resolve nested type mappings.</param>
    /// <param name="buildContext">The shared build state for nested helpers and diagnostics.</param>
    /// <returns>A <see cref="PropertyMapping"/>, or <see langword="null"/> if ignored or unmatched.</returns>
    private static PropertyMapping? CreatePropertyMapping(
        IPropertySymbol property,
        INamedTypeSymbol sourceType,
        HashSet<string> sourcePropertyNames,
        Dictionary<string, CustomMapping> customDestinationMappings,
        MappingScope scope,
        BuildContext buildContext)
    {
        PropertyMapping mapping;
        ITypeSymbol? sourceValueType;
        TypeConfiguration? propertyConfiguration = null;

        if (customDestinationMappings.TryGetValue(property.Name, out var custom) && custom.Nested == null)
        {
            if (custom.IsIgnored)
                return null;

            mapping = CreateCustomPropertyMapping(property, sourceType, custom);
            sourceValueType = string.IsNullOrEmpty(custom.SourceExpression)
                ? GetPathLeafType(sourceType, custom.SourcePath)
                : null;
        }
        else if (sourcePropertyNames.Contains(property.Name))
        {
            mapping = CreateAutoPropertyMapping(property);
            sourceValueType = FindProperty(sourceType, property.Name)?.Type;
            propertyConfiguration = custom.Nested;
        }
        else
        {
            return null;
        }

        if (sourceValueType == null)
            return mapping;

        var (kind, methodName) = ResolveNestedMapping(sourceValueType, property.Type, propertyConfiguration, scope, buildContext);
        if (kind == MappingKind.Direct)
            return mapping;

        return mapping with
        {
            Kind = kind,
            NestedMethodName = methodName,
        };
    }

    /// <summary>
    /// Resolves whether a source value should be deep cloned into the destination type, and creates
    /// the required helper methods. Only type pairs configured with <c>Map&lt;,&gt;</c> or
    /// <c>MapWith&lt;,&gt;</c> are deep cloned; all other values are assigned directly.
    /// </summary>
    /// <param name="sourceType">The source value type.</param>
    /// <param name="destinationType">The destination property type.</param>
    /// <param name="propertyConfiguration">The property-level <c>MapWith</c> configuration, if any.</param>
    /// <param name="scope">The configuration scope to resolve type-level configurations from.</param>
    /// <param name="buildContext">The shared build state.</param>
    /// <returns>The mapping kind and generated helper method name.</returns>
    private static (MappingKind Kind, string MethodName) ResolveNestedMapping(
        ITypeSymbol sourceType,
        ITypeSymbol destinationType,
        TypeConfiguration? propertyConfiguration,
        MappingScope scope,
        BuildContext buildContext)
    {
        // nested object
        if (TypeClassifier.IsComplex(sourceType) && TypeClassifier.IsComplex(destinationType))
        {
            var elementMethod = ResolveComplexHelper(sourceType, destinationType, propertyConfiguration, scope, buildContext);
            return elementMethod == null
                ? (MappingKind.Direct, string.Empty)
                : (MappingKind.Complex, elementMethod);
        }

        // dictionary
        var destinationValueType = TypeClassifier.GetDestinationDictionaryTypes(destinationType, out var destinationKeyType);
        if (destinationValueType != null)
        {
            var sourceValueType = TypeClassifier.GetSourceDictionaryTypes(sourceType, out var sourceKeyType);
            if (sourceValueType == null || !SymbolEqualityComparer.Default.Equals(sourceKeyType, destinationKeyType))
                return (MappingKind.Direct, string.Empty);

            var valueMethod = ResolveComplexHelper(sourceValueType, destinationValueType, propertyConfiguration, scope, buildContext);
            if (valueMethod == null)
                return (MappingKind.Direct, string.Empty);

            var dictionaryMethod = GetOrCreateCollectionHelper(
                MappingKind.Dictionary,
                CollectionKind.Dictionary,
                sourceValueType,
                destinationValueType,
                destinationKeyType,
                valueMethod,
                buildContext);

            return (MappingKind.Dictionary, dictionaryMethod);
        }

        // collection
        var destinationElementType = TypeClassifier.GetDestinationElementType(destinationType, out var collectionKind);
        if (destinationElementType != null)
        {
            var sourceElementType = TypeClassifier.GetSourceElementType(sourceType);
            if (sourceElementType == null)
                return (MappingKind.Direct, string.Empty);

            var elementMethod = ResolveComplexHelper(sourceElementType, destinationElementType, propertyConfiguration, scope, buildContext);
            if (elementMethod == null)
                return (MappingKind.Direct, string.Empty);

            var collectionMethod = GetOrCreateCollectionHelper(
                MappingKind.Collection,
                collectionKind,
                sourceElementType,
                destinationElementType,
                null,
                elementMethod,
                buildContext);

            return (MappingKind.Collection, collectionMethod);
        }

        return (MappingKind.Direct, string.Empty);
    }

    /// <summary>
    /// Finds the configuration for a nested object type pair and returns the name of the helper that clones it.
    /// </summary>
    /// <returns>The helper method name, or <see langword="null"/> if the pair is not configured, not constructible, or cyclic.</returns>
    private static string? ResolveComplexHelper(
        ITypeSymbol sourceType,
        ITypeSymbol destinationType,
        TypeConfiguration? propertyConfiguration,
        MappingScope scope,
        BuildContext buildContext)
    {
        if (!TypeClassifier.IsComplex(sourceType) || !TypeClassifier.IsComplex(destinationType))
            return null;

        var configuration = propertyConfiguration != null && propertyConfiguration.Matches(sourceType, destinationType)
            ? propertyConfiguration
            : FindTypeConfiguration(scope, sourceType, destinationType);

        if (configuration == null)
            return null;

        return GetOrCreateComplexHelper(
            (INamedTypeSymbol)sourceType,
            (INamedTypeSymbol)destinationType,
            configuration.Scope,
            buildContext);
    }

    /// <summary>
    /// Finds the closest type-level <c>Map&lt;,&gt;</c> configuration for the type pair, walking from the
    /// specified scope up to the root scope. Within a scope, the last declaration wins.
    /// </summary>
    private static TypeConfiguration? FindTypeConfiguration(MappingScope? scope, ITypeSymbol sourceType, ITypeSymbol destinationType)
    {
        while (scope != null)
        {
            for (var i = scope.TypeMaps.Count - 1; i >= 0; i--)
            {
                if (scope.TypeMaps[i].Matches(sourceType, destinationType))
                    return scope.TypeMaps[i];
            }

            scope = scope.Parent;
        }

        return null;
    }

    /// <summary>
    /// Gets or creates the helper that deep clones a nested object using the specified configuration scope.
    /// Reports a cycle diagnostic and returns <see langword="null"/> when the type pair is already being built.
    /// </summary>
    private static string? GetOrCreateComplexHelper(
        INamedTypeSymbol sourceType,
        INamedTypeSymbol destinationType,
        MappingScope configurationScope,
        BuildContext buildContext)
    {
        var sourceName = sourceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var destinationName = destinationType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        var key = $"complex|{configurationScope.Id}|{sourceName}|{destinationName}";
        if (buildContext.HelperNames.TryGetValue(key, out var existing))
            return existing;

        var pairKey = GetPairKey(sourceType, destinationType);
        if (buildContext.InProgress.Contains(pairKey))
        {
            buildContext.ReportCycle(sourceType, destinationType);
            return null;
        }

        var constructorParameters = GetConstructorParameterNames(destinationType);
        if (constructorParameters.Length == 0 && !HasPublicParameterlessConstructor(destinationType))
            return null;

        var methodName = buildContext.ReserveMethodName($"Map{sourceType.Name}To{destinationType.Name}");

        buildContext.InProgress.Add(pairKey);
        var properties = BuildPropertyMappings(sourceType, destinationType, configurationScope, constructorParameters, buildContext);
        buildContext.InProgress.Remove(pairKey);

        buildContext.HelperNames[key] = methodName;
        buildContext.Helpers.Add(new NestedMapping
        {
            MethodName = methodName,
            Kind = MappingKind.Complex,
            SourceType = sourceName,
            DestinationType = destinationName,
            ConstructorParameters = [.. constructorParameters],
            Properties = [.. properties],
        });

        return methodName;
    }

    /// <summary>
    /// Gets or creates the helper that deep clones a collection or dictionary element by element.
    /// </summary>
    private static string GetOrCreateCollectionHelper(
        MappingKind kind,
        CollectionKind collectionKind,
        ITypeSymbol sourceElementType,
        ITypeSymbol destinationElementType,
        ITypeSymbol? keyType,
        string elementMethodName,
        BuildContext buildContext)
    {
        var sourceElementName = sourceElementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var destinationElementName = destinationElementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var keyName = keyType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? string.Empty;
        var isElementNullable = destinationElementType.NullableAnnotation == NullableAnnotation.Annotated;

        var key = $"{collectionKind}|{elementMethodName}|{keyName}|{isElementNullable}";
        if (buildContext.HelperNames.TryGetValue(key, out var existing))
            return existing;

        var methodName = buildContext.ReserveMethodName($"{elementMethodName}{collectionKind}");

        string sourceTypeName;
        string destinationTypeName;

        if (collectionKind == CollectionKind.Dictionary)
        {
            sourceTypeName = $"global::System.Collections.Generic.IEnumerable<global::System.Collections.Generic.KeyValuePair<{keyName}, {sourceElementName}>>";
            destinationTypeName = $"global::System.Collections.Generic.Dictionary<{keyName}, {destinationElementName}>";
        }
        else
        {
            sourceTypeName = $"global::System.Collections.Generic.IEnumerable<{sourceElementName}>";
            destinationTypeName = collectionKind switch
            {
                CollectionKind.Array => $"{destinationElementName}[]",
                CollectionKind.HashSet => $"global::System.Collections.Generic.HashSet<{destinationElementName}>",
                _ => $"global::System.Collections.Generic.List<{destinationElementName}>",
            };
        }

        buildContext.HelperNames[key] = methodName;
        buildContext.Helpers.Add(new NestedMapping
        {
            MethodName = methodName,
            Kind = kind,
            SourceType = sourceTypeName,
            DestinationType = destinationTypeName,
            CollectionKind = collectionKind,
            ElementMethodName = elementMethodName,
            ElementSourceType = sourceElementName,
            ElementDestinationType = destinationElementName,
            IsElementNullable = isElementNullable,
            KeyType = keyName,
        });

        return methodName;
    }

    /// <summary>
    /// Determines whether the type has an accessible public parameterless constructor.
    /// </summary>
    private static bool HasPublicParameterlessConstructor(INamedTypeSymbol type)
    {
        foreach (var ctor in type.InstanceConstructors)
        {
            if (ctor.DeclaredAccessibility == Accessibility.Public && ctor.Parameters.Length == 0)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Resolves the type of the leaf property in a source property path.
    /// </summary>
    private static ITypeSymbol? GetPathLeafType(INamedTypeSymbol sourceType, string[] sourcePath)
    {
        if (sourcePath == null || sourcePath.Length == 0)
            return null;

        INamedTypeSymbol? current = sourceType;
        ITypeSymbol? leaf = null;

        foreach (var segment in sourcePath)
        {
            if (current == null)
                return null;

            var property = FindProperty(current, segment);
            if (property == null)
                return null;

            leaf = property.Type;
            current = property.Type as INamedTypeSymbol;
        }

        return leaf;
    }

    /// <summary>
    /// Creates a key that identifies a source/destination type pair, used for cycle detection.
    /// </summary>
    private static string GetPairKey(ITypeSymbol sourceType, ITypeSymbol destinationType)
    {
        return sourceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            + "|"
            + destinationType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
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
    /// <param name="compilation">The compilation used to resolve nested mapping type arguments.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation.</param>
    /// <returns>The root mapping scope containing custom mappings and nested type configurations.</returns>
    private static MappingScope ParseCreateMapMethod(INamedTypeSymbol targetSymbol, Compilation compilation, CancellationToken cancellationToken)
    {
        var scope = new MappingScope(null);

        // search all partial declarations for CreateMap method
        foreach (var syntaxRef in targetSymbol.DeclaringSyntaxReferences)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var syntax = syntaxRef.GetSyntax(cancellationToken);
            if (syntax is not ClassDeclarationSyntax classDeclaration)
                continue;

            SemanticModel? semanticModel = null;

            foreach (var member in classDeclaration.Members)
            {
                if (member is not MethodDeclarationSyntax method)
                    continue;

                if (!string.Equals(method.Identifier.Text, MapperConstants.ConfigureMappingMethodName, StringComparison.Ordinal))
                    continue;

                if (method.Body == null)
                    continue;

                semanticModel ??= compilation.GetSemanticModel(syntaxRef.SyntaxTree);

                ParseCreateMapBody(method.Body.Statements, scope, semanticModel, cancellationToken);
            }
        }

        return scope;
    }

    /// <summary>
    /// Extracts custom mapping configurations from <c>Property(...).From(...)</c>,
    /// <c>Property(...).Value(...)</c>, <c>Property(...).Ignore()</c>, <c>Property(...).MapWith&lt;,&gt;(...)</c>
    /// and <c>Map&lt;,&gt;(...)</c> call chains.
    /// </summary>
    /// <param name="statements">The statements of the configuration body.</param>
    /// <param name="scope">The scope to append parsed mappings to.</param>
    /// <param name="semanticModel">The semantic model used to resolve nested mapping type arguments.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation.</param>
    private static void ParseCreateMapBody(
        IEnumerable<StatementSyntax> statements,
        MappingScope scope,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        foreach (var statement in statements)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (statement is not ExpressionStatementSyntax expressionStatement)
                continue;

            ParseCreateMapExpression(expressionStatement.Expression, scope, semanticModel, cancellationToken);
        }
    }

    /// <summary>
    /// Parses a single mapping configuration expression into the specified scope.
    /// </summary>
    /// <param name="expression">The configuration expression.</param>
    /// <param name="scope">The scope to append parsed mappings to.</param>
    /// <param name="semanticModel">The semantic model used to resolve nested mapping type arguments.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation.</param>
    private static void ParseCreateMapExpression(
        ExpressionSyntax expression,
        MappingScope scope,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        {
            if (expression is not InvocationExpressionSyntax outerInvocation)
                return;

            if (outerInvocation.Expression is not MemberAccessExpressionSyntax outerMemberAccess)
                return;

            var methodName = outerMemberAccess.Name.Identifier.Text;

            // type-level nested configuration: mapping.Map<TSource, TDestination>(...), optionally chained
            if (string.Equals(methodName, MapperConstants.MapMethodName, StringComparison.Ordinal))
            {
                var typeConfiguration = ParseNestedConfiguration(outerInvocation, scope, semanticModel, cancellationToken);
                if (typeConfiguration != null)
                    scope.TypeMaps.Add(typeConfiguration);

                if (outerMemberAccess.Expression is InvocationExpressionSyntax chained)
                    ParseCreateMapExpression(chained, scope, semanticModel, cancellationToken);

                return;
            }

            // the receiver should be the Property(...) invocation
            if (outerMemberAccess.Expression is not InvocationExpressionSyntax propertyInvocation)
                return;

            var destName = GetDestinationPropertyName(propertyInvocation);
            if (destName == null)
                return;

            var results = scope.CustomMappings;

            if (string.Equals(methodName, MapperConstants.MapWithMethodName, StringComparison.Ordinal))
            {
                var nested = ParseNestedConfiguration(outerInvocation, scope, semanticModel, cancellationToken);
                if (nested != null)
                {
                    results.Add(new CustomMapping
                    {
                        DestinationName = destName,
                        SourcePath = [],
                        IsIgnored = false,
                        Nested = nested,
                    });
                }
            }
            else if (string.Equals(methodName, MapperConstants.IgnoreMethodName, StringComparison.Ordinal))
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
    /// Parses a <c>Map&lt;TSource, TDestination&gt;(...)</c> or <c>MapWith&lt;TSource, TDestination&gt;(...)</c>
    /// invocation into a <see cref="TypeConfiguration"/> with its own child scope.
    /// </summary>
    /// <param name="invocation">The invocation syntax.</param>
    /// <param name="parent">The declaring scope.</param>
    /// <param name="semanticModel">The semantic model used to resolve type arguments.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation.</param>
    /// <returns>The parsed configuration, or <see langword="null"/> if the type arguments cannot be resolved.</returns>
    private static TypeConfiguration? ParseNestedConfiguration(
        InvocationExpressionSyntax invocation,
        MappingScope parent,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(invocation, cancellationToken);
        var method = symbolInfo.Symbol as IMethodSymbol;
        if (method == null && symbolInfo.CandidateSymbols.Length > 0)
            method = symbolInfo.CandidateSymbols[0] as IMethodSymbol;

        if (method == null || method.TypeArguments.Length != 2)
            return null;

        var nestedScope = new MappingScope(parent);

        if (invocation.ArgumentList.Arguments.Count == 1)
        {
            var argument = invocation.ArgumentList.Arguments[0].Expression;
            var body = argument switch
            {
                SimpleLambdaExpressionSyntax simple => (CSharpSyntaxNode)simple.Body,
                ParenthesizedLambdaExpressionSyntax paren => paren.Body,
                _ => null,
            };

            if (body is BlockSyntax block)
                ParseCreateMapBody(block.Statements, nestedScope, semanticModel, cancellationToken);
            else if (body is ExpressionSyntax expressionBody)
                ParseCreateMapExpression(expressionBody, nestedScope, semanticModel, cancellationToken);
        }

        return new TypeConfiguration(method.TypeArguments[0], method.TypeArguments[1], nestedScope);
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
        public TypeConfiguration? Nested;
    }

    /// <summary>
    /// A configuration scope corresponding to a <c>ConfigureMapping</c> body or a nested
    /// <c>Map&lt;,&gt;</c> / <c>MapWith&lt;,&gt;</c> lambda body.
    /// </summary>
    private sealed class MappingScope
    {
        private static int _nextId;

        public MappingScope(MappingScope? parent)
        {
            Parent = parent;
            Id = Interlocked.Increment(ref _nextId);
        }

        public int Id { get; }

        public MappingScope? Parent { get; }

        public List<CustomMapping> CustomMappings { get; } = [];

        public List<TypeConfiguration> TypeMaps { get; } = [];
    }

    /// <summary>
    /// A nested type pair configuration declared by <c>Map&lt;,&gt;</c> or <c>MapWith&lt;,&gt;</c>.
    /// </summary>
    private sealed class TypeConfiguration(ITypeSymbol sourceType, ITypeSymbol destinationType, MappingScope scope)
    {
        public ITypeSymbol SourceType { get; } = sourceType;

        public ITypeSymbol DestinationType { get; } = destinationType;

        public MappingScope Scope { get; } = scope;

        public bool Matches(ITypeSymbol source, ITypeSymbol destination)
        {
            return SymbolEqualityComparer.Default.Equals(SourceType, source)
                && SymbolEqualityComparer.Default.Equals(DestinationType, destination);
        }
    }

    /// <summary>
    /// Shared state while building the mapper model: generated helpers, reserved method names,
    /// type pairs currently being built (for cycle detection), and diagnostics.
    /// </summary>
    private sealed class BuildContext(INamedTypeSymbol mapperSymbol, CancellationToken cancellationToken)
    {
        private readonly HashSet<string> _methodNames = new(StringComparer.Ordinal);
        private readonly HashSet<string> _reportedCycles = new(StringComparer.Ordinal);

        public CancellationToken CancellationToken { get; } = cancellationToken;

        public List<NestedMapping> Helpers { get; } = [];

        public Dictionary<string, string> HelperNames { get; } = new(StringComparer.Ordinal);

        public HashSet<string> InProgress { get; } = new(StringComparer.Ordinal);

        public List<DiagnosticInfo> Diagnostics { get; } = [];

        public string ReserveMethodName(string baseName)
        {
            var name = baseName;
            var counter = 2;

            while (!_methodNames.Add(name))
                name = baseName + counter++;

            return name;
        }

        public void ReportCycle(ITypeSymbol sourceType, ITypeSymbol destinationType)
        {
            var sourceName = sourceType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            var destinationName = destinationType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

            if (!_reportedCycles.Add(sourceName + "|" + destinationName))
                return;

            var location = mapperSymbol.Locations.Length > 0 ? mapperSymbol.Locations[0] : null;
            var lineSpan = location?.GetLineSpan();

            Diagnostics.Add(new DiagnosticInfo
            {
                Id = MapperDiagnostics.NestedMappingCycle.Id,
                FilePath = lineSpan?.Path ?? string.Empty,
                TextSpan = location?.SourceSpan ?? default,
                LineSpan = lineSpan?.Span ?? default,
                Arguments = [mapperSymbol.Name, sourceName, destinationName],
            });
        }
    }
}

