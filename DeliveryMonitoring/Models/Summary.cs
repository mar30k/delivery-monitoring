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
    public class DriverSummary : Summary
    {
        public string? DriverPhoneNumber { get; set; }
        public string? Name { get; set; }
        public double TotalDistance { get; set; }
        public double Tip { get; set; }
        public double AverageRating { get; set; }
        public double TotalTimeDeviation { get; set; }
        public string? MostOrdersDate { get; set; } // e.g., "Mon Nov 01, 2025"
        public int MostOrdersCount { get; set; }
        public int TotalConsigneeCount { get; set; }
        public int TotalMerchantCount { get; set; }
    }

    public class SupervisorSummary : Summary
    {
        public string? SupervisorPhoneNumber { get; set; }
        public string? SupervisorName { get; set; }
        public List<PurposeItem>? PurposeSummary { get; set; } // Could be HTML or structured string
        public int TotalConsigneeCount { get; set; }
        public int TotalMerchantCount { get; set; }
    }
    public class PurposeItem
    {
        public string Purpose { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Color { get; set; } = "gray";
    }
}
