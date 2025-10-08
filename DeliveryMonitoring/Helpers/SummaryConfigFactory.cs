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
                    AjaxUrl = "/summary/data?type=merchant",
                    SheetName = "Merchant Summary"
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
