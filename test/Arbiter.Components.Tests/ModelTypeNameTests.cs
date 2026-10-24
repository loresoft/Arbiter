namespace Arbiter.Components.Tests;

public class ModelTypeNameTests
{
    [Test]
    [Arguments(typeof(PurchaseOrderReadModel), "PurchaseOrder")]
    [Arguments(typeof(PurchaseOrderUpdateModel), "PurchaseOrder")]
    [Arguments(typeof(TenantHolder), "TenantHolder")]
    public async Task GetNameRemovesModelSuffix(Type modelType, string expected)
    {
        var name = ModelTypeName.GetName(modelType);

        await Assert.That(name).IsEqualTo(expected);
    }

    [Test]
    public async Task GetLabelSplitsNameIntoWords()
    {
        var label = ModelTypeName.GetLabel(typeof(PurchaseOrderUpdateModel));

        await Assert.That(label).IsEqualTo("Purchase Order");
    }

    [Test]
    public async Task GetNameReturnsNullWhenTypeIsNull()
    {
        var name = ModelTypeName.GetName(null);

        await Assert.That(name).IsNull();
    }

    [Test]
    public async Task GetLabelReturnsNullWhenTypeIsNull()
    {
        var label = ModelTypeName.GetLabel(null);

        await Assert.That(label).IsNull();
    }

    [Test]
    public async Task GetNameUsesUnderlyingTypeOfNullable()
    {
        var name = ModelTypeName.GetName(typeof(PurchaseOrderListModel?));

        await Assert.That(name).IsEqualTo("PurchaseOrder");
    }

    [Test]
    public async Task GetLabelUsesUnderlyingTypeOfNullable()
    {
        var label = ModelTypeName.GetLabel(typeof(PurchaseOrderListModel?));

        await Assert.That(label).IsEqualTo("Purchase Order");
    }

    [Test]
    public async Task GenericGetNameMatchesTypeOverload()
    {
        var name = ModelTypeName.GetName<PurchaseOrderReadModel>();
        var nullableName = ModelTypeName.GetName<PurchaseOrderListModel?>();

        await Assert.That(name).IsEqualTo("PurchaseOrder");
        await Assert.That(nullableName).IsEqualTo("PurchaseOrder");
    }

    [Test]
    public async Task GenericGetLabelMatchesTypeOverload()
    {
        var label = ModelTypeName.GetLabel<PurchaseOrderUpdateModel>();

        await Assert.That(label).IsEqualTo("Purchase Order");
    }

    public sealed class PurchaseOrderReadModel;

    public sealed class PurchaseOrderUpdateModel;

    public struct PurchaseOrderListModel;
}
