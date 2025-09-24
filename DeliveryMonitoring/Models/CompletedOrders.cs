using System.ComponentModel.DataAnnotations;

namespace DeliveryMonitoring.Models
{
    public class CompletedOrders
    {
        public DateTime RequestCreatedAt { get; set; }
        public string? Status { get; set; }
        public string? DriverPhoneNumber { get; set; }
        public string? SupervisorPhoneNumber { get; set; }
        public string? SupervisorName { get; set; }
        public string? FirstName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? VoucherCode { get; set; }
        public double Distance { get; set; }
        public double Eta { get; set; }
        public double TotalAmount { get; set; }
        public double Duration { get; set; }
        public string? Note { get; set; }
        public string? Purpose { get; set; }
        public string? CompanyName { get; set; }
        public string? Tin { get; set; }
        public int CompanyCode { get; set; }
        public int BranchCode { get; set; }
        public string? BranchName { get; set; }
        public string? Review { get; set; }
        public double Rating { get; set; }
        public double Tip { get; set; }
        public string? RequestCreatedAtString { get; set; }
    }

    public class CompletedOrdersViewModel
    {
        public HulubejeResponse<List<CompletedOrders>>? CompletedOrders { get; set; }
        public HulubejeResponse<List<CompletedOrders>>? DineInOders { get; set; }
        public HulubejeResponse<List<CompletedOrders>>? TakeAwayOrders { get; set; }
        public Dictionary<int, string>? PurposeOptions { get; set; }
        public string? CompanyTin { get; set; }
        public string? Type { get; set; }
    }
}