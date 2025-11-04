namespace DeliveryMonitoring.Models
{
    public class TableConfig
    {
        public string Type { get; set; } = "consignee";  // e.g. "merchant" or "consignee"
        public string Title { get; set; } = "";
        public string TableId { get; set; } = "";
        public string AjaxUrl { get; set; } = "";
        public string SheetName { get; set; } = "";
    }
}
