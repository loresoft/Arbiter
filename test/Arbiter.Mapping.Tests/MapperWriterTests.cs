using Arbiter.Mapping.Generators;
using Arbiter.Mapping.Generators.Infrastructure;
using Arbiter.Mapping.Generators.Models;

namespace Arbiter.Mapping.Tests;

public class MapperWriterTests
{
    [Test]
    public async Task GenerateSimplePropertyMapping()
    {
        var mapperClass = new MapperClass
        {
            FullyQualified = "global::TestApp.Mappers.LocationMapper",
            EntityNamespace = "TestApp.Mappers",
            EntityName = "LocationMapper",
            OutputFile = "TestApp.Mappers.LocationMapper.g.cs",
            SourceClass = new MappedClass
            {
                FullyQualified = "global::TestApp.Models.Location",
                EntityNamespace = "TestApp.Models",
                EntityName = "Location"
            },
            DestinationClass = new MappedClass
            {
                FullyQualified = "global::TestApp.Models.LocationReadModel",
                EntityNamespace = "TestApp.Models",
                EntityName = "LocationReadModel"
            },
            Properties = new EquatableArray<PropertyMapping>([
                new PropertyMapping
                {
                    DestinationName = "Id",
                    SourcePath = new EquatableArray<string>(["Id"]),
                    SourceSegmentNullable = new EquatableArray<bool>([false])
                },
                new PropertyMapping
                {
                    DestinationName = "Name",
                    SourcePath = new EquatableArray<string>(["Name"]),
                    SourceSegmentNullable = new EquatableArray<bool>([false])
                },
                new PropertyMapping
                {
                    DestinationName = "City",
                    SourcePath = new EquatableArray<string>(["City"]),
                    SourceSegmentNullable = new EquatableArray<bool>([false]),
                    IsDestinationNullable = true
                }
            ])
        };

        var output = MapperWriter.Generate(mapperClass);

        await Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("[GeneratedCode");
    }

    [Test]
    public async Task GenerateNullableNavigationMapping()
    {
        var mapperClass = new MapperClass
        {
            FullyQualified = "global::TestApp.Mappers.PersonMapper",
            EntityNamespace = "TestApp.Mappers",
            EntityName = "PersonMapper",
            OutputFile = "TestApp.Mappers.PersonMapper.g.cs",
            SourceClass = new MappedClass
            {
                FullyQualified = "global::TestApp.Models.Person",
                EntityNamespace = "TestApp.Models",
                EntityName = "Person"
            },
            DestinationClass = new MappedClass
            {
                FullyQualified = "global::TestApp.Models.PersonModel",
                EntityNamespace = "TestApp.Models",
                EntityName = "PersonModel"
            },
            Properties = new EquatableArray<PropertyMapping>([
                new PropertyMapping
                {
                    DestinationName = "Id",
                    SourcePath = new EquatableArray<string>(["Id"]),
                    SourceSegmentNullable = new EquatableArray<bool>([false])
                },
                new PropertyMapping
                {
                    DestinationName = "FirstName",
                    SourcePath = new EquatableArray<string>(["FirstName"]),
                    SourceSegmentNullable = new EquatableArray<bool>([false])
                },
                new PropertyMapping
                {
                    DestinationName = "DepartmentName",
                    SourcePath = new EquatableArray<string>(["Department", "Name"]),
                    SourceSegmentNullable = new EquatableArray<bool>([true, false]),
                    IsDestinationNullable = true
                }
            ])
        };

        var output = MapperWriter.Generate(mapperClass);

        await Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("[GeneratedCode");
    }

    [Test]
    public async Task GenerateNullableNavigationNonNullableDestination()
    {
        var mapperClass = new MapperClass
        {
            FullyQualified = "global::TestApp.Mappers.OrderMapper",
            EntityNamespace = "TestApp.Mappers",
            EntityName = "OrderMapper",
            OutputFile = "TestApp.Mappers.OrderMapper.g.cs",
            SourceClass = new MappedClass
            {
                FullyQualified = "global::TestApp.Models.Order",
                EntityNamespace = "TestApp.Models",
                EntityName = "Order"
            },
            DestinationClass = new MappedClass
            {
                FullyQualified = "global::TestApp.Models.OrderModel",
                EntityNamespace = "TestApp.Models",
                EntityName = "OrderModel"
            },
            Properties = new EquatableArray<PropertyMapping>([
                new PropertyMapping
                {
                    DestinationName = "CustomerName",
                    SourcePath = new EquatableArray<string>(["Customer", "Name"]),
                    SourceSegmentNullable = new EquatableArray<bool>([true, false]),
                    IsDestinationNullable = false,
                    IsDestinationString = true
                },
                new PropertyMapping
                {
                    DestinationName = "CustomerCode",
                    SourcePath = new EquatableArray<string>(["Customer", "Code"]),
                    SourceSegmentNullable = new EquatableArray<bool>([true, false]),
                    IsDestinationNullable = false,
                    IsDestinationString = false
                }
            ])
        };

        var output = MapperWriter.Generate(mapperClass);

        await Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("[GeneratedCode");
    }

    [Test]
    public async Task GenerateSourceExpressionMapping()
    {
        var mapperClass = new MapperClass
        {
            FullyQualified = "global::TestApp.Mappers.PersonSummaryMapper",
            EntityNamespace = "TestApp.Mappers",
            EntityName = "PersonSummaryMapper",
            OutputFile = "TestApp.Mappers.PersonSummaryMapper.g.cs",
            SourceClass = new MappedClass
            {
                FullyQualified = "global::TestApp.Models.Person",
                EntityNamespace = "TestApp.Models",
                EntityName = "Person"
            },
            DestinationClass = new MappedClass
            {
                FullyQualified = "global::TestApp.Models.PersonSummary",
                EntityNamespace = "TestApp.Models",
                EntityName = "PersonSummary"
            },
            Properties = new EquatableArray<PropertyMapping>([
                new PropertyMapping
                {
                    DestinationName = "Id",
                    SourcePath = new EquatableArray<string>(["Id"]),
                    SourceSegmentNullable = new EquatableArray<bool>([false])
                },
                new PropertyMapping
                {
                    DestinationName = "FullName",
                    SourceExpression = "src.FirstName + \" \" + src.LastName",
                    SourceExpressionParameter = "src",
                    SourcePath = new EquatableArray<string>([]),
                    SourceSegmentNullable = new EquatableArray<bool>([])
                },
                new PropertyMapping
                {
                    DestinationName = "AddressCount",
                    SourceExpression = "p.Addresses.Count()",
                    SourceExpressionParameter = "p",
                    SourcePath = new EquatableArray<string>([]),
                    SourceSegmentNullable = new EquatableArray<bool>([])
                }
            ])
        };

        var output = MapperWriter.Generate(mapperClass);

        await Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("[GeneratedCode");
    }

    [Test]
    public async Task GenerateIgnoredProperties()
    {
        var mapperClass = new MapperClass
        {
            FullyQualified = "global::TestApp.Mappers.ItemMapper",
            EntityNamespace = "TestApp.Mappers",
            EntityName = "ItemMapper",
            OutputFile = "TestApp.Mappers.ItemMapper.g.cs",
            SourceClass = new MappedClass
            {
                FullyQualified = "global::TestApp.Models.Item",
                EntityNamespace = "TestApp.Models",
                EntityName = "Item"
            },
            DestinationClass = new MappedClass
            {
                FullyQualified = "global::TestApp.Models.ItemModel",
                EntityNamespace = "TestApp.Models",
                EntityName = "ItemModel"
            },
            Properties = new EquatableArray<PropertyMapping>([
                new PropertyMapping
                {
                    DestinationName = "Id",
                    SourcePath = new EquatableArray<string>(["Id"]),
                    SourceSegmentNullable = new EquatableArray<bool>([false])
                },
                new PropertyMapping
                {
                    DestinationName = "Name",
                    SourcePath = new EquatableArray<string>(["Name"]),
                    SourceSegmentNullable = new EquatableArray<bool>([false])
                },
                new PropertyMapping
                {
                    DestinationName = "InternalCode",
                    SourcePath = new EquatableArray<string>(["InternalCode"]),
                    SourceSegmentNullable = new EquatableArray<bool>([false]),
                    IsIgnored = true
                }
            ])
        };

        var output = MapperWriter.Generate(mapperClass);

        await Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("[GeneratedCode");
    }

    [Test]
    public async Task GenerateDeepNullableNavigation()
    {
        var mapperClass = new MapperClass
        {
            FullyQualified = "global::TestApp.Mappers.EmployeeMapper",
            EntityNamespace = "TestApp.Mappers",
            EntityName = "EmployeeMapper",
            OutputFile = "TestApp.Mappers.EmployeeMapper.g.cs",
            SourceClass = new MappedClass
            {
                FullyQualified = "global::TestApp.Models.Employee",
                EntityNamespace = "TestApp.Models",
                EntityName = "Employee"
            },
            DestinationClass = new MappedClass
            {
                FullyQualified = "global::TestApp.Models.EmployeeModel",
                EntityNamespace = "TestApp.Models",
                EntityName = "EmployeeModel"
            },
            Properties = new EquatableArray<PropertyMapping>([
                new PropertyMapping
                {
                    DestinationName = "CompanyName",
                    SourcePath = new EquatableArray<string>(["Department", "Company", "Name"]),
                    SourceSegmentNullable = new EquatableArray<bool>([true, true, false]),
                    IsDestinationNullable = true
                },
                new PropertyMapping
                {
                    DestinationName = "CompanyCode",
                    SourcePath = new EquatableArray<string>(["Department", "Company", "Code"]),
                    SourceSegmentNullable = new EquatableArray<bool>([true, true, false]),
                    IsDestinationNullable = false,
                    IsDestinationString = true
                }
            ])
        };

        var output = MapperWriter.Generate(mapperClass);

        await Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("[GeneratedCode");
    }

    [Test]
    public void GenerateThrowsOnNull()
    {
        var action = () => MapperWriter.Generate(null!);

        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("mapperClass");
    }

    [Test]
    public async Task GenerateConstructorParameterMapping()
    {
        var mapperClass = new MapperClass
        {
            FullyQualified = "global::TestApp.Mappers.PersonRecordMapper",
            EntityNamespace = "TestApp.Mappers",
            EntityName = "PersonRecordMapper",
            OutputFile = "TestApp.Mappers.PersonRecordMapper.g.cs",
            SourceClass = new MappedClass
            {
                FullyQualified = "global::TestApp.Models.Person",
                EntityNamespace = "TestApp.Models",
                EntityName = "Person"
            },
            DestinationClass = new MappedClass
            {
                FullyQualified = "global::TestApp.Models.PersonRecord",
                EntityNamespace = "TestApp.Models",
                EntityName = "PersonRecord"
            },
            ConstructorParameters = new EquatableArray<string>(["Id", "FirstName", "LastName", "Email", "FullName", "Age"]),
            Properties = new EquatableArray<PropertyMapping>([
                new PropertyMapping
                {
                    DestinationName = "Id",
                    SourcePath = new EquatableArray<string>(["Id"]),
                    SourceSegmentNullable = new EquatableArray<bool>([false]),
                    IsReadOnly = true
                },
                new PropertyMapping
                {
                    DestinationName = "FirstName",
                    SourcePath = new EquatableArray<string>(["FirstName"]),
                    SourceSegmentNullable = new EquatableArray<bool>([false]),
                    IsReadOnly = true
                },
                new PropertyMapping
                {
                    DestinationName = "LastName",
                    SourcePath = new EquatableArray<string>(["LastName"]),
                    SourceSegmentNullable = new EquatableArray<bool>([false]),
                    IsReadOnly = true
                },
                new PropertyMapping
                {
                    DestinationName = "Email",
                    SourcePath = new EquatableArray<string>(["Email"]),
                    SourceSegmentNullable = new EquatableArray<bool>([false]),
                    IsReadOnly = true
                },
                new PropertyMapping
                {
                    DestinationName = "FullName",
                    SourceExpression = "src.FirstName + \" \" + src.LastName",
                    SourceExpressionParameter = "src",
                    SourcePath = new EquatableArray<string>([]),
                    SourceSegmentNullable = new EquatableArray<bool>([]),
                    IsReadOnly = true
                },
                new PropertyMapping
                {
                    DestinationName = "Age",
                    SourceExpression = "src.BirthDate.Year",
                    SourceExpressionParameter = "src",
                    SourcePath = new EquatableArray<string>([]),
                    SourceSegmentNullable = new EquatableArray<bool>([]),
                    IsReadOnly = true
                }
            ])
        };

        var output = MapperWriter.Generate(mapperClass);

        await Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("[GeneratedCode");
    }

    [Test]
    public async Task GenerateInitOnlySkipsCopyMapper()
    {
        var mapperClass = new MapperClass
        {
            FullyQualified = "global::TestApp.Mappers.RecordMapper",
            EntityNamespace = "TestApp.Mappers",
            EntityName = "RecordMapper",
            OutputFile = "TestApp.Mappers.RecordMapper.g.cs",
            SourceClass = new MappedClass
            {
                FullyQualified = "global::TestApp.Models.Source",
                EntityNamespace = "TestApp.Models",
                EntityName = "Source"
            },
            DestinationClass = new MappedClass
            {
                FullyQualified = "global::TestApp.Models.DestRecord",
                EntityNamespace = "TestApp.Models",
                EntityName = "DestRecord"
            },
            Properties = new EquatableArray<PropertyMapping>([
                new PropertyMapping
                {
                    DestinationName = "Id",
                    SourcePath = new EquatableArray<string>(["Id"]),
                    SourceSegmentNullable = new EquatableArray<bool>([false]),
                    IsReadOnly = true
                },
                new PropertyMapping
                {
                    DestinationName = "Name",
                    SourcePath = new EquatableArray<string>(["Name"]),
                    SourceSegmentNullable = new EquatableArray<bool>([false]),
                    IsReadOnly = true
                }
            ])
        };

        var output = MapperWriter.Generate(mapperClass);

        await Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("[GeneratedCode");
    }

    [Test]
    public async Task GenerateSourceExpressionWithShortParameterName()
    {
        var mapperClass = new MapperClass
        {
            FullyQualified = "global::TestApp.Mappers.OrderSummaryMapper",
            EntityNamespace = "TestApp.Mappers",
            EntityName = "OrderSummaryMapper",
            OutputFile = "TestApp.Mappers.OrderSummaryMapper.g.cs",
            SourceClass = new MappedClass
            {
                FullyQualified = "global::TestApp.Models.Order",
                EntityNamespace = "TestApp.Models",
                EntityName = "Order"
            },
            DestinationClass = new MappedClass
            {
                FullyQualified = "global::TestApp.Models.OrderSummary",
                EntityNamespace = "TestApp.Models",
                EntityName = "OrderSummary"
            },
            Properties = new EquatableArray<PropertyMapping>([
                new PropertyMapping
                {
                    DestinationName = "Id",
                    SourcePath = new EquatableArray<string>(["Id"]),
                    SourceSegmentNullable = new EquatableArray<bool>([false])
                },
                new PropertyMapping
                {
                    DestinationName = "ItemCount",
                    SourceExpression = "s.Items.Count()",
                    SourceExpressionParameter = "s",
                    SourcePath = new EquatableArray<string>([]),
                    SourceSegmentNullable = new EquatableArray<bool>([])
                },
                new PropertyMapping
                {
                    DestinationName = "Summary",
                    SourceExpression = "s.Description + \" (\" + s.Status + \")\"",
                    SourceExpressionParameter = "s",
                    SourcePath = new EquatableArray<string>([]),
                    SourceSegmentNullable = new EquatableArray<bool>([])
                }
            ])
        };

        var output = MapperWriter.Generate(mapperClass);

        await Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("[GeneratedCode");
    }

    [Test]
    public async Task GenerateSourceExpressionWithImportedNamespace()
    {
        // Arrange: a mapper whose raw expression references PayerTypes.Primary from an
        // imported namespace. The Imports list intentionally includes a duplicate of a
        // standard using ("using System.Linq;") to verify it is not emitted twice.
        var mapperClass = new MapperClass
        {
            FullyQualified = "global::TestApp.Mappers.CaseMapper",
            EntityNamespace = "TestApp.Mappers",
            EntityName = "CaseMapper",
            OutputFile = "TestApp.Mappers.CaseMapper.g.cs",
            SourceClass = new MappedClass
            {
                FullyQualified = "global::TestApp.Models.Case",
                EntityNamespace = "TestApp.Models",
                EntityName = "Case"
            },
            DestinationClass = new MappedClass
            {
                FullyQualified = "global::TestApp.Models.CaseModel",
                EntityNamespace = "TestApp.Models",
                EntityName = "CaseModel"
            },
            Imports = new EquatableArray<string>([
                "using System.Linq;",              // duplicate of standard - must not appear twice
                "using TestApp.Domain.Constants;"  // new - must be forwarded to generated output
            ]),
            Properties = new EquatableArray<PropertyMapping>([
                new PropertyMapping
                {
                    DestinationName = "Id",
                    SourcePath = new EquatableArray<string>(["Id"]),
                    SourceSegmentNullable = new EquatableArray<bool>([false])
                },
                new PropertyMapping
                {
                    DestinationName = "SubscriberId",
                    SourceExpression = "src.Client.Payers.Where(p => p.PayerTypeId == PayerTypes.Primary).OrderByDescending(p => p.Created).Select(p => p.SubscriberId).FirstOrDefault()",
                    SourceExpressionParameter = "src",
                    SourcePath = new EquatableArray<string>([]),
                    SourceSegmentNullable = new EquatableArray<bool>([]),
                    IsDestinationNullable = true
                }
            ])
        };

        var output = MapperWriter.Generate(mapperClass);

        await Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("[GeneratedCode");
    }

    [Test]
    public async Task GenerateGenericDestinationMapping()
    {
        var mapperClass = new MapperClass
        {
            FullyQualified = "global::TestApp.Mappers.PersonResultMapper",
            EntityNamespace = "TestApp.Mappers",
            EntityName = "PersonResultMapper",
            OutputFile = "TestApp.Mappers.PersonResultMapper.g.cs",
            SourceClass = new MappedClass
            {
                FullyQualified = "global::TestApp.Models.Person",
                EntityNamespace = "TestApp.Models",
                EntityName = "Person"
            },
            DestinationClass = new MappedClass
            {
                FullyQualified = "global::TestApp.Models.Result<global::TestApp.Models.PersonModel>",
                EntityNamespace = "TestApp.Models",
                EntityName = "Result<PersonModel>"
            },
            Properties = new EquatableArray<PropertyMapping>([
                new PropertyMapping
                {
                    DestinationName = "Id",
                    SourcePath = new EquatableArray<string>(["Id"]),
                    SourceSegmentNullable = new EquatableArray<bool>([false])
                },
                new PropertyMapping
                {
                    DestinationName = "Name",
                    SourcePath = new EquatableArray<string>(["FirstName"]),
                    SourceSegmentNullable = new EquatableArray<bool>([false])
                },
                new PropertyMapping
                {
                    DestinationName = "IsActive",
                    SourcePath = new EquatableArray<string>(["IsActive"]),
                    SourceSegmentNullable = new EquatableArray<bool>([false])
                }
            ])
        };

        var output = MapperWriter.Generate(mapperClass);

        await Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("[GeneratedCode");
    }
}
