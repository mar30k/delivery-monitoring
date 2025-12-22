using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Models;

namespace DeliveryMonitoring.Helpers
{
    public static class TableConfigFactory
    {
        // --- Summary table configs ---
        public static TableConfig CreateSummary(SummaryType type) =>
            (type) switch
            {
                SummaryType.Merchant => new TableConfig
                {
                    Type = "merchant",
                    Title = "Merchant Summary",
                    TableId = "merchantSummary",
                    AjaxUrl = $"/summary/data?stype={type}",
                    SheetName = "Merchant Summary"
                },
                SummaryType.Driver => new TableConfig
                {
                    Type = "driver",
                    Title = "Driver Summary",
                    TableId = "driverSummary",
                    AjaxUrl = $"/summary/data?stype={type}",
                    SheetName = "Driver Summary"
                },
                SummaryType.Supervisor=> new TableConfig
                {
                    Type = "supervisor",
                    Title = "Supervisor Summary",
                    TableId = "supervisorSummary",
                    AjaxUrl = $"/summary/data?stype={type}",
                    SheetName = "Supervisor Summary"
                },
                SummaryType.Consignee or _ => new TableConfig
                {
                    Type = "consignee",
                    Title = "Consignee Summary",
                    TableId = "consigneeSummary",
                    AjaxUrl = "/summary/data?stype=consignee",
                    SheetName = "Consignee Summary"
                }
            };

        // --- Report table configs ---
        public static TableConfig CreateReport(ReportByOrderType type) =>
            type switch
            {
                ReportByOrderType.Dinein => new TableConfig
                {
                    Type = "Dine-in",
                    Title = "Dine-in Report",
                    TableId = "dinein",
                    AjaxUrl = $"/getordersbytype?type={(int)DeliveryOrderTypes.InHouseDining}",
                    SheetName = "Dine-in Report"
                },
                ReportByOrderType.ScheduledDeliveryToLocation => new TableConfig
                {
                    Type = "ScheduledDeliveryToLocation",
                    Title = "Scheduled Delivery To Location Report",
                    TableId = "sDelivery",
                    AjaxUrl = $"/getordersbytype?type={(int)DeliveryOrderTypes.ScheduledDeliveryToLocation}",
                    SheetName = "Scheduled Delivery Report"
                },
                ReportByOrderType.ScheduledPickUp => new TableConfig
                {
                    Type = "ScheduledPickUp",
                    Title = "Scheduled Takeaway Report",
                    TableId = "sTakeaway",
                    AjaxUrl = $"/getordersbytype?type={(int)DeliveryOrderTypes.ScheduledPickUp}",
                    SheetName = "Scheduled Takeaway Report"
                },
                ReportByOrderType.Takeaway => new TableConfig
                {
                    Type = "Takeaway",
                    Title = "Takeaway Report",
                    TableId = "takeaway",
                    AjaxUrl = $"/getordersbytype?type={(int)DeliveryOrderTypes.PickUpAtBranch}",
                    SheetName = "Takeaway Report"
                },
                ReportByOrderType.All => new TableConfig
                {
                    Type = "AllorderTypes",
                    Title = "All Orders Report",
                    TableId = "allOrders",
                    AjaxUrl = "/getAllOrders",
                    SheetName = "All Orders Report"
                },
                ReportByOrderType.Delivery or _ => new TableConfig
                {
                    Type = "Delivery",
                    Title = "Delivery Report",
                    TableId = "deliveryTable",
                    AjaxUrl = "/getCompletedOrders",
                    SheetName = "Delivery Report"
                }
            };
        // --- Report table configs ---
        public static List<TableConfig> CreateCompletedOrderTables() {
            return new List<TableConfig> { 
                new ()
                {
                    Type = "Delivery",
                    Title = "Delivery",
                    TableId = AppConstants.TableIds.Delivery,
                    AjaxUrl = "/getCompletedOrders",
                    SheetName = "_DeliveryOrders"
                },
                new ()
                {
                    Type = DeliveryOrderTypes.ScheduledDeliveryToLocation.ToString(),
                    Title = "Scheduled Delivery",
                    TableId = AppConstants.TableIds.ScheduledDelivery,
                    AjaxUrl = $"/getordersbytype?type={(int)DeliveryOrderTypes.ScheduledDeliveryToLocation}",
                    SheetName = "_DeliveryOrders"
                },
                new ()
                {
                    Type = "Takeaway",
                    Title = "Takeaway",
                    TableId = AppConstants.TableIds.TakeAway,
                    AjaxUrl = $"/getordersbytype?type={(int)DeliveryOrderTypes.PickUpAtBranch}",
                    SheetName = "_NonDeliveryOrders"
                },
                
                new ()
                {
                    Type = DeliveryOrderTypes.ScheduledPickUp.ToString(),
                    Title = "Scheduled Pick Up",
                    TableId = AppConstants.TableIds.ScheduledPickUp,
                    AjaxUrl = $"/getordersbytype?type={(int)DeliveryOrderTypes.ScheduledPickUp}",
                    SheetName = "_NonDeliveryOrders"
                },
                new()
                {
                    Type = "Dine-in",
                    Title = "Dine-in",
                    TableId = AppConstants.TableIds.DineIn,
                    AjaxUrl = $"/getordersbytype?type={(int)DeliveryOrderTypes.InHouseDining}",
                    SheetName = "_NonDeliveryOrders"
                }
                
            };
        }
            
    }
}