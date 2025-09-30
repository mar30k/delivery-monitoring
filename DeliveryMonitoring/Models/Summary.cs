namespace DeliveryMonitoring.Models
{
    public abstract class Summary
    {
        public double DineInAmount { get; set; }
        public double TakeawayAmount { get; set; }
        public double DeliveryAmount { get; set; }
        public int TotalDineInOrders { get; set; }
        public int TotalTakeAwayOrders { get; set; }
        public int TotalDeliveryOrders { get; set; }
        public double GrandTotal { get; set; }
    }

    public class ConsigneeSummary : Summary
    {
        public string? PhoneNumber { get; set; }
        public string? Name { get; set; }
        public int TotalMerchantCount { get; set; }
    }

    public class MerchantSummary : Summary
    {
        public string? Tin { get; set; }
        public string? CompanyName { get; set; }
        public string? BranchName { get; set; }
        public int TotalConsigneeCount { get; set; }
    }

}
