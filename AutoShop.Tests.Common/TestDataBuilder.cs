using AutoShop.Core.Entities;

namespace AutoShop.Tests.Common;

public static class TestDataBuilder
{
    public static Customer CreateCustomer(string firstName = "Tom", string lastName = "Parlette")
    {
        return new Customer
        {
            FirstName = firstName,
            LastName = lastName,
            Phone = "555-555-1212",
            Email = "tom@example.com",
            AddressLine1 = "123 Main St",
            City = "Denver",
            State = "CO",
            PostalCode = "80202"
        };
    }

    public static Vehicle CreateVehicle(int customerId = 1, string vin = "VIN-TEST-001")
    {
        return new Vehicle
        {
            CustomerId = customerId,
            Year = 2022,
            Make = "Honda",
            Model = "Civic",
            Vin = vin,
            Mileage = 10000
        };
    }

    public static Part CreatePart(string partNumber = "OIL-FLTR-001")
    {
        return new Part
        {
            PartNumber = partNumber,
            Description = "Oil Filter",
            Cost = 10m,
            SellPrice = 13.50m,
            QuantityOnHand = 5,
            ReorderLevel = 2,
            Supplier = "ACME",
            Active = true
        };
    }
}