using DeliveryMonitoring.Models;
namespace DeliveryMonitoring.Helpers
{
    public static class SummaryConfigFactory
    {
        public static SummaryTableConfig Create(string type) =>
            (type ?? "").ToLower() switch
            {
                "merchant" => new SummaryTableConfig
                {
                    Type = "merchant",
                    Title = "Merchant Summary",
                    TableId = "merchantSummary",
                    AjaxUrl = $"/summary/data?type={type}",
                    SheetName = "Merchant Summary"
                },
                "driver" => new SummaryTableConfig
                {
                    Type = "driver",
                    Title = "Driver Summary",
                    TableId = "driverSummary",
                    AjaxUrl = $"/summary/data?type={type}",
                    SheetName = "Driver Summary"
                },
                "supervisor" => new SummaryTableConfig
                {
                    Type = "supervisor",
                    Title = "Supervisor Summary",
                    TableId = "supervisorSummary",
                    AjaxUrl = $"/summary/data?type={type}",
                    SheetName = "Supervisor Summary"
                },
                "consignee" or _ => new SummaryTableConfig
                {
                    Type = "consignee",
                    Title = "Consignee Summary",
                    TableId = "consigneeSummary",
                    AjaxUrl = "/summary/data?type=consignee",
                    SheetName = "Consignee Summary"
                }
            };
    }

}
