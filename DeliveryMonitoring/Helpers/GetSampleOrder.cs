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
                Id = "1bf2e4c9-1854-46dc-a178-4206d689a2c9",

                AssignedDriverPhoneNumber = "0990002764",
                Status = "accepted",
                VoucherCode = "0915575075-12222-336",
                ExceptDrivers = string.Empty,

                CompanyCode = 50301,
                CompanyName = "Amrogn Chicken",
                BranchName = "Sarbet Branch +251903464646 Located in Mela Building in the Sarbet area ",
                CompanyTin = "0056406223",
                DeliveryTin = "0076217301",

                TargetBranchLat = 8.98621276650009,
                TargetBranchLng = 38.7374223883795,
                TargetBranchLocation = new Location
                {
                    lat = 8.98621276650009,
                    lng = 38.7374223883795
                },

                Eta = DateTime.Parse("2026-01-13T09:54:02.651Z"),
                DeliveryDateTime = null,

                SupervisedBy = "0968948553",
                SosReason = null,

                GrandTotal = 1540.09m,

                CustomerDeviceID = string.Empty,
                CustomerFirstName = "Rediet",
                CustomerPhoneNumber = "0915575075",
                CustomerGeocodeAddress = "kadisco traffic lights ",
                CustomerSpecificAddress = string.Empty,
                CustomerLat = 8.96228417891022,
                CustomerLng = 38.7673442065716,

                Customer = new CustomerDetail
                {
                    FirstName = "Rediet",
                    DeviceID = string.Empty,
                    PhoneNumber = "0915575075",
                    SpecificAddress = string.Empty,
                    LatLng = new Location
                    {
                        lat = 8.96228417891022,
                        lng = 38.7673442065716
                    }
                },

                DriverAssignedTime = null,
                IsAssignedAck = false,
                IsNoDriversAck = false,
                OrderArrivedAckByCustomer = false,
                OrderArrivedAckByDriver = false,

                Platform = "IOS",

                UpdatedAt = DateTime.Parse("2026-01-13T09:03:33.076Z"),
                CreatedAt = DateTime.Parse("2026-01-13T09:03:25.330Z"),

                PreparationTime = 25,
                StatusTrackerId = "59485",

                CustomerSpecialRequest = null,
                StatusReport = null,

                OrderPrinted = true,
                PhotoAttachment = null,
                PaymentMethod = "Telebirr USSD Push",

                LineItemsDetail = new LineItemsDetail
                {
                    LineItems = new List<LineItem>
                    {
                        new ()
                        {
                            Article = 27,
                            Name = "Chicken Moffo 1/2",
                            UnitAmount = 1086.96m,
                            Quantity = 1,
                            TaxableAmount = 1086.96m,
                            Note = "",
                            LineItemId = 10213297
                        },
                        new ()
                        {
                            Article = 50,
                            Name = "cup",
                            UnitAmount = 8.69m,
                            Quantity = 1,
                            TaxableAmount = 8.69m,
                            Note = "",
                            LineItemId = 10213299
                        },
                        new ()
                        {
                            Article = 100,
                            Name = "MANGO",
                            UnitAmount = 199.99m,
                            Quantity = 1,
                            TaxableAmount = 199.99m,
                            Note = "",
                            LineItemId = 10213296
                        },
                        new ()
                        {
                            Article = 185,
                            Name = "box grill",
                            UnitAmount = 43.47m,
                            Quantity = 1,
                            TaxableAmount = 43.47m,
                            Note = "",
                            LineItemId = 10213300
                        },
                        new ()
                        {
                            Article = 663,
                            Name = "HuluBeje Driver",
                            UnitAmount = 0.1m,
                            Quantity = 1,
                            TaxableAmount = 0.1m,
                            Note = null,
                            LineItemId = 10213298
                        }
                    },

                    ExtraCharge = new Dictionary<string, decimal>
                    {
                        { "TXBL 1", 1339.21m },
                        { "TAX1 15%", 200.883m }
                    },

                    GrandTotal = 1540.09m,

                    ExtraInformation = new Dictionary<string, string>(),

                    ExtraData = new Dictionary<string, string>
                    {
                        { "voucherId", "4780347" },
                        { "tin", "0056406223" }
                    },

                    IssuedDate = DateTime.Parse("2026-01-13T12:02:33.203"),
                    BranchCode = 44502,
                    PromoDetail = null,
                    PhoneNumber = "0915575075",
                    CompanyName = "Amrogn Chicken",
                    VoucherCode = "0915575075-12222-336"
                },

                Activities = new Activities
                {
                    StartTime = DateTime.Parse("2026-01-13T12:02:24.13"),
                    SupervisorName = "Heni",
                    CurrentTime = DateTime.Parse("2026-01-13T12:10:19.5408368+03:00"),
                    Eta = DateTime.Parse("2026-01-13T12:54:02"),
                    Alert = null,
                    ActualArrival = null,

                    ActivityResponse = new List<ActivityResponse>
                    {
                        new ()
                        {
                            Name = "Order Placed",
                            Time = DateTime.Parse("2026-01-13T12:02:24.13"),
                            TimeElapsed = "0 Seconds",
                            Longitude = 8.962293458,
                            Latitude = 8.962293458
                        },
                        new ()
                        {
                            Name = "Order received and is being prepared",
                            Time = DateTime.Parse("2026-01-13T12:02:54.32"),
                            TimeElapsed = "30 Seconds",
                            Longitude = null,
                            Latitude = null
                        },
                        new ()
                        {
                            Name = "Order Delivery accepted by the Driver",
                            Time = DateTime.Parse("2026-01-13T12:03:26.713"),
                            TimeElapsed = "1.04 Minutes",
                            Longitude = 38.7574853,
                            Latitude = 9.0021729
                        },
                        new ()
                        {
                            Name = "Order Delivery accepted by the Supervisor",
                            Time = DateTime.Parse("2026-01-13T12:04:32.11"),
                            TimeElapsed = "2.13 Minutes",
                            Longitude = 0,
                            Latitude = 0
                        }
                    }
                }
            };
        }
    }
}
