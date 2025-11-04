using DeliveryMonitoring.Constants;
using DeliveryMonitoring.Models;

namespace DeliveryMonitoring.Helpers
{
    public static class TableConfigFactory
    {
        // --- Summary table configs ---
        public static TableConfig CreateSummary(SummaryReportType type) =>
            (type) switch
            {
                SummaryReportType.Merchant => new TableConfig
                {
                    Type = "merchant",
                    Title = "Merchant Summary",
                    TableId = "merchantSummary",
                    AjaxUrl = $"/summary/data?type={type}",
                    SheetName = "Merchant Summary"
                },
                SummaryReportType.Driver => new TableConfig
                {
                    Type = "driver",
                    Title = "Driver Summary",
                    TableId = "driverSummary",
                    AjaxUrl = $"/summary/data?type={type}",
                    SheetName = "Driver Summary"
                },
                SummaryReportType.Supervisor=> new TableConfig
                {
                    Type = "supervisor",
                    Title = "Supervisor Summary",
                    TableId = "supervisorSummary",
                    AjaxUrl = $"/summary/data?type={type}",
                    SheetName = "Supervisor Summary"
                },
                SummaryReportType.Consignee or _ => new TableConfig
                {
                    Type = "consignee",
                    Title = "Consignee Summary",
                    TableId = "consigneeSummary",
                    AjaxUrl = "/summary/data?type=consignee",
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
    }
}