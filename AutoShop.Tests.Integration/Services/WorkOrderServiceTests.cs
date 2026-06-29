namespace AutoShop.Tests.Integration.Services;

public class WorkOrderServiceTests
{
    [Fact]
    public void SaveWorkOrder_PersistsWorkOrder()
    {
        using var scope = new TestDatabaseScope();

        var customerService = new CustomerService();
        var vehicleService = new VehicleService();
        var workOrderService = new WorkOrderService();

        var customer = customerService.SaveCustomer(TestDataBuilder.CreateCustomer());
        var vehicle = vehicleService.SaveVehicle(TestDataBuilder.CreateVehicle(customer.Id));

        var workOrder = workOrderService.SaveWorkOrder(new WorkOrder
        {
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            Status = WorkOrderStatus.Open,
            CreatedAt = DateTime.Now,
            WorkOrderNumber = "WO-TEST-001",
            Complaint = "Noise"
        });

        workOrder.Id.Should().BeGreaterThan(0);

        var loaded = workOrderService.GetWorkOrderById(workOrder.Id);
        loaded.Should().NotBeNull();
        loaded!.Complaint.Should().Be("Noise");
    }
}