using Arbiter.CommandQuery.Tests.Models;

using PersonEntity = Arbiter.CommandQuery.Tests.Models.Person;

namespace Arbiter.Mapping.Tests;

public class MapperGeneratedTests
{
    private readonly GeneratedPersonToModelMapper _personToModelMapper = new();
    private readonly GeneratedModelToPersonMapper _modelToPersonMapper = new();
    private readonly GeneratedRecordToModelMapper _recordToModelMapper = new();
    private readonly GeneratedPersonToRecordMapper _personToRecordMapper = new();
    private readonly GeneratedPersonConstructorRecordMapper _personToConstructorRecordMapper = new();
    private readonly GeneratedPersonToConstructorModelMapper _personToConstructorModelMapper = new();

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
    public void ProjectTo_NullSource_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => _personToModelMapper.ProjectTo(null!);
        action.Should().Throw<ArgumentNullException>();
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

    [Test]
    public void Map_ValidRecord_ReturnsCorrectPersonModel()
    {
        // Arrange
        var record = new PersonRecord
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            FullName = "John Doe",
            Age = 35,
            DepartmentName = "Engineering",
            AddressCount = 2
        };

        // Act
        var result = _recordToModelMapper.Map(record);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Email.Should().Be("john.doe@example.com");
        result.FullName.Should().Be("John Doe");
        result.Age.Should().Be(35);
        result.DepartmentName.Should().Be("Engineering");
        result.AddressCount.Should().Be(2);
    }

    [Test]
    public void Map_NullRecord_ReturnsNull()
    {
        // Act
        var result = _recordToModelMapper.Map(null);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void Map_RecordWithNullDepartmentName_MapsNull()
    {
        // Arrange
        var record = new PersonRecord
        {
            Id = 2,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            FullName = "Jane Smith",
            Age = 40,
            DepartmentName = null,
            AddressCount = 0
        };

        // Act
        var result = _recordToModelMapper.Map(record);

        // Assert
        result.Should().NotBeNull();
        result.DepartmentName.Should().BeNull();
        result.AddressCount.Should().Be(0);
    }

    [Test]
    public void Map_RecordToExistingDestination_UpdatesProperties()
    {
        // Arrange
        var record = new PersonRecord
        {
            Id = 5,
            FirstName = "Alice",
            LastName = "Williams",
            Email = "alice@example.com",
            FullName = "Alice Williams",
            Age = 30,
            DepartmentName = "Marketing",
            AddressCount = 1
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
        _recordToModelMapper.Map(record, existingModel);

        // Assert
        existingModel.Id.Should().Be(5);
        existingModel.FirstName.Should().Be("Alice");
        existingModel.LastName.Should().Be("Williams");
        existingModel.FullName.Should().Be("Alice Williams");
        existingModel.Email.Should().Be("alice@example.com");
        existingModel.Age.Should().Be(30);
        existingModel.DepartmentName.Should().Be("Marketing");
        existingModel.AddressCount.Should().Be(1);
    }

    [Test]
    public void Map_RecordToExistingWithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        var existingModel = new PersonModel();

        // Act & Assert
        var action = () => _recordToModelMapper.Map(null!, existingModel);
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Map_RecordToNullDestination_ThrowsArgumentNullException()
    {
        // Arrange
        var record = new PersonRecord { Id = 1, FirstName = "Test", LastName = "User", Email = "test@test.com", FullName = "Test User", Age = 25, DepartmentName = null, AddressCount = 0 };

        // Act & Assert
        var action = () => _recordToModelMapper.Map(record, null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void ProjectTo_QueryableOfRecords_ReturnsQueryableOfPersonModels()
    {
        // Arrange
        var records = new List<PersonRecord>
        {
            new() { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", FullName = "John Doe", Age = 35, DepartmentName = "Engineering", AddressCount = 2 },
            new() { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", FullName = "Jane Smith", Age = 40, DepartmentName = null, AddressCount = 0 }
        }.AsQueryable();

        // Act
        var result = _recordToModelMapper.ProjectTo(records);

        // Assert
        result.Should().NotBeNull();
        var materialized = result.ToList();
        materialized.Should().HaveCount(2);

        materialized[0].FullName.Should().Be("John Doe");
        materialized[0].DepartmentName.Should().Be("Engineering");
        materialized[0].AddressCount.Should().Be(2);

        materialized[1].FullName.Should().Be("Jane Smith");
        materialized[1].DepartmentName.Should().BeNull();
        materialized[1].AddressCount.Should().Be(0);
    }

    [Test]
    public void ProjectTo_NullRecordSource_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => _recordToModelMapper.ProjectTo(null!);
        action.Should().Throw<ArgumentNullException>();
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
        var result = _personToRecordMapper.Map(person);

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
    public void Map_NullPerson_ReturnsNullRecord()
    {
        // Act
        var result = _personToRecordMapper.Map(null);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void Map_PersonWithNullDepartment_ReturnsRecordWithNullDepartmentName()
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
        var result = _personToRecordMapper.Map(person);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(2);
        result.FullName.Should().Be("Jane Smith");
        result.DepartmentName.Should().BeNull();
        result.AddressCount.Should().Be(0);
    }

    [Test]
    public void Map_PersonToExistingRecord_ThrowsNotSupportedException()
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

        var existingRecord = new PersonRecord
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

        // Act & Assert — copy mapper not generated for all-init-only records
        var action = () => _personToRecordMapper.Map(person, existingRecord);
        action.Should().Throw<NotSupportedException>();
    }

    [Test]
    public void Map_PersonToRecordWithNullSource_ThrowsNotSupportedException()
    {
        // Arrange
        var existingRecord = new PersonRecord();

        // Act & Assert — copy mapper not generated for all-init-only records
        var action = () => _personToRecordMapper.Map(null!, existingRecord);
        action.Should().Throw<NotSupportedException>();
    }

    [Test]
    public void Map_PersonToNullRecord_ThrowsNotSupportedException()
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

        // Act & Assert — copy mapper not generated for all-init-only records
        var action = () => _personToRecordMapper.Map(person, null!);
        action.Should().Throw<NotSupportedException>();
    }

    [Test]
    public void ProjectTo_QueryableOfPersons_ReturnsQueryableOfRecords()
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
        var result = _personToRecordMapper.ProjectTo(persons);

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
    public void ProjectTo_NullPersonSourceForRecord_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => _personToRecordMapper.ProjectTo(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Map_ValidPerson_ReturnsCorrectPositionalRecord()
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
        var result = _personToConstructorRecordMapper.Map(person);

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
    public void Map_NullPerson_ReturnsNullPositionalRecord()
    {
        // Act
        var result = _personToConstructorRecordMapper.Map(null);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void Map_PersonWithNullDepartment_ReturnsPositionalRecordWithNullDepartmentName()
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
        var result = _personToConstructorRecordMapper.Map(person);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(2);
        result.FullName.Should().Be("Jane Smith");
        result.DepartmentName.Should().BeNull();
        result.AddressCount.Should().Be(0);
    }

    [Test]
    public void Map_PersonToExistingPositionalRecord_ThrowsNotSupportedException()
    {
        // Arrange
        var person = new PersonEntity
        {
            Id = 5,
            FirstName = "Alice",
            LastName = "Williams",
            Email = "alice.williams@example.com",
            BirthDate = new DateTime(1992, 3, 8),
            Department = new Department { Id = 2, Name = "Marketing" },
            Addresses = new List<Address>
            {
                new() { Id = 1, Street = "456 Oak Ave", City = "Portland", State = "OR", ZipCode = "97201" }
            }
        };

        var existingRecord = new PersonConstructorRecord(
            Id: 999,
            FirstName: "Old",
            LastName: "Name",
            Email: "old@example.com",
            FullName: "Old Name",
            Age: 99,
            DepartmentName: "Old Department",
            AddressCount: 99);

        // Act & Assert — copy mapper not generated for all-init-only records
        var action = () => _personToConstructorRecordMapper.Map(person, existingRecord);
        action.Should().Throw<NotSupportedException>();
    }

    [Test]
    public void ProjectTo_QueryableOfPersons_ReturnsQueryableOfPositionalRecords()
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
        var result = _personToConstructorRecordMapper.ProjectTo(persons);

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
    public void ProjectTo_NullPersonSourceForPositionalRecord_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => _personToConstructorRecordMapper.ProjectTo(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Map_ValidPerson_ReturnsCorrectConstructorModel()
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
        var result = _personToConstructorModelMapper.Map(person);

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
    public void Map_NullPerson_ReturnsNullConstructorModel()
    {
        // Act
        var result = _personToConstructorModelMapper.Map(null);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void Map_PersonToExistingConstructorModel_ThrowsNotSupportedException()
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

        var existingModel = new PersonConstructorModel(
            id: 999,
            firstName: "Old",
            lastName: "Name",
            email: "old@example.com",
            fullName: "Old Name",
            age: 99,
            departmentName: "Old Department",
            addressCount: 99);

        // Act & Assert — copy mapper not generated for all-getter-only constructor models
        var action = () => _personToConstructorModelMapper.Map(person, existingModel);
        action.Should().Throw<NotSupportedException>();
    }

    [Test]
    public void ProjectTo_QueryableOfPersons_ReturnsQueryableOfConstructorModels()
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
        var result = _personToConstructorModelMapper.ProjectTo(persons);

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
}

[GenerateMapper]
internal sealed partial class GeneratedPersonToModelMapper : MapperProfile<PersonEntity, PersonModel>
{
    protected override void ConfigureMapping(MappingBuilder<PersonEntity, PersonModel> mapping)
    {
        mapping.Property(d => d.FullName).From(p => p.FirstName + " " + p.LastName);
        mapping.Property(d => d.Age).From(p => DateTime.Now.Year - p.BirthDate.Year);
        mapping.Property(d => d.DepartmentName).From(p => p.Department!.Name);
        mapping.Property(d => d.AddressCount).From(p => p.Addresses.Count());
    }
}

[GenerateMapper]
internal sealed partial class GeneratedModelToPersonMapper : MapperProfile<PersonModel, PersonEntity>
{
    protected override void ConfigureMapping(MappingBuilder<PersonModel, PersonEntity> mapping)
    {
        mapping.Property(d => d.BirthDate).From(p => DateTime.Now.AddYears(-p.Age));
        mapping.Property(d => d.DepartmentId).Ignore();
        mapping.Property(d => d.Department).Ignore();
        mapping.Property(d => d.Addresses).Ignore();
    }
}

[GenerateMapper]
internal sealed partial class GeneratedRecordToModelMapper : MapperProfile<PersonRecord, PersonModel>;

[GenerateMapper]
internal sealed partial class GeneratedPersonToRecordMapper : MapperProfile<PersonEntity, PersonRecord>
{
    protected override void ConfigureMapping(MappingBuilder<PersonEntity, PersonRecord> mapping)
    {
        mapping.Property(d => d.FullName).From(p => p.FirstName + " " + p.LastName);
        mapping.Property(d => d.Age).From(p => DateTime.Now.Year - p.BirthDate.Year);
        mapping.Property(d => d.DepartmentName).From(p => p.Department!.Name);
        mapping.Property(d => d.AddressCount).From(p => p.Addresses.Count());
    }
}

[GenerateMapper]
internal sealed partial class GeneratedPersonConstructorRecordMapper : MapperProfile<PersonEntity, PersonConstructorRecord>
{
    protected override void ConfigureMapping(MappingBuilder<PersonEntity, PersonConstructorRecord> mapping)
    {
        mapping.Property(d => d.FullName).From(p => p.FirstName + " " + p.LastName);
        mapping.Property(d => d.Age).From(p => DateTime.Now.Year - p.BirthDate.Year);
        mapping.Property(d => d.DepartmentName).From(p => p.Department!.Name);
        mapping.Property(d => d.AddressCount).From(p => p.Addresses.Count());
    }
}

[GenerateMapper]
internal sealed partial class GeneratedPersonToConstructorModelMapper : MapperProfile<PersonEntity, PersonConstructorModel>
{
    protected override void ConfigureMapping(MappingBuilder<PersonEntity, PersonConstructorModel> mapping)
    {
        mapping.Property(d => d.FullName).From(p => p.FirstName + " " + p.LastName);
        mapping.Property(d => d.Age).From(p => DateTime.Now.Year - p.BirthDate.Year);
        mapping.Property(d => d.DepartmentName).From(p => p.Department!.Name);
        mapping.Property(d => d.AddressCount).From(p => p.Addresses.Count());
    }
}

