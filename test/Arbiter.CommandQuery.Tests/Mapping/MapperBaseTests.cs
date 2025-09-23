using System.Linq.Expressions;

using Arbiter.CommandQuery.Mapping;
using Arbiter.CommandQuery.Tests.Models;

using PersonEntity = Arbiter.CommandQuery.Tests.Models.Person;

namespace Arbiter.CommandQuery.Tests.Mapping;

public class MapperBaseTests
{
    private readonly PersonToPersonModelMapper _personToModelMapper = new();
    private readonly PersonModelToPersonMapper _modelToPersonMapper = new();
    private readonly PersonToPersonErrorMapper _errorMapper = new();
    private readonly PersonToPersonRecordMapper _recordMapper = new();

    [Test]
    public void Map_ValidPerson_ReturnsCorrectPersonModel()
    {
        // Arrange
        var department = new Department { Id = 1, Name = "Engineering" };
        var person = new PersonEntity
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            BirthDate = new DateTime(1990, 5, 15),
            Department = department,
            Addresses = new List<Address>
            {
                new() { Id = 1, Street = "123 Main St", City = "Springfield", State = "IL", ZipCode = "62701" },
                new() { Id = 2, Street = "456 Oak Ave", City = "Springfield", State = "IL", ZipCode = "62702" }
            }
        };

        // Act
        var result = _personToModelMapper.Map(person);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.FullName.Should().Be("John Doe");
        result.Email.Should().Be("john.doe@example.com");
        result.Age.Should().BeInRange(DateTime.Now.Year - 1990 - 1, DateTime.Now.Year - 1990 + 1);
        result.DepartmentName.Should().Be("Engineering");
        result.AddressCount.Should().Be(2);
    }

    [Test]
    public void Map_ValidPerson_ReturnsCorrectPersonRecord()
    {
        // Arrange
        var department = new Department { Id = 1, Name = "Engineering" };
        var person = new PersonEntity
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            BirthDate = new DateTime(1990, 5, 15),
            Department = department,
            Addresses = new List<Address>
            {
                new() { Id = 1, Street = "123 Main St", City = "Springfield", State = "IL", ZipCode = "62701" },
                new() { Id = 2, Street = "456 Oak Ave", City = "Springfield", State = "IL", ZipCode = "62702" }
            }
        };

        // Act
        var result = _recordMapper.Map(person);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.FullName.Should().Be("John Doe");
        result.Email.Should().Be("john.doe@example.com");
        result.Age.Should().BeInRange(DateTime.Now.Year - 1990 - 1, DateTime.Now.Year - 1990 + 1);
        result.DepartmentName.Should().Be("Engineering");
        result.AddressCount.Should().Be(2);
    }

    [Test]
    public void Map_PersonWithNullDepartment_HandlesNullCorrectly()
    {
        // Arrange
        var person = new PersonEntity
        {
            Id = 2,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            BirthDate = new DateTime(1985, 10, 20),
            Department = null,
            Addresses = new List<Address>()
        };

        // Act
        var result = _personToModelMapper.Map(person);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(2);
        result.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Smith");
        result.FullName.Should().Be("Jane Smith");
        result.Email.Should().Be("jane.smith@example.com");
        result.DepartmentName.Should().BeNull();
        result.AddressCount.Should().Be(0);
    }

    [Test]
    public void Map_NullPerson_ReturnsNull()
    {
        // Act
        var result = _personToModelMapper.Map(null);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void Map_ValidPersonModel_ReturnsCorrectPerson()
    {
        // Arrange
        var personModel = new PersonModel
        {
            Id = 3,
            FirstName = "Bob",
            LastName = "Johnson",
            Email = "bob.johnson@example.com",
            Age = 35
        };

        // Act
        var result = _modelToPersonMapper.Map(personModel);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(3);
        result.FirstName.Should().Be("Bob");
        result.LastName.Should().Be("Johnson");
        result.Email.Should().Be("bob.johnson@example.com");
    }

    [Test]
    public void Map_PersonModelWithEmptyName_HandlesCorrectly()
    {
        // Arrange
        var personModel = new PersonModel
        {
            Id = 4,
            FirstName = "Madonna",
            LastName = "",
            Email = "madonna@example.com",
            Age = 60
        };

        // Act
        var result = _modelToPersonMapper.Map(personModel);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Madonna");
        result.LastName.Should().Be("");
    }

    [Test]
    public void Map_ToExistingDestination_UpdatesProperties()
    {
        // Arrange
        var department = new Department { Id = 2, Name = "Marketing" };
        var person = new PersonEntity
        {
            Id = 5,
            FirstName = "Alice",
            LastName = "Williams",
            Email = "alice.williams@example.com",
            BirthDate = new DateTime(1992, 3, 8),
            Department = department,
            Addresses = new List<Address>
            {
                new() { Id = 1, Street = "456 Oak Ave", City = "Portland", State = "OR", ZipCode = "97201" }
            }
        };

        var existingModel = new PersonModel
        {
            Id = 999,
            FirstName = "Old",
            LastName = "Name",
            FullName = "Old Name",
            Email = "old@example.com",
            Age = 99,
            DepartmentName = "Old Department",
            AddressCount = 99
        };

        // Act
        _personToModelMapper.Map(person, existingModel);

        // Assert
        existingModel.Id.Should().Be(5);
        existingModel.FirstName.Should().Be("Alice");
        existingModel.LastName.Should().Be("Williams");
        existingModel.FullName.Should().Be("Alice Williams");
        existingModel.Email.Should().Be("alice.williams@example.com");
        existingModel.DepartmentName.Should().Be("Marketing");
        existingModel.AddressCount.Should().Be(1);
    }

    [Test]
    public void Map_ToExistingDestinationWithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        var existingModel = new PersonModel
        {
            Id = 1,
            FirstName = "Test",
            Email = "test@test.com",
            Age = 25
        };

        // Act & Assert
        var action = () => _personToModelMapper.Map(null!, existingModel);
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Map_ToNullDestination_ThrowsArgumentNullException()
    {
        // Arrange
        var person = new PersonEntity
        {
            Id = 1,
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            BirthDate = DateTime.Now.AddYears(-25)
        };

        // Act & Assert
        var action = () => _personToModelMapper.Map(person, null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Map_PersonWithNullDepartmentUsingErrorMapper_ThrowsNullReferenceException()
    {
        // Arrange
        var person = new PersonEntity
        {
            Id = 6,
            FirstName = "Error",
            LastName = "Test",
            Email = "error@test.com",
            BirthDate = new DateTime(1990, 1, 1),
            Department = null, // This will cause NullReferenceException
            Addresses = new List<Address>()
        };

        // Act & Assert
        var action = () => _errorMapper.Map(person);
        action.Should().Throw<NullReferenceException>();
    }

    [Test]
    public void Map_PersonToExistingRecordUsingRecordMapper_ThrowsException()
    {
        // Arrange
        var person = new PersonEntity
        {
            Id = 8,
            FirstName = "Record",
            LastName = "Test",
            Email = "record@test.com",
            BirthDate = new DateTime(1990, 1, 1),
            Department = new Department { Id = 1, Name = "TestDept" },
            Addresses = new List<Address>
            {
                new() { Id = 1, Street = "123 Test St", City = "TestCity", State = "TS", ZipCode = "12345" }
            }
        };

        var existingRecord = new PersonRecord(
            999,
            "Old",
            "Record",
            "old@record.com",
            "Old Record",
            99,
            "Old Department",
            99
        );

        // Act & Assert
        // Records are immutable, so trying to map to an existing record instance should fail
        // The mapper will try to assign to read-only properties which will throw an exception
        var action = () => _recordMapper.Map(person, existingRecord);
        action.Should().Throw<InvalidOperationException>("because record properties are read-only and cannot be modified");
    }

    [Test]
    public void ProjectTo_QueryableOfPersons_ReturnsQueryableOfPersonModels()
    {
        // Arrange
        var persons = new List<PersonEntity>
        {
            new()
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                BirthDate = new DateTime(1990, 1, 1),
                Department = new Department { Name = "Engineering" },
                Addresses = new List<Address>
                {
                    new() { City = "Springfield" }
                }
            },
            new()
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                BirthDate = new DateTime(1985, 6, 15),
                Department = null,
                Addresses = new List<Address>()
            }
        }.AsQueryable();

        // Act
        var result = _personToModelMapper.ProjectTo(persons);

        // Assert
        result.Should().NotBeNull();
        var materialized = result.ToList();
        materialized.Should().HaveCount(2);

        materialized[0].FullName.Should().Be("John Doe");
        materialized[0].DepartmentName.Should().Be("Engineering");
        materialized[0].AddressCount.Should().Be(1);

        materialized[1].FullName.Should().Be("Jane Smith");
        materialized[1].DepartmentName.Should().BeNull();
        materialized[1].AddressCount.Should().Be(0);
    }

    [Test]
    public void ProjectTo_QueryableOfPersons_ReturnsQueryableOfPersonRecords()
    {
        // Arrange
        var persons = new List<PersonEntity>
        {
            new()
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                BirthDate = new DateTime(1990, 5, 15),
                Department = new Department { Name = "Engineering" },
                Addresses = new List<Address>
                {
                    new() { Id = 1, Street = "123 Main St", City = "Springfield", State = "IL", ZipCode = "62701" },
                    new() { Id = 2, Street = "456 Oak Ave", City = "Springfield", State = "IL", ZipCode = "62702" }
                }
            },
            new()
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                BirthDate = new DateTime(1985, 10, 30),
                Department = new Department { Name = "Marketing" },
                Addresses = new List<Address>
                {
                    new() { Id = 3, Street = "789 Pine St", City = "Springfield", State = "IL", ZipCode = "62703" }
                }
            }
        }.AsQueryable();

        // Act
        var result = _personToModelMapper.ProjectTo(persons);

        // Assert
        result.Should().NotBeNull();
        var materialized = result.ToList();
        materialized.Should().HaveCount(2);

        materialized[0].FullName.Should().Be("John Doe");
        materialized[0].DepartmentName.Should().Be("Engineering");
        materialized[0].AddressCount.Should().Be(2);

        materialized[1].FullName.Should().Be("Jane Smith");
        materialized[1].DepartmentName.Should().Be("Marketing");
        materialized[1].AddressCount.Should().Be(1);
    }

    [Test]
    public void ProjectTo_EmptyQueryable_ReturnsEmptyQueryable()
    {
        // Arrange
        var emptyQueryable = Enumerable.Empty<PersonEntity>().AsQueryable();

        // Act
        var result = _personToModelMapper.ProjectTo(emptyQueryable);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public void Mapper_ConsistencyTest_SameInputProducesSameOutput()
    {
        // Arrange
        var person = new PersonEntity
        {
            Id = 100,
            FirstName = "Consistent",
            LastName = "Test",
            Email = "consistent@test.com",
            BirthDate = new DateTime(1990, 1, 1),
            Department = new Department { Name = "TestDept" },
            Addresses = new List<Address>
            {
                new() { Street = "123 Test St", City = "TestCity", State = "TS", ZipCode = "12345" }
            }
        };

        // Act
        var result1 = _personToModelMapper.Map(person);
        var result2 = _personToModelMapper.Map(person);

        // Assert
        result1.Should().BeEquivalentTo(result2);
    }

    [Test]
    public void Mapper_EdgeCases_HandlesEmptyStringsAndCollections()
    {
        // Arrange
        var person = new PersonEntity
        {
            Id = 0,
            FirstName = "",
            LastName = "",
            Email = "",
            BirthDate = DateTime.MinValue,
            Department = new Department { Name = "" },
            Addresses = new List<Address>()
        };

        // Act
        var result = _personToModelMapper.Map(person);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(0);
        result.FullName.Should().Be(" ");
        result.Email.Should().Be("");
        result.DepartmentName.Should().Be("");
        result.AddressCount.Should().Be(0);
    }
}

internal sealed class PersonToPersonModelMapper : MapperBase<PersonEntity, PersonModel>
{
    protected override Expression<Func<PersonEntity, PersonModel>> CreateMapping()
    {
        return person => new PersonModel
        {
            Id = person.Id,
            FirstName = person.FirstName,
            LastName = person.LastName,
            FullName = person.FirstName + " " + person.LastName,
            Email = person.Email,
            Age = DateTime.Now.Year - person.BirthDate.Year,
            DepartmentName = person.Department != null ? person.Department.Name : null,
            AddressCount = person.Addresses != null ? person.Addresses.Count() : 0
        };
    }
}

internal sealed class PersonModelToPersonMapper : MapperBase<PersonModel, PersonEntity>
{
    protected override Expression<Func<PersonModel, PersonEntity>> CreateMapping()
    {
        return model => new PersonEntity
        {
            Id = model.Id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            BirthDate = DateTime.Now.AddYears(-model.Age)
        };
    }
}

internal sealed class PersonToPersonErrorMapper : MapperBase<PersonEntity, PersonModel>
{
    protected override Expression<Func<PersonEntity, PersonModel>> CreateMapping()
    {
        return person => new PersonModel
        {
            Id = person.Id,
            FirstName = person.FirstName,
            LastName = person.LastName,
            FullName = person.FirstName + " " + person.LastName,
            Email = person.Email,
            Age = DateTime.Now.Year - person.BirthDate.Year,
            DepartmentName = person.Department!.Name,
            AddressCount = person.Addresses.Count()
        };
    }
}

internal sealed class PersonToPersonRecordMapper : MapperBase<PersonEntity, PersonRecord>
{
    protected override Expression<Func<PersonEntity, PersonRecord>> CreateMapping()
    {
        return person => new PersonRecord
        (
            person.Id,
            person.FirstName,
            person.LastName,
            person.Email,
            person.FirstName + " " + person.LastName,
            DateTime.Now.Year - person.BirthDate.Year,
            person.Department != null ? person.Department.Name : null,
            person.Addresses.Count()
        );
    }
}
