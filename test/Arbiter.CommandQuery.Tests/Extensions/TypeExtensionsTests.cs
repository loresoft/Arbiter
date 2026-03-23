using Arbiter.CommandQuery.Extensions;

namespace Arbiter.CommandQuery.Tests.Extensions;

public class TypeExtensionsTests
{
    [Test]
    public void GetPortableName_ShouldThrowArgumentNullException_WhenTypeIsNull()
    {
        // Arrange
        Type? type = null;

        // Act
        var act = () => type!.GetPortableName();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void GetPortableName_ShouldReturnPortableName_ForSimpleType()
    {
        // Arrange
        var type = typeof(string);

        // Act
        var result = type.GetPortableName();

        // Assert
        result.Should().Be("System.String, System.Private.CoreLib");

        var resolvedType = Type.GetType(result);
        resolvedType.Should().Be(type);
    }

    [Test]
    public void GetPortableName_ShouldReturnPortableName_ForIntType()
    {
        // Arrange
        var type = typeof(int);

        // Act
        var result = type.GetPortableName();

        // Assert
        result.Should().Be("System.Int32, System.Private.CoreLib");

        var resolvedType = Type.GetType(result);
        resolvedType.Should().Be(type);
    }

    [Test]
    public void GetPortableName_ShouldReturnPortableName_ForCustomType()
    {
        // Arrange
        var type = typeof(TypeExtensionsTests);

        // Act
        var result = type.GetPortableName();

        // Assert
        result.Should().Be("Arbiter.CommandQuery.Tests.Extensions.TypeExtensionsTests, Arbiter.CommandQuery.Tests");

        var resolvedType = Type.GetType(result);
        resolvedType.Should().Be(type);
    }

    [Test]
    public void GetPortableName_ShouldReturnPortableName_ForGenericTypeWithOneArgument()
    {
        // Arrange
        var type = typeof(List<string>);

        // Act
        var result = type.GetPortableName();

        // Assert
        result.Should().Be("System.Collections.Generic.List`1[[System.String, System.Private.CoreLib]], System.Private.CoreLib");

        var resolvedType = Type.GetType(result);
        resolvedType.Should().Be(type);
    }

    [Test]
    public void GetPortableName_ShouldReturnPortableName_ForGenericTypeWithTwoArguments()
    {
        // Arrange
        var type = typeof(Dictionary<string, int>);

        // Act
        var result = type.GetPortableName();

        // Assert
        result.Should().Be("System.Collections.Generic.Dictionary`2[[System.String, System.Private.CoreLib],[System.Int32, System.Private.CoreLib]], System.Private.CoreLib");

        var resolvedType = Type.GetType(result);
        resolvedType.Should().Be(type);
    }

    [Test]
    public void GetPortableName_ShouldReturnPortableName_ForNestedGenericTypes()
    {
        // Arrange
        var type = typeof(List<List<string>>);

        // Act
        var result = type.GetPortableName();

        // Assert
        result.Should().Be("System.Collections.Generic.List`1[[System.Collections.Generic.List`1[[System.String, System.Private.CoreLib]], System.Private.CoreLib]], System.Private.CoreLib");

        var resolvedType = Type.GetType(result);
        resolvedType.Should().Be(type);
    }

    [Test]
    public void GetPortableName_ShouldReturnPortableName_ForComplexNestedGenerics()
    {
        // Arrange
        var type = typeof(Dictionary<string, List<int>>);

        // Act
        var result = type.GetPortableName();

        // Assert
        result.Should().Be("System.Collections.Generic.Dictionary`2[[System.String, System.Private.CoreLib],[System.Collections.Generic.List`1[[System.Int32, System.Private.CoreLib]], System.Private.CoreLib]], System.Private.CoreLib");

        var resolvedType = Type.GetType(result);
        resolvedType.Should().Be(type);
    }

    [Test]
    public void GetPortableName_ShouldReturnPortableName_ForTuple()
    {
        // Arrange
        var type = typeof(Tuple<string, int>);

        // Act
        var result = type.GetPortableName();

        // Assert
        result.Should().Be("System.Tuple`2[[System.String, System.Private.CoreLib],[System.Int32, System.Private.CoreLib]], System.Private.CoreLib");

        var resolvedType = Type.GetType(result);
        resolvedType.Should().Be(type);
    }

    [Test]
    public void GetPortableName_ShouldReturnPortableName_ForValueTuple()
    {
        // Arrange
        var type = typeof(ValueTuple<string, int>);

        // Act
        var result = type.GetPortableName();

        // Assert
        result.Should().Be("System.ValueTuple`2[[System.String, System.Private.CoreLib],[System.Int32, System.Private.CoreLib]], System.Private.CoreLib");

        var resolvedType = Type.GetType(result);
        resolvedType.Should().Be(type);
    }

    [Test]
    public void GetPortableName_ShouldNotIncludeVersionOrCulture()
    {
        // Arrange
        var type = typeof(string);

        // Act
        var result = type.GetPortableName();

        // Assert
        result.Should().NotContain("Version=");
        result.Should().NotContain("Culture=");
        result.Should().NotContain("PublicKeyToken=");
    }

    [Test]
    public void GetPortableName_ShouldReturnPortableName_ForNullable()
    {
        // Arrange
        var type = typeof(int?);

        // Act
        var result = type.GetPortableName();

        // Assert
        result.Should().Be("System.Nullable`1[[System.Int32, System.Private.CoreLib]], System.Private.CoreLib");

        var resolvedType = Type.GetType(result);
        resolvedType.Should().Be(type);
    }

    [Test]
    public void GetPortableName_ShouldReturnPortableName_ForArray()
    {
        // Arrange
        var type = typeof(string[]);

        // Act
        var result = type.GetPortableName();

        // Assert
        result.Should().Be("System.String[], System.Private.CoreLib");

        var resolvedType = Type.GetType(result);
        resolvedType.Should().Be(type);
    }
}
