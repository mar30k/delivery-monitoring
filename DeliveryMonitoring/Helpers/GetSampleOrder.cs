using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Models;

namespace DeliveryMonitoring.Helpers
{
    public class GetSampleOrder
    {
        public static OrderDetail CreateSampleOrder()
        {
            return new OrderDetail
            {
                Id = "ORD123456",
                AssignedDriverPhoneNumber = "0966767628",
                BranchName = "Addis Branch",
                CompanyCode = 1001,
                CompanyName = "Tech Logistics",
                CompanyTin = "0039441045",
                DeliveryTin = AppConstants.Company.AdminTin,
                SupervisedBy = "SUP001",
                SupervisorName = "Mr. Dawit",
                SosReason = "Delayed",
                GrandTotal = 1500.00m,
                Platform = "Web",
                RequestCreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                CreatedAtString = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                RequestCreatedAtIso = DateTime.UtcNow,
                DriverAssignedTime = DateTime.UtcNow.AddMinutes(-10),
                DeliveryDateTime = DateTime.UtcNow.AddHours(1),
                CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                UpdatedAt = DateTime.UtcNow,
                Eta = DateTime.UtcNow.AddHours(2),
                Status = "In Transit",
                TargetBranchLocation = new Location
                {
                    lat = 8.9806,
                    lng = 38.7578
                },
                TargetBranchLat = 8.9806,
                TargetBranchLng = 38.7578,
                VoucherCode = "0939977886-132754-255",
                Alert = string.Empty,
                ExceptDrivers = "DRV002,DRV003",

                Customer = new CustomerDetail
                {
                    DeviceID = "DEV123",
                    FirstName = "Abebe",
                    GeocodeAddress = "Bole Medhanialem, Addis Ababa",
                    PhoneNumber = "0911122233",
                    SpecificAddress = "Behind XYZ Building",
                    LatLng = new Location
                    {
                        lat = 8.998812,
                        lng = 38.785802
                    }
                },

                CustomerDeviceID = "DEV123",
                CustomerFirstName = "Abebe",
                CustomerGeocodeAddress = "Bole Medhanialem, Addis Ababa",
                CustomerLat = 8.998812,
                CustomerLng = 38.785802,
                CustomerPhoneNumber = "0911122233",
                CustomerSpecificAddress = "Behind XYZ Building",
                DriverAssignedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                IsAssignedAck = true,
                IsNoDriversAck = false,
                OrderArrivedAckByCustomer = false,
                OrderArrivedAckByDriver = true,
                StatusReport = "መንገድ ተዘጋግቷል",
                PreparationTime = 20,
                CustomerSpecialRequest = "Special Request",
                LineItemsDetail = new LineItemsDetail
                {
                    LineItems = new List<LineItem>
                    {
                        new ()
                        {
                            Article = 101,
                            Name = "Laptop",
                            UnitAmount = 1200.00m,
                            Quantity = 1,
                            TaxableAmount = 1200.00m
                        },
                        new ()
                        {
                            Article = 202,
                            Name = "Mouse",
                            UnitAmount = 100.00m,
                            Quantity = 2,
                            TaxableAmount = 200.00m
                        }
                    },
                    ExtraCharge = new Dictionary<string, decimal>
                    {
                        { "VAT", 150.00m },
                        { "Delivery", 50.00m }
                    },
                    GrandTotal = 1500.00m,
                    ExtraInformation = new Dictionary<string, string>
                    {
                        { "DeliveredBy", "Drone" },
                        { "Packaging", "Eco-friendly" }
                    },
                    ExtraData = new Dictionary<string, string>
                    {
                        { "VoucherId", "555" },
                        { "Tin", "1234567890" }
                    },
                    IssuedDate = DateTime.UtcNow,
                    BranchCode = 10,
                    PromoDetail = "10% Discount",
                    PhoneNumber = "0911122233",
                    CompanyName = "Tech Logistics",
                    VoucherCode = "PROMO2025"
                },

                Activities = new Activities
                {
                    StartTime = DateTime.UtcNow.AddHours(-1),
                    CurrentTime = DateTime.UtcNow,
                    Eta = DateTime.UtcNow.AddHours(1),
                    ActualArrival = null,
                    Alert = "Driver Delayed",
                    ActivityResponse = new List<ActivityResponse>
                    {
                        new ()
                        {
                            Name = "Picked Up",
                            Time = DateTime.UtcNow.AddMinutes(-30),
                            TimeElapsed = "30 minutes ago"
                        },
                        new ()
                        {
                            Name = "En Route",
                            Time = DateTime.UtcNow.AddMinutes(-10),
                            TimeElapsed = "10 minutes ago"
                        }
                    }
                },

                OrderAcceptedNotification = DateTime.UtcNow.AddMinutes(-20),
                OrderReceiveNotification = DateTime.UtcNow.AddMinutes(-5)
            };
        }
    }
}
