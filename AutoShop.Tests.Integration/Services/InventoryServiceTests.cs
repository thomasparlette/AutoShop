namespace AutoShop.Tests.Integration.Services;

public class InventoryServiceTests
{
    [Fact]
    public void AdjustQuantity_WritesInventoryTransaction_AndUpdatesPart()
    {
        using var scope = new TestDatabaseScope();

        var parts = new PartService();
        var inventory = new InventoryService();

        var part = parts.SavePart(TestDataBuilder.CreatePart("INV-001"));

        inventory.AdjustQuantity(part.Id, -2, "Work Order", "WO-1001", "Completion test");

        var reloaded = parts.GetPartById(part.Id);
        reloaded!.QuantityOnHand.Should().Be(3);

        var txs = inventory.GetTransactionsForPart(part.Id);
        txs.Should().ContainSingle();
        txs[0].QuantityBefore.Should().Be(5);
        txs[0].QuantityAfter.Should().Be(3);
        txs[0].QuantityChange.Should().Be(-2);
    }
}