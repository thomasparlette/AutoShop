namespace AutoShop.Tests.Integration.Services;

public class PurchaseOrderServiceTests
{
    [Fact]
    public void CreateDraftFromLowStock_CreatesPurchaseOrder()
    {
        using var scope = new TestDatabaseScope();

        var partService = new PartService();
        partService.SavePart(new Part
        {
            PartNumber = "LOW-001",
            Description = "Low Stock Part",
            Cost = 2m,
            SellPrice = 3m,
            QuantityOnHand = 1,
            ReorderLevel = 5,
            Supplier = "Vendor A",
            Active = true
        });

        var service = new PurchaseOrderService();
        var po = service.CreateDraftFromLowStock("Vendor A");

        po.Should().NotBeNull();
        po!.LineItems.Should().NotBeEmpty();
        po.Supplier.Should().Be("Vendor A");
    }
}