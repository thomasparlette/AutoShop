namespace AutoShop.Tests.Integration.Services;

public class PartServiceTests
{
    [Fact]
    public void SavePart_ThenGetPartByNumber_PersistsPart()
    {
        using var scope = new TestDatabaseScope();

        var service = new PartService();

        var part = service.SavePart(TestDataBuilder.CreatePart());

        part.Id.Should().BeGreaterThan(0);

        var loaded = service.GetPartByNumber("OIL-FLTR-001");
        loaded.Should().NotBeNull();
        loaded!.QuantityOnHand.Should().Be(5);
    }

    [Fact]
    public void DeletePart_WhenReferenced_DeactivatesInsteadOfDeleting()
    {
        using var scope = new TestDatabaseScope();

        var partService = new PartService();
        var inventory = new InventoryService();

        var part = partService.SavePart(TestDataBuilder.CreatePart("REF-001"));

        inventory.AdjustQuantity(part.Id, -1, "Work Order", "WO-1", "test");
        partService.DeletePart(part.Id);

        var loaded = partService.GetPartById(part.Id);
        loaded.Should().NotBeNull();
        loaded!.Active.Should().BeFalse();
    }
}