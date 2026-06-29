namespace AutoShop.Tests.Unit.Domain;

public class EntityComputedPropertyTests
{
    [Fact]
    public void WorkOrderLineItem_LineTotal_ComputesCorrectly()
    {
        var item = new WorkOrderLineItem
        {
            Quantity = 3m,
            UnitPrice = 12.50m
        };

        item.LineTotal.Should().Be(37.50m);
    }

    [Fact]
    public void PurchaseOrderTotals_ComputesCorrectly()
    {
        var po = new PurchaseOrder
        {
            LineItems =
            {
                new PurchaseOrderLineItem { QuantityOrdered = 2, QuantityReceived = 1, UnitCost = 10m },
                new PurchaseOrderLineItem { QuantityOrdered = 3, QuantityReceived = 3, UnitCost = 5m }
            }
        };

        po.TotalOrderedQuantity.Should().Be(5);
        po.TotalReceivedQuantity.Should().Be(4);
        po.OrderedTotal.Should().Be(35m);
        po.ReceivedTotal.Should().Be(25m);
    }
}