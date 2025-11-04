namespace DeliveryMonitoring.Models
{
    public class DriverReview
    {
        public int? Count { get; set; }
        public decimal Rating { get; set; }
        public List<Reviews>? Reviews { get; set; }
        public Driver? DriveInfo { get; set; }
    }

    public class Reviews
    {
        public string? Image { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string ReviewerPhoneNumber { get; set; } = string.Empty;
        public bool IsVerifiedUser { get; set; }
        public string Review { get; set; } = string.Empty;
        public string VoucherCode { get; set; } = string.Empty;
        public string ReferenceVoucher { get; set; } = string.Empty;
        public string Attachment { get; set; } = string.Empty;
        public string? Reply { get; set; }
        public decimal Rating { get; set; }
        public DateTime Date { get; set; }
    }
}
