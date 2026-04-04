using System.Collections.Immutable;

using Arbiter.Mapping.Generators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Arbiter.Mapping.Tests;

public class MapperDiagnosticAnalyzerTests
{
    /// <summary>
    /// Minimal stubs for Arbiter.Mapping types needed by the analyzer.
    /// Included as source in each test compilation so the analyzer can resolve symbols.
    /// </summary>
    private const string MappingStubs = """
        using System;
        using System.Linq;
        using System.Linq.Expressions;

        namespace Arbiter.Mapping
        {
            [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
            public class GenerateMapperAttribute : Attribute { }

            public abstract class MapperProfile<TSource, TDestination>
            {
                protected virtual void ConfigureMapping(MappingBuilder<TSource, TDestination> mapping) { }
            }

            public class MappingBuilder<TSource, TDestination>
            {
                public PropertyBuilder<TSource, TDestination, TMember> Property<TMember>(
                    Expression<Func<TDestination, TMember>> destinationMember)
                    => new PropertyBuilder<TSource, TDestination, TMember>(destinationMember);
            }

            public class PropertyBuilder<TSource, TDestination, TMember>
            {
                public PropertyBuilder(Expression<Func<TDestination, TMember>> destinationMember) { }
                public void From<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceExpression) { }
                public void Value(TMember value) { }
                public void Ignore() { }
            }
        }
        """;

    // ───────────────────────────────────────────────────────────────
    // ARB0001: Class must be partial
    // ───────────────────────────────────────────────────────────────

    [Test]
    public async Task ARB0001_ClassNotPartial_ReportsDiagnostic()
    {
        var source = """
            using Arbiter.Mapping;

            namespace TestApp;

            [GenerateMapper]
            public class MyMapper : MapperProfile<Source, Dest> { }

            public class Source { public int Id { get; set; } }
            public class Dest { public int Id { get; set; } }
            """;

        var diagnostics = await RunAnalyzerAsync(source);

        diagnostics.Should().ContainSingle(d => d.Id == "ARB0001");
    }

    [Test]
    public async Task ARB0001_ClassIsPartial_NoDiagnostic()
    {
        var source = """
            using Arbiter.Mapping;

            namespace TestApp;

            [GenerateMapper]
            public partial class MyMapper : MapperProfile<Source, Dest> { }

            public class Source { public int Id { get; set; } }
            public class Dest { public int Id { get; set; } }
            """;

        var diagnostics = await RunAnalyzerAsync(source);

        diagnostics.Should().NotContain(d => d.Id == "ARB0001");
    }

    // ───────────────────────────────────────────────────────────────
    // ARB0002: Class must inherit MapperProfile<,>
    // ───────────────────────────────────────────────────────────────

    [Test]
    public async Task ARB0002_DoesNotInheritMapperProfile_ReportsDiagnostic()
    {
        var source = """
            using Arbiter.Mapping;

            namespace TestApp;

            [GenerateMapper]
            public partial class MyMapper { }
            """;

        var diagnostics = await RunAnalyzerAsync(source);

        diagnostics.Should().ContainSingle(d => d.Id == "ARB0002");
    }

    [Test]
    public async Task ARB0002_InheritsMapperProfile_NoDiagnostic()
    {
        var source = """
            using Arbiter.Mapping;

            namespace TestApp;

            [GenerateMapper]
            public partial class MyMapper : MapperProfile<Source, Dest> { }

            public class Source { public int Id { get; set; } }
            public class Dest { public int Id { get; set; } }
            """;

        var diagnostics = await RunAnalyzerAsync(source);

        diagnostics.Should().NotContain(d => d.Id == "ARB0002");
    }

    // ───────────────────────────────────────────────────────────────
    // ARB0003: ConfigureMapping contains unsupported statement
    // ───────────────────────────────────────────────────────────────

    [Test]
    public async Task ARB0003_VariableDeclaration_ReportsDiagnostic()
    {
        var source = """
            using Arbiter.Mapping;

            namespace TestApp;

            [GenerateMapper]
            public partial class MyMapper : MapperProfile<Source, Dest>
            {
                protected override void ConfigureMapping(MappingBuilder<Source, Dest> mapping)
                {
                    var x = 42;
                }
            }

            public class Source { public int Id { get; set; } }
            public class Dest { public int Id { get; set; } }
            """;

        var diagnostics = await RunAnalyzerAsync(source);

        diagnostics.Should().ContainSingle(d => d.Id == "ARB0003");
        diagnostics.First(d => d.Id == "ARB0003").GetMessage().Should().Contain("variable declaration");
    }

    [Test]
    public async Task ARB0003_IfStatement_ReportsDiagnostic()
    {
        var source = """
            using Arbiter.Mapping;

            namespace TestApp;

            [GenerateMapper]
            public partial class MyMapper : MapperProfile<Source, Dest>
            {
                protected override void ConfigureMapping(MappingBuilder<Source, Dest> mapping)
                {
                    if (true) { }
                }
            }

            public class Source { public int Id { get; set; } }
            public class Dest { public int Id { get; set; } }
            """;

        var diagnostics = await RunAnalyzerAsync(source);

        diagnostics.Should().ContainSingle(d => d.Id == "ARB0003");
        diagnostics.First(d => d.Id == "ARB0003").GetMessage().Should().Contain("if statement");
    }

    [Test]
    public async Task ARB0003_ForEachLoop_ReportsDiagnostic()
    {
        var source = """
            using Arbiter.Mapping;

            namespace TestApp;

            [GenerateMapper]
            public partial class MyMapper : MapperProfile<Source, Dest>
            {
                protected override void ConfigureMapping(MappingBuilder<Source, Dest> mapping)
                {
                    foreach (var item in new int[0]) { }
                }
            }

            public class Source { public int Id { get; set; } }
            public class Dest { public int Id { get; set; } }
            """;

        var diagnostics = await RunAnalyzerAsync(source);

        diagnostics.Should().ContainSingle(d => d.Id == "ARB0003");
        diagnostics.First(d => d.Id == "ARB0003").GetMessage().Should().Contain("foreach loop");
    }

    [Test]
    public async Task ARB0003_ReturnStatement_ReportsDiagnostic()
    {
        var source = """
            using Arbiter.Mapping;

            namespace TestApp;

            [GenerateMapper]
            public partial class MyMapper : MapperProfile<Source, Dest>
            {
                protected override void ConfigureMapping(MappingBuilder<Source, Dest> mapping)
                {
                    return;
                }
            }

            public class Source { public int Id { get; set; } }
            public class Dest { public int Id { get; set; } }
            """;

        var diagnostics = await RunAnalyzerAsync(source);

        diagnostics.Should().ContainSingle(d => d.Id == "ARB0003");
    }

    [Test]
    public async Task ARB0003_NonMappingMethodCall_ReportsDiagnostic()
    {
        var source = """
            using Arbiter.Mapping;

            namespace TestApp;

            [GenerateMapper]
            public partial class MyMapper : MapperProfile<Source, Dest>
            {
                protected override void ConfigureMapping(MappingBuilder<Source, Dest> mapping)
                {
                    System.Console.WriteLine("hello");
                }
            }

            public class Source { public int Id { get; set; } }
            public class Dest { public int Id { get; set; } }
            """;

        var diagnostics = await RunAnalyzerAsync(source);

        diagnostics.Should().ContainSingle(d => d.Id == "ARB0003");
    }

    [Test]
    public async Task ARB0003_ValidMappingCalls_NoDiagnostic()
    {
        var source = """
            using Arbiter.Mapping;

            namespace TestApp;

            [GenerateMapper]
            public partial class MyMapper : MapperProfile<Source, Dest>
            {
                protected override void ConfigureMapping(MappingBuilder<Source, Dest> mapping)
                {
                    mapping.Property(d => d.Name).From(s => s.Name);
                    mapping.Property(d => d.Value).Value(42);
                    mapping.Property(d => d.Extra).Ignore();
                }
            }

            public class Source { public string Name { get; set; } }
            public class Dest
            {
                public string Name { get; set; }
                public int Value { get; set; }
                public string Extra { get; set; }
            }
            """;

        var diagnostics = await RunAnalyzerAsync(source);

        diagnostics.Should().NotContain(d => d.Id == "ARB0003");
    }

    [Test]
    public async Task ARB0003_MixedValidAndInvalid_ReportsOnlyInvalid()
    {
        var source = """
            using Arbiter.Mapping;

            namespace TestApp;

            [GenerateMapper]
            public partial class MyMapper : MapperProfile<Source, Dest>
            {
                protected override void ConfigureMapping(MappingBuilder<Source, Dest> mapping)
                {
                    mapping.Property(d => d.Name).From(s => s.Name);
                    var x = 1;
                    mapping.Property(d => d.Extra).Ignore();
                }
            }

            public class Source { public string Name { get; set; } }
            public class Dest
            {
                public string Name { get; set; }
                public string Extra { get; set; }
            }
            """;

        var diagnostics = await RunAnalyzerAsync(source);

        diagnostics.Where(d => d.Id == "ARB0003").Should().HaveCount(1);
    }

    // ───────────────────────────────────────────────────────────────
    // ARB0004: Property type mismatch
    // ───────────────────────────────────────────────────────────────

    [Test]
    public async Task ARB0004_IncompatibleAutoMatchedTypes_ReportsDiagnostic()
    {
        var source = """
            using Arbiter.Mapping;

            namespace TestApp;

            [GenerateMapper]
            public partial class MyMapper : MapperProfile<Source, Dest> { }

            public class Source { public string Name { get; set; } }
            public class Dest { public int Name { get; set; } }
            """;

        var diagnostics = await RunAnalyzerAsync(source);

        diagnostics.Should().ContainSingle(d => d.Id == "ARB0004");
        var diag = diagnostics.First(d => d.Id == "ARB0004");
        diag.GetMessage().Should().Contain("Name");
        diag.GetMessage().Should().Contain("string");
        diag.GetMessage().Should().Contain("int");
    }

    [Test]
    public async Task ARB0004_CompatibleAutoMatchedTypes_NoDiagnostic()
    {
        var source = """
            using Arbiter.Mapping;

            namespace TestApp;

            [GenerateMapper]
            public partial class MyMapper : MapperProfile<Source, Dest> { }

            public class Source { public int Id { get; set; } public string Name { get; set; } }
            public class Dest { public int Id { get; set; } public string Name { get; set; } }
            """;

        var diagnostics = await RunAnalyzerAsync(source);

        diagnostics.Should().NotContain(d => d.Id == "ARB0004");
    }

    [Test]
    public async Task ARB0004_ImplicitNumericConversion_NoDiagnostic()
    {
        var source = """
            using Arbiter.Mapping;

            namespace TestApp;

            [GenerateMapper]
            public partial class MyMapper : MapperProfile<Source, Dest> { }

            public class Source { public int Value { get; set; } }
            public class Dest { public long Value { get; set; } }
            """;

        var diagnostics = await RunAnalyzerAsync(source);

        diagnostics.Should().NotContain(d => d.Id == "ARB0004");
    }

    [Test]
    public async Task ARB0004_ExplicitConversionOnly_ReportsDiagnostic()
    {
        var source = """
            using Arbiter.Mapping;

            namespace TestApp;

            [GenerateMapper]
            public partial class MyMapper : MapperProfile<Source, Dest> { }

            public class Source { public long Value { get; set; } }
            public class Dest { public int Value { get; set; } }
            """;

        var diagnostics = await RunAnalyzerAsync(source);

        diagnostics.Should().ContainSingle(d => d.Id == "ARB0004");
    }

    [Test]
    public async Task ARB0004_NullableToNonNullableSameType_NoDiagnostic()
    {
        var source = """
            using Arbiter.Mapping;

            namespace TestApp;

            [GenerateMapper]
            public partial class MyMapper : MapperProfile<Source, Dest> { }

            public class Source { public int? Value { get; set; } }
            public class Dest { public int Value { get; set; } }
            """;

        var diagnostics = await RunAnalyzerAsync(source);

        diagnostics.Should().NotContain(d => d.Id == "ARB0004");
    }

    [Test]
    public async Task ARB0004_CustomMappedProperty_SkipsCheck()
    {
        var source = """
            using Arbiter.Mapping;

            namespace TestApp;

            [GenerateMapper]
            public partial class MyMapper : MapperProfile<Source, Dest>
            {
                protected override void ConfigureMapping(MappingBuilder<Source, Dest> mapping)
                {
                    mapping.Property(d => d.Name).Ignore();
                }
            }

            public class Source { public int Name { get; set; } }
            public class Dest { public string Name { get; set; } }
            """;

        var diagnostics = await RunAnalyzerAsync(source);

        diagnostics.Should().NotContain(d => d.Id == "ARB0004");
    }

    [Test]
    public async Task ARB0004_MultipleIncompatibleProperties_ReportsEach()
    {
        var source = """
            using Arbiter.Mapping;

            namespace TestApp;

            [GenerateMapper]
            public partial class MyMapper : MapperProfile<Source, Dest> { }

            public class Source
            {
                public string Id { get; set; }
                public string Count { get; set; }
            }
            public class Dest
            {
                public int Id { get; set; }
                public int Count { get; set; }
            }
            """;

        var diagnostics = await RunAnalyzerAsync(source);

        diagnostics.Where(d => d.Id == "ARB0004").Should().HaveCount(2);
    }

    // ───────────────────────────────────────────────────────────────
    // ARB0005: Duplicate destination mapping
    // ───────────────────────────────────────────────────────────────

    [Test]
    public async Task ARB0005_DuplicateDestination_ReportsDiagnostic()
    {
        var source = """
            using Arbiter.Mapping;

            namespace TestApp;

            [GenerateMapper]
            public partial class MyMapper : MapperProfile<Source, Dest>
            {
                protected override void ConfigureMapping(MappingBuilder<Source, Dest> mapping)
                {
                    mapping.Property(d => d.Name).From(s => s.First);
                    mapping.Property(d => d.Name).From(s => s.Last);
                }
            }

            public class Source
            {
                public string First { get; set; }
                public string Last { get; set; }
            }
            public class Dest { public string Name { get; set; } }
            """;

        var diagnostics = await RunAnalyzerAsync(source);

        diagnostics.Should().ContainSingle(d => d.Id == "ARB0005");
        diagnostics.First(d => d.Id == "ARB0005").GetMessage().Should().Contain("Name");
    }

    [Test]
    public async Task ARB0005_UniqueDestinations_NoDiagnostic()
    {
        var source = """
            using Arbiter.Mapping;

            namespace TestApp;

            [GenerateMapper]
            public partial class MyMapper : MapperProfile<Source, Dest>
            {
                protected override void ConfigureMapping(MappingBuilder<Source, Dest> mapping)
                {
                    mapping.Property(d => d.Name).From(s => s.Name);
                    mapping.Property(d => d.Value).Value(42);
                }
            }

            public class Source { public string Name { get; set; } }
            public class Dest
            {
                public string Name { get; set; }
                public int Value { get; set; }
            }
            """;

        var diagnostics = await RunAnalyzerAsync(source);

        diagnostics.Should().NotContain(d => d.Id == "ARB0005");
    }

    // ───────────────────────────────────────────────────────────────
    // ARB0006: Invalid mapping call pattern
    // ───────────────────────────────────────────────────────────────

    [Test]
    public async Task ARB0006_UnrecognizedMethodOnPropertyChain_ReportsDiagnostic()
    {
        var source = """
            using Arbiter.Mapping;

            namespace TestApp;

            [GenerateMapper]
            public partial class MyMapper : MapperProfile<Source, Dest>
            {
                protected override void ConfigureMapping(MappingBuilder<Source, Dest> mapping)
                {
                    mapping.Property(d => d.Name).ToString();
                }
            }

            public class Source { public string Name { get; set; } }
            public class Dest { public string Name { get; set; } }
            """;

        var diagnostics = await RunAnalyzerAsync(source);

        diagnostics.Should().ContainSingle(d => d.Id == "ARB0006");
    }

    // ───────────────────────────────────────────────────────────────
    // No diagnostics for valid mapper
    // ───────────────────────────────────────────────────────────────

    [Test]
    public async Task ValidMapper_NoDiagnostics()
    {
        var source = """
            using Arbiter.Mapping;

            namespace TestApp;

            [GenerateMapper]
            public partial class MyMapper : MapperProfile<Source, Dest>
            {
                protected override void ConfigureMapping(MappingBuilder<Source, Dest> mapping)
                {
                    mapping.Property(d => d.FullName).From(s => s.FirstName + " " + s.LastName);
                    mapping.Property(d => d.Extra).Ignore();
                }
            }

            public class Source
            {
                public int Id { get; set; }
                public string FirstName { get; set; }
                public string LastName { get; set; }
            }
            public class Dest
            {
                public int Id { get; set; }
                public string FullName { get; set; }
                public string Extra { get; set; }
            }
            """;

        var diagnostics = await RunAnalyzerAsync(source);

        diagnostics.Should().BeEmpty();
    }

    [Test]
    public async Task ValidMapperWithoutConfigureMapping_NoDiagnostics()
    {
        var source = """
            using Arbiter.Mapping;

            namespace TestApp;

            [GenerateMapper]
            public partial class MyMapper : MapperProfile<Source, Dest> { }

            public class Source
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }
            public class Dest
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }
            """;

        var diagnostics = await RunAnalyzerAsync(source);

        diagnostics.Should().BeEmpty();
    }

    [Test]
    public async Task ClassWithoutGenerateMapper_NoDiagnostics()
    {
        var source = """
            using Arbiter.Mapping;

            namespace TestApp;

            public class MyMapper : MapperProfile<Source, Dest> { }

            public class Source { public int Id { get; set; } }
            public class Dest { public int Id { get; set; } }
            """;

        var diagnostics = await RunAnalyzerAsync(source);

        diagnostics.Should().BeEmpty();
    }

    // ───────────────────────────────────────────────────────────────
    // Helper
    // ───────────────────────────────────────────────────────────────

    private static async Task<List<Diagnostic>> RunAnalyzerAsync(string source)
    {
        var syntaxTrees = new[]
        {
            CSharpSyntaxTree.ParseText(MappingStubs, path: "MappingStubs.cs"),
            CSharpSyntaxTree.ParseText(source, path: "TestSource.cs"),
        };

        // Collect references from the current runtime
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Cast<MetadataReference>()
            .ToList();

        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: syntaxTrees,
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithNullableContextOptions(NullableContextOptions.Enable));

        var analyzer = new MapperDiagnosticAnalyzer();
        var compilationWithAnalyzers = compilation.WithAnalyzers(
            ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));

        var allDiagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        // Return only diagnostics from our analyzer (ARB*)
        return allDiagnostics
            .Where(d => d.Id.StartsWith("ARB", StringComparison.Ordinal))
            .ToList();
    }
}
